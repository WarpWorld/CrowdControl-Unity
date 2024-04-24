using Newtonsoft.JsonCC;
using System.Collections.Generic;

namespace WarpWorld.CrowdControl {
    public class CCJsonBlock { // 0xFA
        public string jsonString;

        private string JSONString(string objectName, object obj) {
            JsonSerializerSettings settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
            string JSONContent = JsonConvert.SerializeObject(obj, settings);
            return $"\"{objectName}\": {JSONContent}";
        }

        public CCJsonBlock(string gameName, Dictionary<string, CCEffectBase> effectList) {
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

            foreach (string id in effectList.Keys)  {
                CCEffectBase effect = effectList[id];
                EffectJSON effectJSON = new EffectJSON(effect);

                effectString += JSONString(id.ToString(), effectJSON);
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
