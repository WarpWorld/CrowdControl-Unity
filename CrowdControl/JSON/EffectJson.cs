using Newtonsoft.JsonCC;
using System;

namespace WarpWorld.CrowdControl
{
    [Serializable]
    internal class EffectJSON
    {
        internal class QuantityProperties
        {
            [JsonProperty(PropertyName = "min")]
            public uint Min = 1;

            [JsonProperty(PropertyName = "max")]
            public uint Max = 99;

            public QuantityProperties(uint min, uint max)
            {
                Min = min;
                Max = max;
            }
        }

        internal class DurationProperties
        {
            [JsonProperty(PropertyName = "value")]
            public long? Value = 1;

            public DurationProperties(float length)
            {
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
        public string[] Category;

        [JsonProperty(PropertyName = "description")]
        public string Description;

        [JsonProperty(PropertyName = "price")]
        private uint? Price = 1;

        [JsonProperty(PropertyName = "moral")]
        public int Moral;

        [JsonProperty(PropertyName = "parameters")]
        public object Parameters;

        [JsonProperty(PropertyName = "quantity")]
        public QuantityProperties Quantity;

        [JsonProperty(PropertyName = "duration")]
        public DurationProperties Duration;

        /*[JsonProperty(PropertyName = "userCooldown")]
        public TimeSpan? ViewerCooldown;
        */
        public EffectJSON(CCEffectBase effect)
        {
            Inactive = !effect.Sellable;
            Disabled = !effect.Visible;
            Name = effect.Name;
            Category = effect.Categories;
            Description = effect.Description;
            Price = effect.Price;
            NoPooling = effect.NoPooling;
            Moral = (int)effect.Morality;

            if (Moral == 2)
                Moral = -1;

            //
            //ViewerCooldown = TimeSpan.FromSeconds((double)(new decimal(effect.PendingDelay)));

            if (effect is CCEffectTimed) {
                Duration = new DurationProperties((effect as CCEffectTimed).Duration);
            }
            else if (effect is CCEffectParameters) {
                CCEffectParameters paramEffect = effect as CCEffectParameters;

                int paramIndex = 0;
                int optionIndex = 0;

                string paramList = string.Empty;

                foreach (string paramID in paramEffect.ParameterEntries.Keys) {
                    paramIndex++;

                    if (paramEffect.ParameterEntries[paramID].ParamKind == ParameterEntry.Kind.Quantity) {
                        Quantity = new QuantityProperties(paramEffect.ParameterEntries[paramID].Min, paramEffect.ParameterEntries[paramID].Max);
                        continue;
                    }

                    ParameterEntry paramEntry = paramEffect.ParameterEntries[paramID];

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

                bidWarList = $"{{ \"{effect.ID}_options\": {{ ";
                bidWarList += $"\"name\": \"{effect.Name} Options\", ";
                bidWarList += "\"type\": \"options\", ";
                bidWarList += "\"options\": {";

                int index = 0;

                foreach (string bidID in bidWarEffect.BidWarEntries.Keys)  {
                    bidWarList += $"\"{bidID}\": {{ \"name\": \"{bidWarEffect.BidWarEntries[bidID].Name}\"}}";

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