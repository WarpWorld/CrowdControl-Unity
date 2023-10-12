using System.Collections.Generic;
using UnityEngine;

namespace WarpWorld.CrowdControl {
    /// <summary>A Crowd Control effect that handles parameters. </summary>
    public abstract class CCEffectParameters : CCEffectBase {
        /// <summary>List of entries for this parameter effect</summary>
        [HideInInspector] public Dictionary<string, ParameterEntry> ParameterEntries { get; private set; } = new Dictionary<string, ParameterEntry>();

        [SerializeField]
        [HideInInspector]
        private List<ParameterEntry> m_parameterEntries = new List<ParameterEntry>();

        protected internal sealed override EffectResult OnTriggerEffect(CCEffectInstance effectInstance)  {
            return OnTriggerEffect(effectInstance as CCEffectInstanceParameters);
        }

        protected abstract EffectResult OnTriggerEffect(CCEffectInstanceParameters effectInstance);

        internal void RegisterParameters() {
            ParameterEntries = new Dictionary<string, ParameterEntry>();

            foreach (ParameterEntry entry in m_parameterEntries) {
                entry.SetID(ID);
                ParameterEntries.Add(entry.ID, entry);
            }
        }

        /// <summary>Returns true if this Parameter Effect is the parent of the parameter ID. Overridable</summary>
        public override bool HasParameterID(string id) {
            return ParameterEntries.ContainsKey(id);
        }
    }
}
