using System.Collections.Generic;
using UnityEngine;

namespace WarpWorld.CrowdControl
{
    /// <summary>A Crowd Control effect that handles parameters. </summary>
    public abstract class CCEffectParameters : CCEffectBase
    {
        /// <summary>List of entries for this parameter effect</summary>
        [HideInInspector] public Dictionary<uint, ParameterEntry> ParameterEntries { get; private set; } = new Dictionary<uint, ParameterEntry>();

        [SerializeField]
        [HideInInspector]
        [Tooltip("List of entries for this parameter effect")]
        private List<ParameterEntry> m_parameterEntries = new List<ParameterEntry>();
        private List<string> m_parameterStrings = new List<string>();

        protected internal sealed override EffectResult OnTriggerEffect(CCEffectInstance effectInstance)
        {
            return OnTriggerEffect(effectInstance as CCEffectInstanceParameters);
        }

        protected abstract EffectResult OnTriggerEffect(CCEffectInstanceParameters effectInstance);

        /// <summary>Function for dynamically adding object(s) to the parameter list. </summary>
        public void AddParameters(params object[] prms)
        {
            if (prms.Length == 0)
            {
                CrowdControl.LogError("You cannot pass in zero parameters!");
                return;
            }

            for (uint i = 0; i < prms.Length; i++)
            {
                uint key = Utils.ComputeMd5Hash(prms[i].ToString() + identifier);
                ParameterEntries.Add(key, new ParameterEntry(key, prms[i].ToString()));
                m_parameterStrings.Add(prms[i].ToString());
            }
        }

        /// <summary>Clearing the established parameter list. </summary>
        public void ClearParameters()
        {
            ParameterEntries = new Dictionary<uint, ParameterEntry>();
            m_parameterEntries = new List<ParameterEntry>();
        }

        /// <summary>All Parameters for this effect as a string.</summary>
        public override string Params()
        {
            return string.Join(",", m_parameterStrings.ToArray());
        }

        /// <summary>Used for processing the newly received parameter array. Can be overridden by a derived class.</summary>
        public virtual void AssignParameters(string[] prms)
        {

        }

        /// <summary> Takes the list of this effect's parameters and adds them to the effect list. </summary>
        public override void RegisterParameters(CCEffectEntries effectEntries)
        {
            foreach (ParameterEntry entry in m_parameterEntries)
            {
                uint key = Utils.ComputeMd5Hash(entry.Name + identifier);
                entry.SetID(key);
                ParameterEntries.Add(key, entry);
                effectEntries.AddParameter(key, entry.Name, identifier, ItemKind.Usable);
                CrowdControl.instance?.Log("Registered Paramter {0} for {1} with key {2}", entry.Name, displayName, key);

                if (entry.ParamKind != ParameterEntry.Kind.Item)
                {
                    continue;
                }

                entry.InitOptions();

                foreach (ParameterOption option in entry.Options)
                {
                    effectEntries.AddParameter(option.ID, option.Name, option.ParentID, ItemKind.Effect);
                    CrowdControl.instance?.Log("Registered Paramter Options {0} for {1} with key {2}", option.Name, entry.Name, option.ID);
                }
            }
        }

        public override bool HasParameterID(uint id)
        {
            return ParameterEntries.ContainsKey(id);
        }
    }
}
