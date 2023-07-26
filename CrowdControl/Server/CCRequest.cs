using System;
using Newtonsoft.JsonCC;
using System.Collections.Generic;

namespace WarpWorld.CrowdControl {
    public enum MessageType {
        Generic = 0xD0,
        JsonBlock = 0xFA,
        Disconnect = 0xFF,
        EffectUpdate = 0x01
    }

    public class CCRequest {
        public enum ResponseType : byte {
            Success = 0x10,
            FailUnsupportedVersion = 0x20,
            FailUnsupportedGame = 0x21,
            BadTemporaryToken = 0x30,
            BadSessionToken = 0x31,
            AlreadyLoggedIn = 0x32,
            BannedUser = 0x33
        };

        protected byte[] byteStream;
        protected ushort size;
        protected ushort offset = 0;
        protected byte checksum = 0;
        public uint blockID;

        public byte[] ByteStream {
            get {
                return byteStream;
            }
        }

        protected void InitBuffer(ushort size) {
            this.size = size;
            byteStream = new byte[size];
            Protocol.Write(byteStream, ref offset, size);
        }

        protected void WriteMessageType(MessageType messageType, ref ushort offset) {
            Protocol.Write(byteStream, ref offset, Convert.ToByte(messageType));
        }

        protected void WriteChecksumByte() {
            uint sum = 0;

            for (int i = 0; i < byteStream.Length; i++)
                sum += byteStream[i];

            checksum = Convert.ToByte(sum % 0x100);

            Protocol.Write(byteStream, ref offset, checksum);
        }

        protected virtual void CreateByteArray() { }
    }

    public class CCMessageGeneric : CCRequest { // 0xD0
        public string genericName;
        public List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

        public ResponseType response;

        protected override void CreateByteArray() {
            ushort size = (ushort)(14 + genericName.Length * 2);

            for (int i = 0; i < parameters.Count; i++) {
                size += (ushort)(parameters[i].Key.Length * 2 + 2);

                if (string.IsNullOrEmpty(parameters[i].Value)) {
                    size += 2;
                    continue;
                }

                size += (ushort)(parameters[i].Value.Length * 2 + 2);
            }

            InitBuffer(size);
            WriteMessageType(MessageType.Generic, ref offset);
            Protocol.Write(byteStream, ref offset, blockID);

            Protocol.Write(byteStream, ref offset, genericName);
            Protocol.Write(byteStream, ref offset, (uint)parameters.Count);

            for (int i = 0; i < parameters.Count; i++) {
                Protocol.Write(byteStream, ref offset, parameters[i].Key);

                if (string.IsNullOrEmpty(parameters[i].Value)) {
                    Protocol.Write(byteStream, ref offset, (ushort)0);
                    continue;
                }

                Protocol.Write(byteStream, ref offset, parameters[i].Value);
            }

            WriteChecksumByte();
        }

        public CCMessageGeneric(uint blockID, string name, KeyValuePair<string, string>[] paramArray) {
            this.blockID = blockID;
            genericName = name;
            parameters = new List<KeyValuePair<string, string>>(paramArray);
            CreateByteArray();
        }

        public CCMessageGeneric(byte[] buffer) {
            int offset = 3;
            Protocol.Read(buffer, ref offset, out blockID);
            Protocol.Read(buffer, ref offset, out genericName);
            Protocol.Read(buffer, ref offset, out int count);

            for (int i = 0; i < count; i++) {
                Protocol.Read(buffer, ref offset, out string key);
                Protocol.Read(buffer, ref offset, out string value);
                parameters.Add(new KeyValuePair<string, string>(key, value));
            }
        }
    }

    public class CCJsonBlock : CCRequest { // 0xFA
        public string jsonString;

        public void CreateByteArray(uint blockID) {
            this.blockID = blockID;
            offset = 0;

            InitBuffer((ushort)(10 + jsonString.Length * 2));
            WriteMessageType(MessageType.JsonBlock, ref offset);
            Protocol.Write(byteStream, ref offset, blockID);
            Protocol.Write(byteStream, ref offset, jsonString);
            WriteChecksumByte();
        }

        private string JSONString(string objectName, object obj) {
            JsonSerializerSettings settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
            string JSONContent = JsonConvert.SerializeObject(obj, settings);

            return $"\"{objectName}\": {JSONContent}";
        }

        public CCJsonBlock(string gameName, Dictionary<string, CCEffectBase> effectList, CCEffectEntries ccEffectEntries) {
            string effectString = "";

            var jsonBlock = new {
                meta = new MetaData {
                    Name = gameName
                },

                effects = new {
                    game = new { }
                }
            };

            int index = 0;

            foreach (string key in effectList.Keys)  {
                CCEffectBase effect = effectList[key];
                EffectJSON effectJSON = new EffectJSON(effect);

                effectString += JSONString(key.ToString(), effectJSON);
                index++;

                if (index < effectList.Keys.Count) 
                    effectString += ",";
            }

            JsonSerializerSettings settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
            string serializedJSON = JsonConvert.SerializeObject(jsonBlock, settings);
            jsonString = serializedJSON.Insert(serializedJSON.IndexOf("\"game\"") + 8, effectString);
        }
    }

    public class CCMessageDisconnect : CCRequest { // 0xFF
        public CCMessageDisconnect(uint blockID) {
            this.blockID = blockID;
            InitBuffer(0x08);
            WriteMessageType(MessageType.Disconnect, ref offset);
            Protocol.Write(byteStream, ref offset, blockID);
            WriteChecksumByte();
        }
    }

    public class CCMessageEffectUpdate : CCRequest { // 0x01
        public CCMessageEffectUpdate(uint blockID, string effectID, Protocol.EffectState status, ushort payload) {
            this.blockID = blockID;
            InitBuffer(Convert.ToUInt16(0x0D + (payload > 0 ? 1 : 0)));
            WriteMessageType(MessageType.EffectUpdate, ref offset);

            Protocol.Write(byteStream, ref offset, blockID);
            Protocol.Write(byteStream, ref offset, effectID);
            Protocol.Write(byteStream, ref offset, Convert.ToByte(status));

            if (payload > 0)
                Protocol.Write(byteStream, ref offset, Convert.ToByte(payload % 0x100));

            WriteChecksumByte();
        }
    }
}
