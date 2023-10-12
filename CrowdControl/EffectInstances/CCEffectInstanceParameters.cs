using System.Collections.Generic;
using System;

namespace WarpWorld.CrowdControl {
    public class CCEffectInstanceParameters : CCEffectInstance {
        /// <summary>The parameters sent into the effect. Eg: Item Type, Quantity</summary>
        public Dictionary<string, JSONEffectRequest.JSONParameterEntry> Parameters { get; private set; }

        internal void AssignParameters(Dictionary<string, JSONEffectRequest.JSONParameterEntry> newParams) {
            Parameters = newParams;
        }

        /// <summary>Get the current parameter name.</summary>
        public string GetParameter(string id) {
            foreach (string key in Parameters.Keys) {
                if (string.Equals(id, key))
                    return Parameters[key].m_name;

                if (string.Equals((effect as CCEffectParameters).ParameterEntries[key].Name, id))
                    return Parameters[key].m_name;
            }

            return string.Empty;
        }

        /// <summary>Get the parameter based on its quantity value.</summary>
        public uint GetQuantity(string id) {
            foreach (string key in Parameters.Keys) {
                if (string.Equals(id, key))
                    return Convert.ToUInt32(Parameters[key].m_value);

                CCEffectParameters effectParameters = (effect as CCEffectParameters);

                if (effectParameters.ParameterEntries.ContainsKey(key) && string.Equals(effectParameters.ParameterEntries[key].Name, id))
                    return Convert.ToUInt32(Parameters[key].m_value);
            }

            return Convert.ToUInt32(Parameters["quantity"].m_value);
        }
    }
}