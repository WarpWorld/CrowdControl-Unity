using Newtonsoft.JsonCC;
using System;

namespace WarpWorld.CrowdControl {
    [Serializable]
    public class EffectJSON {
        public class QuantityProperties {
            [JsonProperty(PropertyName = "min")]
            public uint Min = 1;

            [JsonProperty(PropertyName = "max")]
            public uint Max = 99;

            public QuantityProperties(uint min, uint max) {
                Min = min;
                Max = max;
            }
        }

        public class DurationProperties {
            [JsonProperty(PropertyName = "value")]
            public long? Value = 1;

            public DurationProperties(float length) {
                Value = (long?)length;
            }
        }

        [JsonProperty(PropertyName = "inactive")]
        public bool Inactive;

        [JsonProperty(PropertyName = "disabled")]
        public bool Disabled;

        [JsonProperty(PropertyName = "unpoolable")]
        public bool NoPooling;

        [JsonProperty(PropertyName = "name")]
        public string Name;

        [JsonProperty(PropertyName = "sortName")]
        public string SortName;

        [JsonProperty(PropertyName = "note")]
        public string Note;

        [JsonProperty(PropertyName = "category")]
        public string [] Category;

        [JsonProperty(PropertyName = "description")]
        public string Description;

        [JsonProperty(PropertyName = "price")]
        private uint? Price = 1;

        [JsonProperty(PropertyName = "morality")]
        public int Morality;

        [JsonProperty(PropertyName = "parameters")]
        public object Parameters;

        [JsonProperty(PropertyName = "quantity")]
        public QuantityProperties Quantity;

        [JsonProperty(PropertyName = "duration")]
        public DurationProperties Duration;

        [JsonProperty(PropertyName = "userCooldown")]
        public TimeSpan? ViewerCooldown;

        public EffectJSON(CCEffectBase effect) {
            Inactive = effect.Inactive;
            Disabled = effect.Disabled;
            Name = effect.Name;
            Category = effect.Categories;
            Description = effect.description;
            Price = effect.price;
            NoPooling = effect.NoPooling;
            Morality = (int)effect.Morality;

            if (Morality == 2)
                Morality = -1;

            ViewerCooldown = TimeSpan.FromSeconds((double)(new decimal(effect.pendingDelay)));

            if (effect is CCEffectTimed) {
                Duration = new DurationProperties((effect as CCEffectTimed).duration);
            }
            else if (effect is CCEffectParameters) {
                CCEffectParameters paramEffect = effect as CCEffectParameters;

                int paramIndex = 0;
                int optionIndex = 0;

                string paramList = string.Empty;

                foreach (string paramKey in paramEffect.ParameterEntries.Keys) {
                    paramIndex++;

                    if (paramEffect.ParameterEntries[paramKey].ParamKind == ParameterEntry.Kind.Quantity) {
                        Quantity = new QuantityProperties(paramEffect.ParameterEntries[paramKey].Min, paramEffect.ParameterEntries[paramKey].Max);

                        continue;
                    }

                    ParameterEntry paramEntry = paramEffect.ParameterEntries[paramKey];

                    paramList += $"{{ \"{paramEntry.ID}\": {{ ";
                    paramList += $"\"name\": \"{paramEntry.Name}\", ";
                    paramList += "\"type\": \"options\", ";
                    paramList += "\"options\": {";

                    optionIndex = 0;

                    foreach (ParameterOption option in paramEntry.Options) {
                        paramList += $"\"{option.ID}\": {{ \"name\": \"{option.Name}\"}}";

                        optionIndex++;

                        if (optionIndex < paramEntry.Options.Length)
                            paramList += ", ";
                    }

                    paramList += "}}}";

                    paramIndex++;
                    if (paramIndex < paramEffect.ParameterEntries.Keys.Count - 1)
                        paramList += ", ";
                }

                Parameters = JsonConvert.DeserializeObject(paramList);
            }
            else if (effect is CCEffectBidWar) {
                CCEffectBidWar bidWarEffect = effect as CCEffectBidWar;

                string bidWarList = "";

                bidWarList = $"{{ \"{effect.effectKey}_options\": {{ ";
                bidWarList += $"\"name\": \"{effect.Name} Options\", ";
                bidWarList += "\"type\": \"options\", ";
                bidWarList += "\"options\": {";

                int index = 0;

                foreach (string bidKey in bidWarEffect.BidWarEntries.Keys) {
                    bidWarList += $"\"{bidKey}\": {{ \"name\": \"{bidWarEffect.BidWarEntries[bidKey].Name}\"}}";

                    index++;

                    if (index < bidWarEffect.BidWarEntries.Keys.Count)
                        bidWarList += ", ";
                }

                bidWarList += "}}}";

                Parameters = JsonConvert.DeserializeObject(bidWarList);
            }
        }
    }
}
