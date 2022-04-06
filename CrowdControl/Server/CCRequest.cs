using System;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace WarpWorld.CrowdControl
{
    public enum MessageType 
    {
        Version = 0xF0,
        TokenAquisition = 0xF1,
        TokenHandshake = 0xF2,
        JsonBlock = 0xFA,
        Ping = 0xFB,
        BlockError = 0xFD,
        UserMessage = 0xFE,
        Disconnect = 0xFF,
        EffectRequest = 0x00,
        EffectUpdate = 0x01
    }

    public class CCRequest
    {
        public enum ResponseType : byte
        {
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

        public byte [] ByteStream
        {
            get
            {
                return byteStream;
            }
        }

        protected void InitBuffer(ushort size)
        {
            this.size = size;
            byteStream = new byte[size];
            Protocol.Write(byteStream, ref offset, size);
        }

        protected void WriteMessageType(MessageType messageType, ref ushort offset)
        {
            Protocol.Write(byteStream, ref offset, Convert.ToByte(messageType));
        }

        protected void WriteChecksumByte()
        {
            
            uint sum = 0;

            for (int i = 0; i < byteStream.Length; i++)
                sum += byteStream[i];

            checksum = Convert.ToByte(sum % 0x100);

            Protocol.Write(byteStream, ref offset, checksum);
        }

        protected virtual void CreateByteArray()
        {

        }
    }

    public class CCMessageVersion : CCRequest // 0xF0
    {
        private uint version;
        private uint gameID;
        private uint config;
        private ulong fingerPrint;

        public ResponseType response;

        protected override void CreateByteArray()
        {
            InitBuffer(0x1C);
            WriteMessageType(MessageType.Version, ref offset);

            Protocol.Write(byteStream, ref offset, blockID);
            Protocol.Write(byteStream, ref offset, version);
            Protocol.Write(byteStream, ref offset, config);
            Protocol.Write(byteStream, ref offset, gameID);
            Protocol.Write(byteStream, ref offset, fingerPrint);

            WriteChecksumByte();
        }

        public CCMessageVersion(byte [] buffer)
        {
            int offset = 3;
            Protocol.Read(buffer, ref offset, out blockID);
            Protocol.Read(buffer, ref offset, out byte responseByte);
            Protocol.Read(buffer, ref offset, out checksum);
            response = (ResponseType)responseByte;
        }

        public CCMessageVersion(uint blockID, uint version, uint gameID, ulong fingerPrint)
        {
            this.blockID = blockID;
            this.version = version;
            config = 0; // Temp
            this.gameID = gameID;

            this.fingerPrint = fingerPrint;
            CreateByteArray();
        }
    }

    public class CCMessageTokenAquisition : CCRequest // 0xF1
    {
        private string token;

        public Greeting greeting;
        public UUID uniqueToken;

        protected override void CreateByteArray()
        {
            InitBuffer(Convert.ToUInt16(token.Length * 2 + 2 + 8));
            WriteMessageType(MessageType.TokenAquisition, ref offset);
            Protocol.Write(byteStream, ref offset, blockID);
            Protocol.Write(byteStream, ref offset, token);

            WriteChecksumByte();
        }

        public CCMessageTokenAquisition(byte [] buffer)
        {
            int offset = 3;
            Protocol.Read(buffer, ref offset, out blockID);
            greeting = (Greeting)buffer[offset++];

            if (greeting != Greeting.Success)
            {
                return;
            }

            uniqueToken = new UUID();
            Protocol.Read(buffer, ref offset, out uniqueToken.a);
            Protocol.Read(buffer, ref offset, out uniqueToken.b);
            Protocol.Read(buffer, ref offset, out uniqueToken.c);
            Protocol.Read(buffer, ref offset, out uniqueToken.d);
            Protocol.Read(buffer, ref offset, out uniqueToken.e);
            Protocol.Read(buffer, ref offset, out uniqueToken.f);
            Protocol.Read(buffer, ref offset, out uniqueToken.g);
            Protocol.Read(buffer, ref offset, out uniqueToken.h);
            Protocol.Read(buffer, ref offset, out uniqueToken.i);
            Protocol.Read(buffer, ref offset, out uniqueToken.j);
            Protocol.Read(buffer, ref offset, out uniqueToken.k);
        }

        public CCMessageTokenAquisition(uint blockID, string token)
        {
            this.blockID = blockID;
            this.token = token;
            UnityEngine.Debug.LogError(token);
            CreateByteArray();
        }
    }

    public class CCMessageTokenHandshake : CCRequest // 0xF2
    {
        UUID token;
        public ResponseType response;
        public string streamerID;
        public string streamerName;
        public string streamerIconURL;
        public BroadcasterType broadcasterType;
        public Greeting greeting;
 

        protected override void CreateByteArray()
        {
            InitBuffer(0x19);
            WriteMessageType(MessageType.TokenHandshake, ref offset);

            Protocol.Write(byteStream, ref offset, blockID);
            Protocol.Write(byteStream, ref offset, token);
            Protocol.Write(byteStream, ref offset, Convert.ToByte(1)); // Options

            WriteChecksumByte();
        }

        public CCMessageTokenHandshake(byte [] buffer)
        {
            int offset = 3;
            Protocol.Read(buffer, ref offset, out blockID);
            greeting = (Greeting)buffer[offset++];
            Protocol.Read(buffer, ref offset, out streamerID);
            Protocol.Read(buffer, ref offset, out streamerName);
            Protocol.Read(buffer, ref offset, out streamerIconURL);
            broadcasterType = (BroadcasterType)buffer[offset++];
        }

        public CCMessageTokenHandshake(uint blockID, UUID token)
        {
            this.token = token;
            CreateByteArray();
        }
    }

    public class CCJsonBlock : CCRequest // 0xFA
    {
        public List<string> jsonStrings = new List<string>();

        public void CreateByteArray(uint blockID, int index)
        {
            this.blockID = blockID;
            offset = 0;

            InitBuffer(Convert.ToUInt16(jsonStrings[index].Length * 2 + 10));
            WriteMessageType(MessageType.JsonBlock, ref offset);
            Protocol.Write(byteStream, ref offset, blockID);
            Protocol.Write(byteStream, ref offset, jsonStrings[index]);

            UnityEngine.Debug.LogError(jsonStrings[index]);
            WriteChecksumByte();
        }

        public CCJsonBlock(string gameName, Dictionary<uint, CCEffectBase> effectList)
        {
            const int PADDING = 0;
            const int BLOCK_OVERHEAD = 10;
            const int MAX_LENGTH = (0x10000 - (BLOCK_OVERHEAD + PADDING)) / 2; //we take half because it's UTF16

            Queue<string> segments = new Queue<string>();

            List<ItemType> itemTypes = new List<ItemType>();

            foreach (uint key in effectList.Keys)
            {
                CCEffectBase effect = effectList[key];

                if (effect is CCEffectParameters)
                {
                    foreach (ParameterEntry entry in (effect as CCEffectParameters).ParameterEntries.Values)
                    {
                        itemTypes.Add(new ItemType(entry));
                    }
                }
            }

            MenuData append = new MenuData() { LoadType = MenuData.MenuLoadType.Append };
            Queue<EffectDescription> remaining = new Queue<EffectDescription>();
            JsonSerializerSettings settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

            string json = JsonConvert.SerializeObject(new MenuData
            {
                GameName = gameName,
                ItemTypes = itemTypes,
                LoadType = MenuData.MenuLoadType.Overwrite
            }, settings);

            segments.Enqueue(json);

            foreach (uint key in effectList.Keys)
            {
                CCEffectBase effect = effectList[key];
                EffectDescription effectDesc = new EffectDescription(key, effect);
                remaining.Enqueue(effectDesc);

                if (effect is CCEffectBidWar)
                {
                    foreach (BidWarEntry entry in (effect as CCEffectBidWar).BidWarEntries.Values)
                    {
                        remaining.Enqueue(new EffectDescription(entry, effect, ItemKind.BidWarValue));
                    }

                    continue;
                }

                if (effect is CCEffectParameters)
                {
                    foreach (ParameterEntry entry in (effect as CCEffectParameters).ParameterEntries.Values)
                    {
                        remaining.Enqueue(new EffectDescription(entry, effect, ItemKind.Usable));

                        entry.InitOptions();

                        foreach (ParameterOption option in entry.Options)
                        {
                            remaining.Enqueue(new EffectDescription(option));
                        }
                    }
                }
            }

            while (remaining.Count > 0)
            {
                EffectDescription currentDescription = remaining.Dequeue();

                if (currentDescription.Kind == ItemKind.Usable && !currentDescription.Parameter)
                {
                    continue;
                }

                append.Items.Add(currentDescription);
                json = JsonConvert.SerializeObject(append, settings);

                if (json.Length > MAX_LENGTH)
                {
                    if (append.Items.Count <= 1)
                    {
                        if (string.IsNullOrEmpty(currentDescription.Description?.Trim()))
                        {
                            throw new SerializationException("Cannot auto-serialize this menu into 64k blocks.");
                        }

                        currentDescription.EraseDescription();
                    }
                    append.Items.Remove(currentDescription);
                    remaining.Enqueue(currentDescription);

                    json = JsonConvert.SerializeObject(append, settings);
                    append.Items.Clear();

                    segments.Enqueue(json);
                }
            }

            if (append.Items.Count > 0)
            {
                json = JsonConvert.SerializeObject(append, settings);
                segments.Enqueue(json);
            }

            while (segments.Count > 0)
            {
                jsonStrings.Add(segments.Dequeue());
            }
        }
    }

    public class CCMessagePing : CCRequest // 0xFB
    {
        protected override void CreateByteArray()
        {
            InitBuffer(8);
            WriteMessageType(MessageType.Ping, ref offset);
            Protocol.Write(byteStream, ref offset, blockID);
            WriteChecksumByte();
        }

        public CCMessagePing(uint blockID)
        {
            this.blockID = blockID;
            CreateByteArray();
        }
    }

    public class CCMessageBlockError : CCRequest // 0xFD
    {
        public CCMessageBlockError(uint blockID)
        {
            this.blockID = blockID;
            //InitBuffer(?);
            WriteMessageType(MessageType.BlockError, ref offset);

            Protocol.Write(byteStream, ref offset, blockID);

            WriteChecksumByte();
        }

        public CCMessageBlockError(byte[] buffer)
        {
            int offset = 3;
            Protocol.Read(buffer, ref offset, out blockID);
        }
    }

    public class CCMessageUserMessage : CCRequest // 0xFE
    {
        public string receivedMessage;

        public CCMessageUserMessage(byte[] buffer)
        {
            int offset = 3;
            Protocol.Read(buffer, ref offset, out blockID);
            Protocol.Read(buffer, ref offset, out receivedMessage);
        }

        public CCMessageUserMessage(uint blockID)
        {
            this.blockID = blockID;
            //InitBuffer(?);
            WriteMessageType(MessageType.UserMessage, ref offset);

            Protocol.Write(byteStream, ref offset, blockID);

            WriteChecksumByte();
        }
    }

    public class CCMessageDisconnect : CCRequest // 0xFF
    {
        public CCMessageDisconnect(uint blockID)
        {
            this.blockID = blockID;
            InitBuffer(0x08);
            WriteMessageType(MessageType.Disconnect, ref offset);
            Protocol.Write(byteStream, ref offset, blockID);
            WriteChecksumByte();
        }
    }

    public class CCMessageEffectRequest : CCRequest // 0x00
    {
        public class Viewer
        {
            public readonly string displayName;
            public readonly string iconURL;

            public Viewer(string displayName, string avatarURL = "")
            {
                this.displayName = displayName;
                this.iconURL = avatarURL;
            }

            public static implicit operator Viewer(string displayName) => new Viewer(displayName, string.Empty);
        }

        public uint effectID;
        public Viewer[] viewers;
        public string parameters;

        public int viewerCount = 0;

        public CCMessageEffectRequest(byte[] buffer)
        {
            int offset = 3;
            Protocol.Read(buffer, ref offset, out blockID);
            Protocol.Read(buffer, ref offset, out effectID);
            Protocol.Read(buffer, ref offset, out viewerCount);

            viewers = new Viewer[viewerCount];

            for (int i = 0; i < viewerCount; i++)
            {
                Protocol.Read(buffer, ref offset, out string displayName);
                Protocol.Read(buffer, ref offset, out string iconURL);
                viewers[i] = new Viewer(displayName, iconURL);
            }

            Protocol.Read(buffer, ref offset, out parameters);
            Protocol.Read(buffer, ref offset, out checksum);
        }

        public CCMessageEffectRequest(uint blockID, uint effectID, Protocol.EffectState status, ushort time, string message = "")
        {
            this.blockID = blockID;

            bool usesTime = status == Protocol.EffectState.TimedEffectBegin || status == Protocol.EffectState.ExactDelay || status == Protocol.EffectState.DelayedSuccess ||
                            status == Protocol.EffectState.EstimatedDelay || status == Protocol.EffectState.TimedResume;

            ushort messageSize = 0x0F;
            
            if (usesTime)
            {
                messageSize += 2;
            }

            InitBuffer(messageSize);
            WriteMessageType(MessageType.EffectRequest, ref offset);

            Protocol.Write(byteStream, ref offset, blockID);
            Protocol.Write(byteStream, ref offset, effectID);
            Protocol.Write(byteStream, ref offset, Convert.ToByte(status));

            if (usesTime)
            {
                Protocol.Write(byteStream, ref offset, time);
            }

            Protocol.Write(byteStream, ref offset, message);
            WriteChecksumByte();
        }
    }

    public class CCMessageEffectUpdate : CCRequest // 0x01
    {
        public CCMessageEffectUpdate(uint blockID, uint effectID, Protocol.EffectState status, ushort payload)
        {
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
