using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace WarpWorld.CrowdControl {
    /// <summary>A Crowd Control effect that handles parameters. </summary>
    public abstract class CCEffectParameters : CCEffectBase {
        /// <summary>List of entries for this parameter effect</summary>
        [HideInInspector] public Dictionary<string, ParameterEntry> ParameterEntries { get; private set; } = new Dictionary<string, ParameterEntry>();

        [SerializeField]
        [HideInInspector]
        [Tooltip("List of entries for this parameter effect")]
        private List<ParameterEntry> m_parameterEntries = new List<ParameterEntry>();
        private List<string> m_parameterStrings = new List<string>();

        [HideInInspector] public string testParameter; 

        protected internal sealed override EffectResult OnTriggerEffect(CCEffectInstance effectInstance)  {
            return OnTriggerEffect(effectInstance as CCEffectInstanceParameters);
        }

        protected abstract EffectResult OnTriggerEffect(CCEffectInstanceParameters effectInstance);

        /// <summary>Function for dynamically adding object(s) to the parameter list. </summary>
        public void AddParameters(params object[] prms)  {
            if (prms.Length == 0) {
                CrowdControl.LogError("You cannot pass in zero parameters!");
                return;
            }

            for (uint i = 0; i < prms.Length; i++) {
                Regex rgx = new Regex("[^a-z0-9-]");
                string effectKey = prms[i].ToString().ToLower();
                string key = rgx.Replace(effectKey, "");

                ParameterEntries.Add(key, new ParameterEntry(key, prms[i].ToString()));
                m_parameterStrings.Add(prms[i].ToString());
            }
        }

        /// <summary>Clearing the established parameter list. </summary>
        public void ClearParameters() {
            ParameterEntries = new Dictionary<string, ParameterEntry>();
            m_parameterEntries = new List<ParameterEntry>();
        }

        /// <summary>All Parameters for this effect as a string.</summary>
        public override string Params() {
            return string.Join(",", m_parameterStrings.ToArray());
        }

        /// <summary>Used for processing the newly received parameter array. Can be overridden by a derived class.</summary>
        public virtual void AssignParameters(string key, string value) {
            foreach (ParameterEntry parameterEntry in ParameterEntries.Values) {
                
            }
        }

        /// <summary> Takes the list of this effect's parameters and adds them to the effect list. </summary>
        public override void RegisterParameters(CCEffectEntries effectEntries) {
            foreach (ParameterEntry entry in m_parameterEntries) {
                Regex rgx = new Regex("[^a-z0-9-]");
                string entryKey = rgx.Replace(entry.Name.ToLower(), "");
                entryKey = $"{effectKey}_{entryKey}";

                entry.SetID(entryKey);
                ParameterEntries.Add(entryKey, entry);

                effectEntries.AddParameter(entryKey, entry.Name, effectKey);
                CrowdControl.instance?.LogFormat("Registered Paramter {0} for {1} with key {2}", entry.Name, displayName, entryKey);

                if (entry.ParamKind != ParameterEntry.Kind.Item)
                    continue;

                entry.InitOptions();

                foreach (ParameterOption option in entry.Options) {
                    effectEntries.AddParameter(option.ID, option.Name, option.ParentID);
                    CrowdControl.instance?.LogFormat("Registered Paramter Options {0} for {1} with key {2}", option.Name, entry.Name, option.ID);
                }
            }
        }

        /// <summary>Returns true if this Parameter Effect is the parent of the parameter ID. Overridable</summary>
        public override bool HasParameterID(string id) {
            return ParameterEntries.ContainsKey(id);
        }
    }
}
