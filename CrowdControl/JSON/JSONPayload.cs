using Newtonsoft.JsonCC;

namespace WarpWorld.CrowdControl {
    public class JSONPayload {

    }

    internal class JSONData {
        [JsonProperty(PropertyName = "action")]
        public string m_action = "";

        [JsonProperty(PropertyName = "data")]
        public string m_data = "";

        public JSONData(string action, string data = "") {
            m_action = action;
            m_data = data;
        }
    }
}
