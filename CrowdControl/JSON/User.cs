using Newtonsoft.JsonCC;

namespace WarpWorld.CrowdControl {
    public class Streamer {
        [JsonProperty(PropertyName = "type")]
        public string m_type;

        [JsonProperty(PropertyName = "jti")]
        public string m_jti;

        [JsonProperty(PropertyName = "ccUID")]
        public string m_ccUID;

        [JsonProperty(PropertyName = "originID")]
        public string m_originID;

        [JsonProperty(PropertyName = "profileType")]
        public string m_profileType;

        [JsonProperty(PropertyName = "name")]
        public string m_name;

        [JsonProperty(PropertyName = "subscriptions")]
        public string [] m_subscriptions;

        [JsonProperty(PropertyName = "roles")]
        public string [] m_roles;

        [JsonProperty(PropertyName = "exp")]
        public uint m_exp;

        [JsonProperty(PropertyName = "ver")]
        public string m_ver;
    }
}
