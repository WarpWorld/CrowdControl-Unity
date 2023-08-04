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
            //Protocol.Write(byteStream, ref offset, size);
        }

        protected void WriteMessageType(MessageType messageType, ref ushort offset) {
            //Protocol.Write(byteStream, ref offset, Convert.ToByte(messageType));
        }

        protected void WriteChecksumByte() {
            uint sum = 0;

            for (int i = 0; i < byteStream.Length; i++)
                sum += byteStream[i];

            checksum = Convert.ToByte(sum % 0x100);

            //Protocol.Write(byteStream, ref offset, checksum);
        }

        protected virtual void CreateByteArray() { }
    }

    public class CCJsonBlock : CCRequest { // 0xFA
        public string jsonString;

        public void CreateByteArray(uint blockID) {
            this.blockID = blockID;
            offset = 0;

            InitBuffer((ushort)(10 + jsonString.Length * 2));
            WriteMessageType(MessageType.JsonBlock, ref offset);
            //Protocol.Write(byteStream, ref offset, blockID);
            //Protocol.Write(byteStream, ref offset, jsonString);
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
                CrowdControl.Log(key);
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
}
