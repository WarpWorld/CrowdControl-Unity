using System.Collections.Generic;

namespace WarpWorld.CrowdControl {
    public class CCEffectInstanceParameters : CCEffectInstance {
        /// <summary>The parameters sent into the effect. Eg: Item Type, Quantity</summary>
        Dictionary<string, JSONEffectRequest.JSONParameterEntry> Parameters;


        public void AssignParameters(Dictionary<string, JSONEffectRequest.JSONParameterEntry> newParams) {
            Parameters = newParams;
        }

        /// <summary>Get the parameter based on it's specific index.</summary>
        public JSONEffectRequest.JSONParameterEntry GetParameter(string id) {
            return Parameters[id];
        }
    }
}
