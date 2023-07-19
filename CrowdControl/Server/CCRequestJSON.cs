using Newtonsoft.JsonCC;
using System.Collections.Generic;

namespace WarpWorld.CrowdControl {
    public class CCRequestJSON {
        [JsonProperty(PropertyName = "game")]
        public string game = CrowdControl.GameKey;

        [JsonProperty(PropertyName = "channel")]
        public string channel = "";

        [JsonProperty(PropertyName = "token")]
        public string token = CrowdControl.instance.CurrentToken;

        [JsonProperty(PropertyName = "blockID")]
        public uint blockID;

        [JsonProperty(PropertyName = "checksum")]
        protected byte checksum;

        [JsonProperty(PropertyName = "messagetype")]
        protected byte messageType;

        [JsonProperty(PropertyName = "responsetype")]
        protected byte responseType;

        protected JsonSerializerSettings settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

        public string SerializedString() { return JsonConvert.SerializeObject(this, settings); }
    }

    public class CCMessageGenericJSON : CCRequestJSON { // 0xD0
        [JsonProperty(PropertyName = "event")]
        public string genericEvent;

        [JsonProperty(PropertyName = "data")]
        public object genericData;

        public CCMessageGenericJSON(uint blockID, string name, params string [] paramArray) {
            if (paramArray.Length % 2 > 0) {
                CrowdControl.LogError("There is an uneven amount of parameters!");
                return;
            }

            messageType = 0xd0;
            genericEvent = name;

            string entryList = "{";

            for (int i = 0; i < paramArray.Length; i+= 2) {
                entryList += $"\"{paramArray[i]}\":\"{paramArray[i + 1]}\"";

                if (i < paramArray.Length - 3) {
                    entryList += ",\n";
                }
            }
            entryList += "}";
            genericData = JsonConvert.DeserializeObject(entryList);
        }
    }

    public class CCMessageVersionJSON : CCRequestJSON { // 0xF0
        [JsonProperty(PropertyName = "version")]
        private uint version;

        [JsonProperty(PropertyName = "config")]
        private uint config;

        [JsonProperty(PropertyName = "fingerPrint")]
        private ulong fingerPrint;
    }

    public class CCMessageTokenAquisitionJSON : CCRequestJSON { // 0xF1
        [JsonProperty(PropertyName = "greeting")]
        public byte greeting;

        [JsonProperty(PropertyName = "tempToken")]
        public string tempToken;
    }

    public class CCMessageTokenHandshakeJSON : CCRequestJSON { // 0xF2
        [JsonProperty(PropertyName = "streamerID")]
        public string streamerID;

        [JsonProperty(PropertyName = "streamerName")]
        public string streamerName;

        [JsonProperty(PropertyName = "streamerIconURL")]
        public string streamerIconURL;

        [JsonProperty(PropertyName = "greeting")]
        public byte greeting;
    }

    public class CCJsonBlockJSON : CCRequestJSON { // 0xFA
        
    }

    public class CCMessagePingJSON : CCRequestJSON  { // 0xFB
    }

    public class CCMessageBlockErrorJSON : CCRequestJSON { // 0xFD
    }

    public class CCMessageUserMessageJSON : CCRequestJSON { // 0xFE
        [JsonProperty(PropertyName = "receivedMessage")]
        public string receivedMessage;
    }

    public class CCMessageDisconnectJSON : CCRequestJSON { // 0xFF
    }

    public class CCMessageEffectRequestJSONSend : CCRequestJSON {
        [JsonProperty(PropertyName = "effectID")]
        public uint effectID;

        [JsonProperty(PropertyName = "status")]
        public byte status;

        [JsonProperty(PropertyName = "time")]
        public ushort time;

        [JsonProperty(PropertyName = "message")]
        public string message;
    }

    public class CCMessageEffectRequestJSONGet : CCRequestJSON {
        public class Viewer {
            [JsonProperty(PropertyName = "displayName")]
            public readonly string displayName;

            [JsonProperty(PropertyName = "iconURL")]
            public readonly string iconURL;

            public Viewer(string displayName, string iconURL = "") {
                this.displayName = displayName;
                this.iconURL = iconURL;
            }
        }

        [JsonProperty(PropertyName = "effectID")]
        public uint effectID;

        [JsonProperty(PropertyName = "durationTime")]
        public uint durationTime;

        [JsonProperty(PropertyName = "viewerCount")]
        public uint viewerCount;

        [JsonProperty(PropertyName = "viewers")]
        public Viewer [] viewers;

        [JsonProperty(PropertyName = "parameters")]
        public string parameters;
    }

    public class CCMessageEffectUpdateJSON : CCRequestJSON { // 0x01
        [JsonProperty(PropertyName = "effectID")]
        public uint effectID;

        [JsonProperty(PropertyName = "status")]
        public byte status;

        [JsonProperty(PropertyName = "payload")]
        public byte payload;
    }
}
