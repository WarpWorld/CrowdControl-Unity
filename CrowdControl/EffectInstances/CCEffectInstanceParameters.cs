using System.Collections.Generic;
using System.Linq;
using System;

namespace WarpWorld.CrowdControl {
    public class CCEffectInstanceParameters : CCEffectInstance {
        internal void AssignParameters(Dictionary<string, JSONEffectRequest.JSONParameterEntry> newParams) {
            Parameters = newParams;
        }

        /// <summary>Get the parameter based on it's specific index.</summary>
        public string GetParameter(string id) {
            foreach (string key in Parameters.Keys) {
                if (string.Equals(id, Parameters[key].m_title))
                    return Parameters[key].m_name;
            }

            return string.Empty;
        }

        /// <summary>Get the parameter based on its quantity value.</summary>
        public uint GetQuantity() {
            return Convert.ToUInt32(Parameters["quantity"].m_value.Split('_').Last());
        }
    }
}
