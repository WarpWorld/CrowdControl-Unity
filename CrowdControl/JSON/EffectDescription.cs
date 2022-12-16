﻿using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.JsonCC;

namespace WarpWorld.CrowdControl
{
    public class EffectDescription
    {
        /// <summary> The parent identifier. </summary>
        [JsonProperty(PropertyName = "parent")]
        private string Parent;

        /// <summary> The name of the effect. </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name;

        /// <summary> The effect code for use in commands. </summary>
        [JsonProperty(PropertyName = "safeName")]
        private readonly string SafeName;

        /// <summary> A long description of the associated effect. </summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; private set; }

        /// <summary> A long description of the associated effect. </summary>
        [JsonProperty(PropertyName = "image")]
        private string Image;

        /// <summary> True if the item corresponds to a timed effect, otherwise false. </summary>
        [JsonProperty(PropertyName = "durational")]
        private bool Durational;

        /// <summary> The default time of the effect. </summary>
        [JsonProperty(PropertyName = "duration")]
        private string Duration;

        [JsonProperty(PropertyName = "formula")]
        private Formulas Formula = Formulas.Sum;

        public enum Formulas : byte
        {
            Sum = 0,
            Product = 1,
            BaseAddMultipliedPairs = 2,
            ArcLevel = 3,
            First = 4,
            Last = 5,
            Expression = 255
        }

        [JsonProperty(PropertyName = "formulaExpression")]
        private string FormulaExpression;

        /// <summary> The kind of item. </summary>
        [JsonProperty(PropertyName = "kind")]
        public ItemKind Kind { get; private set; }

        /// <summary> The type of item. </summary>
        [JsonProperty(PropertyName = "type")]
        private string Type;

        /// <summary> The type of item. </summary>
        [JsonProperty(PropertyName = "paramTypes")]
        private List<string> ParamTypes;

        /// <summary> True if the item is currently marked as hidden, otherwise false. </summary>
        [JsonProperty(PropertyName = "hidden")]
        private bool Hidden;

        /// <summary> The cost of the item, in coins. </summary>
        [DefaultValue(1)]
        [JsonProperty(PropertyName = "price", DefaultValueHandling = DefaultValueHandling.Populate)]
        private uint? Price = 1;

        [JsonProperty(PropertyName = "tags")]
        private List<string> Tags;


        [JsonIgnore]
        public bool Parameter = false;

        public EffectDescription(uint key, CCEffectBase effect, string parent = "")
        {
            Name = effect.displayName;
            SafeName = effect.identifier.ToString();

            if (effect is CCEffectTimed) {
                int time = System.Convert.ToInt32((effect as CCEffectTimed).duration);

                int seconds = time % 60;
                int minutes = (time / 60) % 60;
                int hours = (time / 2400);

                Durational = true;

                Duration = string.Format("{0}:{1}:{2}", hours.ToString("00"), minutes.ToString("00"), seconds.ToString("00"));
            }

            Kind = ItemKind.Effect;
            Price = effect.price;
            Parent = parent;

            if (effect is CCEffectBidWar)
            {
                Kind = ItemKind.BidWar;
                return;
            }

            if (!(effect is CCEffectParameters))
            {
                return;
            }

            CCEffectParameters effectParameters = effect as CCEffectParameters;
            ParamTypes = new List<string>();

            foreach (uint paramKey in effectParameters.ParameterEntries.Keys)
            {
                ParamTypes.Add(paramKey.ToString());
            }
        }

        public EffectDescription(string name, ItemKind kind, string key, string parent)
        {
            Name = name;
            SafeName = key;
            Kind = kind;
            Parent = parent;
        }

        public EffectDescription(BidWarEntry entry, CCEffectBase parent, ItemKind kind)
        {
            Name = entry.Name;
            SafeName = entry.ID.ToString();
            Kind = kind;
            Parent = parent.identifier.ToString();
        }

        public EffectDescription(ParameterOption option)
        {
            Name = option.Name;
            SafeName = option.ID.ToString();
            Kind = ItemKind.Usable;
            Type = option.ParentID.ToString();
            Parameter = true;
        }

        public void EraseDescription()
        {
            Description = string.Empty;
        }
    }
}
