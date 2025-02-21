﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.JsonCC;

namespace WarpWorld.CrowdControl {
    internal class JSONMessageGet {
        [JsonProperty(PropertyName = "domain")]
        public string m_domain;

        [JsonProperty(PropertyName = "type")]
        public string m_type;

        [JsonProperty(PropertyName = "payload")]
        public object m_payload;

        [JsonProperty(PropertyName = "timestamp")]
        public ulong m_timestamp;
    }

    internal class JSONWebMessageGet {
        internal class JSONWebMessageGetResult {
            [JsonProperty(PropertyName = "data")]
            public object m_data;
        }

        [JsonProperty(PropertyName = "result")]
        public JSONWebMessageGetResult m_result;
    }

    internal class JSONMessageSend {
        [JsonProperty(PropertyName = "action")]
        public string m_action;

        [JsonProperty(PropertyName = "data")]
        public JSONData m_data;

        public JSONMessageSend(string action) {
            m_action = action;
        }
    }

    internal class JSONWhoAmI : JSONPayload {
        [JsonProperty(PropertyName = "connectionID")]
        public string m_connectionID;
    }

    internal class JSONLoginSuccess : JSONPayload {
        [JsonProperty(PropertyName = "token")]
        public string m_token;

        public string m_decodedToken = "";

        internal string DecodeToken() {
            string[] segments = m_token.Split('.');

            while (segments[1].Length % 4 > 0) {
                segments[1] += "=";
            }

            byte[] data = Convert.FromBase64String(segments[1]);
            m_decodedToken = Encoding.UTF8.GetString(data);
            return m_decodedToken;
        }
    }

    internal class JSONSubResult : JSONPayload {
        [JsonProperty(PropertyName = "success")]
        public string [] m_success;

        [JsonProperty(PropertyName = "failure")]
        public string[] m_failure;
    }

    internal class JSONGameSession : JSONPayload {
        [JsonProperty(PropertyName = "gameSessionID")]
        public string m_gameSessionID;
    }

    public class JSONEffectRequest : JSONPayload {
        [JsonProperty(PropertyName = "effectRequest")]
        public JSONEffectBody m_effectRequest;

        public class JSONEffectBody {
            [JsonProperty(PropertyName = "requestID")]
            public string m_requestID;

            [JsonProperty(PropertyName = "gameSessionID")]
            public string m_gameSessionID;

            [JsonProperty(PropertyName = "bankID")]
            public string m_bankID;

            [JsonProperty(PropertyName = "createdAt")]
            public string m_createdAt;

            [JsonProperty(PropertyName = "updatedAt")]
            public string m_updatedAt;

            [JsonProperty(PropertyName = "type")]
            public string m_type;

            [JsonProperty(PropertyName = "target")]
            public JSONUser m_target;

            [JsonProperty(PropertyName = "requester")]
            public JSONUser m_requester;

            [JsonProperty(PropertyName = "game")]
            public JSONGame m_game;

            [JsonProperty(PropertyName = "gamePack")]
            public JSONGamePack m_gamePack;

            [JsonProperty(PropertyName = "effect")]
            public JSONEffect m_effect;

            [JsonProperty(PropertyName = "unitPrice")]
            public uint m_unitPrice;

            [JsonProperty(PropertyName = "isTest")]
            public bool m_isTest;

            [JsonProperty(PropertyName = "parameters")]
            public Dictionary<string, JSONParameterEntry> m_parameters;

            [JsonProperty(PropertyName = "status")]
            public string m_status;

            [JsonProperty(PropertyName = "quantity")]
            public uint m_quantity = 0;
        }

        public class JSONEffect {
            [JsonProperty(PropertyName = "effectID")]
            public string m_effectID;

            [JsonProperty(PropertyName = "type")]
            public string m_type;

            [JsonProperty(PropertyName = "name")]
            public string m_name;

            [JsonProperty(PropertyName = "note")]
            public string m_note;

            [JsonProperty(PropertyName = "category")]
            public string [] m_category;

            [JsonProperty(PropertyName = "description")]
            public string m_description;

            [JsonProperty(PropertyName = "price")]
            public uint m_price;

            [JsonProperty(PropertyName = "parameters")]
            public Dictionary<string, JSONParameters> m_parameters;

            [JsonProperty(PropertyName = "image")]
            public string m_image;
        }

        public class JSONParameters {
            [JsonProperty(PropertyName = "name")]
            public string m_name;

            [JsonProperty(PropertyName = "type")]
            public string m_type;

            [JsonProperty(PropertyName = "options")]
            public Dictionary<string, JSONParameterEntry> m_options;
        }

        public class JSONParameterEntry {
            [JsonProperty(PropertyName = "name")]
            public string m_name;

            [JsonProperty(PropertyName = "title")]
            public string m_title;

            [JsonProperty(PropertyName = "type")]
            public string m_type;

            [JsonProperty(PropertyName = "value")]
            public string m_value;
        }

        public class JSONGame {
            [JsonProperty(PropertyName = "gameID")]
            public string m_gameID;

            [JsonProperty(PropertyName = "name")]
            public string m_name;
        }

        public class JSONGamePack {
            [JsonProperty(PropertyName = "gamePackID")]
            public string m_gamePackID;

            [JsonProperty(PropertyName = "name")]
            public string m_name;

            [JsonProperty(PropertyName = "platform")]
            public string m_platform;
        }

        public class JSONUser {
            [JsonProperty(PropertyName = "ccUID")]
            public string m_ccUID;

            [JsonProperty(PropertyName = "name")]
            public string m_name;

            [JsonProperty(PropertyName = "profile")]
            public string m_profile;

            [JsonProperty(PropertyName = "originID")]
            public string m_originID;

            [JsonProperty(PropertyName = "subscriptions")]
            public string [] m_subscriptions;

            [JsonProperty(PropertyName = "roles")]
            public string [] m_roles;

            [JsonProperty(PropertyName = "image")]
            public string m_image;
        }
    }

    internal class JSONEffectSuccess : JSONPayload {
        [JsonProperty(PropertyName = "gameSessionID")]
        public string[] m_gameSessionID;
    }

    internal class JSONSubscribe {
        [JsonProperty(PropertyName = "token")]
        public string token = "";

        [JsonProperty(PropertyName = "topics")]
        public string[] topics;

        public JSONSubscribe(string hash) {
            token = hash;
            topics = new string[3];
            topics[0] = "session/self";
            topics[1] = "prv/self";
            topics[2] = "pub/self"; 
        }
    }

    internal class JSONStartSession {
        [JsonProperty(PropertyName = "gamePackID")]
        public string m_gamePackID = "";

        [JsonProperty(PropertyName = "effectReportArgs")]
        public string[] m_effectReportArgs;

        public JSONStartSession(string gamePackID, params string [] effetReportArgs) {
            m_gamePackID = gamePackID;
            m_effectReportArgs = effetReportArgs;
        }
    }

    internal class JSONEffectOverride<T> {
        [JsonProperty(PropertyName = "gamePackID")]
        public string m_gamePackID = CrowdControl.GameID;

        [JsonProperty(PropertyName = "effectOverrides")]
        public JSONEffectOverrideEntry[] m_effectOverrides = new JSONEffectOverrideEntry[1];

        internal class JSONEffectOverrideEntry {
            [JsonProperty(PropertyName = "effectID")]
            public string m_effectID = "";

            [JsonProperty(PropertyName = "type")]
            public string m_type = "game";

            public virtual void Set(string effectID, T value) { }
        }
    }

    internal class JSONEffectChangePrice : JSONEffectOverride<uint> {
        internal class JSONEffectPriceEntry : JSONEffectOverrideEntry {
            [JsonProperty(PropertyName = "price")]
            public uint m_price = 0;

            public override void Set(string effectID, uint price) {
                m_effectID = effectID;
                m_price = price;
            }
        }

        public JSONEffectChangePrice(string effectID, uint value) {
            m_effectOverrides[0] = new JSONEffectPriceEntry();
            m_effectOverrides[0].Set(effectID, value);
        }
    }

    internal class JSONEffectChangeNonPoolable : JSONEffectOverride<bool> {
        internal class JSONEffectPriceEntry : JSONEffectOverride<bool>.JSONEffectOverrideEntry {
            [JsonProperty(PropertyName = "inactive")]
            public bool m_unPoolable;

            public override void Set(string effectID, bool unPoolable) {
                m_effectID = effectID;
                m_unPoolable = unPoolable;
            }
        }

        public JSONEffectChangeNonPoolable(string effectID, bool value) {
            m_effectOverrides[0] = new JSONEffectPriceEntry();
            m_effectOverrides[0].Set(effectID, value);
        }
    }

    internal class JSONEffectChangeSessionMax : JSONEffectOverride<uint> {
        internal class JSONEffectSessionEntry : JSONEffectOverride<uint>.JSONEffectOverrideEntry {
            [JsonProperty(PropertyName = "sessionMax")]
            public uint m_sessionMax = 0;

            public override void Set(string effectID, uint max) {
                m_effectID = effectID;
                m_sessionMax = max;
            }
        }

        public JSONEffectChangeSessionMax(string effectID, uint value) {
            m_effectOverrides[0] = new JSONEffectSessionEntry();
            m_effectOverrides[0].Set(effectID, value);
        }
    }

    internal class JSONStopSession {
        [JsonProperty(PropertyName = "gameSessionID")]
        public string m_gameSessionID = "";

        public JSONStopSession(string gameSessionID) {
            m_gameSessionID = gameSessionID;
        }
    }

    internal class JSONRequestUser {
        [JsonProperty(PropertyName = "token")]
        public string m_token = "";
    }

    internal class JSONRequestEffect {
        [JsonProperty(PropertyName = "gameSessionID")]
        public string m_gameSessionID;

        [JsonProperty(PropertyName = "sourceDetails")]
        public JSONRequestSourceDetails m_sourceDetails;

        internal class JSONRequestSourceDetails {
            [JsonProperty(PropertyName = "type")]
            public string m_type = "crowd-control-test";
        }

        [JsonProperty(PropertyName = "effectType")]
        public string m_effectType = "game";

        [JsonProperty(PropertyName = "effectID")]
        public string m_effectID;

        /*[JsonProperty(PropertyName = "parameters")]
        Dictionary<string, string> parameters = new Dictionary<string, string>();*/

        public JSONRequestEffect(string gameSessionID, string effectID, params string [] paramterList) {
            m_gameSessionID = gameSessionID;
            m_effectID = effectID;

            m_sourceDetails = new JSONRequestSourceDetails();

            for (int i = 0; i < paramterList.Length; i += 2) {
                //parameters.Add(paramterList[i], paramterList[i + 1]);
            }
        }
    }

    internal class JSONEffectReport {
        internal class JSONEffectReportArgs {
            [JsonProperty(PropertyName = "id")]
            public string m_id = "";

            [JsonProperty(PropertyName = "status")]
            public string m_status;

            [JsonProperty(PropertyName = "stamp")]
            public long m_stamp = 1;

            [JsonProperty(PropertyName = "ids")]
            public string[] m_ids;

            [JsonProperty(PropertyName = "effectType")]
            public string m_effectType = "game";

            [JsonProperty(PropertyName = "identifierType")]
            public string m_identifierType = "effect";

            public JSONEffectReportArgs(string status, params string[] ids) {
                m_status = status;
                m_ids = ids; 
                m_id = Utils.GenerateRandomString(26);

                var now = DateTime.UtcNow;
                var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var unixTimestamp = (long)(now - unixEpoch).TotalSeconds;
                m_stamp = unixTimestamp;
            }
        }

        [JsonProperty(PropertyName = "token")]
        public string m_token = "";

        [JsonProperty(PropertyName = "call")]
        public JSONReportCall m_call = new JSONReportCall();

        internal class JSONReportCall {
            [JsonProperty(PropertyName = "method")]
            public string m_method = "effectReport";

            [JsonProperty(PropertyName = "args")]
            public JSONEffectReportArgs[] m_args = new JSONEffectReportArgs[1];

            [JsonProperty(PropertyName = "id")]
            public string m_id = "string";

            [JsonProperty(PropertyName = "type")]
            public string m_type = "call";
        }

        public JSONEffectReport(string token, CCEffectBase effect, string status) {
            m_token = token;
            m_call.m_args[0] = new JSONEffectReportArgs(status, effect.ID);
        }
    }

    public class JSONUserInfo {
        [JsonProperty(PropertyName = "profile")]
        public JSONUserInfoProfile m_profile;

        public class JSONUserInfoProfile {
            [JsonProperty(PropertyName = "createdAt")]
            public string m_createdAt;

            [JsonProperty(PropertyName = "name")]
            public string m_name;

            [JsonProperty(PropertyName = "originID")]
            public string m_originID;

            [JsonProperty(PropertyName = "image")]
            public string m_image;

            [JsonProperty(PropertyName = "ccUID")]
            public string m_ccUID;

            [JsonProperty(PropertyName = "originData")]
            public JSONUserOriginData m_originData;

            public class JSONUserOriginData {
                [JsonProperty(PropertyName = "_type")]
                public string m_type;

                [JsonProperty(PropertyName = "user")]
                public JSONUserOriginUserData m_user;

                public class JSONUserOriginUserData {
                    [JsonProperty(PropertyName = "offline_image_url")]
                    public string m_offline_image_url;

                    [JsonProperty(PropertyName = "description")]
                    public string m_description;

                    [JsonProperty(PropertyName = "created_at")]
                    public string m_createdAt;

                    [JsonProperty(PropertyName = "profile_image_url")]
                    public string m_profile_image_url;

                    [JsonProperty(PropertyName = "id")]
                    public string m_id;

                    [JsonProperty(PropertyName = "login")]
                    public string m_login;

                    [JsonProperty(PropertyName = "display_name")]
                    public string m_display_name;

                    [JsonProperty(PropertyName = "type")]
                    public string m_type;

                    [JsonProperty(PropertyName = "view_count")]
                    public int m_view_count;

                    [JsonProperty(PropertyName = "email")]
                    public string m_email;
                }
            }

            [JsonProperty(PropertyName = "type")]
            public string m_type;

            [JsonProperty(PropertyName = "roles")]
            public string[] m_roles;

            [JsonProperty(PropertyName = "subscriptions")]
            public string[] m_subscriptions;
        }
    }

    internal class JSONRpc {
        [JsonProperty(PropertyName = "token")]
        public string m_token = "";

        [JsonProperty(PropertyName = "call")]
        public JSONRpcCall m_call = new JSONRpcCall();

        internal class JSONRpcCall {
            [JsonProperty(PropertyName = "method")]
            public string m_method = "effectResponse";

            [JsonProperty(PropertyName = "args")]
            public JSONRpcArgs[] m_args = new JSONRpcArgs[1];

            [JsonProperty(PropertyName = "id")]
            public string m_id = "";

            [JsonProperty(PropertyName = "type")]
            public string m_type = "call";
        }

        internal class JSONRpcArgs
        {
            [JsonProperty(PropertyName = "timeRemaining")]
            public float m_timeRemaining;

            [JsonProperty(PropertyName = "request")]
            public string m_request = "";

            [JsonProperty(PropertyName = "id")]
            public string m_id = "";

            [JsonProperty(PropertyName = "stamp")]
            public float m_stamp;

            [JsonProperty(PropertyName = "status")]
            public string m_status;

            [JsonProperty(PropertyName = "message")]
            public string m_message = "";

            public JSONRpcArgs(CCEffectInstance effectInstance, string status) {
                m_status = status;
                m_request = effectInstance.id;
                m_id = Utils.GenerateRandomString(26); 

                if (effectInstance is CCEffectInstanceTimed) {
                    m_timeRemaining = (effectInstance as CCEffectInstanceTimed).unscaledTimeLeft;
                }

                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                m_stamp = (long)(DateTime.UtcNow - epoch).TotalSeconds;
            }
        }

        public JSONRpc(string token, CCEffectInstance effectInstance, string status) {
            m_token = token;
            m_call.m_args[0] = new JSONRpcArgs(effectInstance, status);
        }
    }
}
