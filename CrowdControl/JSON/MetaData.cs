using Newtonsoft.JsonCC;
using System;

namespace WarpWorld.CrowdControl {
    [Serializable]
    [Newtonsoft.JsonCC.JsonObject(Title = "meta")]
    internal class MetaData {
        [JsonProperty(PropertyName = "platform")]
        public string Platform = "PC";

        [JsonProperty(PropertyName = "name")]
        public string Name;

        [JsonProperty(PropertyName = "safename")]
        public string SafeName;

        [JsonProperty(PropertyName = "notes")]
        public string Notes;

        [JsonProperty(PropertyName = "connector")]
        public string[] Connector;

        [JsonProperty(PropertyName = "emulator")]
        public string Emulator;

        [JsonProperty(PropertyName = "patch")]
        public bool Patch;

        [JsonProperty(PropertyName = "guide")]
        public string Guide;
    }
}
