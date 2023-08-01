using System.Collections.Generic;
using System.Linq;
using System;

namespace WarpWorld.CrowdControl {
    public class CCEffectInstanceParameters : CCEffectInstance {
        /// <summary>The parameters sent into the effect. Eg: Item Type, Quantity</summary>
        Dictionary<string, JSONEffectRequest.JSONParameterEntry> Parameters;

        public void AssignParameters(Dictionary<string, JSONEffectRequest.JSONParameterEntry> newParams) {
            Parameters = newParams;
        }

        /// <summary>Get the parameter based on it's specific index.</summary>
        public string GetParameter(string id) {
            CCEffectParameters effectParams = effect as CCEffectParameters;

            string keyID = effectParams.ParameterEntries.Keys.FirstOrDefault(entryID =>
                                    string.Equals(id, effectParams.ParameterEntries[entryID].Name));

            return effectParams.ParameterEntries[keyID]?.GetOptionName(Parameters[keyID].m_value);
        }

        /// <summary>Get the parameter based on its quantity value.</summary>
        public uint GetParameterQuantity(string id) {
            CCEffectParameters effectParams = effect as CCEffectParameters;

            string keyID = effectParams.ParameterEntries.Keys.FirstOrDefault(entryID =>
                                    string.Equals(id, effectParams.ParameterEntries[entryID].Name));

            return Convert.ToUInt32(Parameters[keyID].m_value.Split('_').Last());
        }
    }
}
