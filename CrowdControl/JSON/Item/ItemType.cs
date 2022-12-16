using System;
using Newtonsoft.JsonCC;

namespace WarpWorld.CrowdControl
{
    [Serializable]
    public class ItemType
    {
        /// <summary> The item identifier. </summary>
        [JsonProperty(PropertyName = "id")]
        private uint? ID;

        /// <summary> The type name. </summary>
        [JsonProperty(PropertyName = "name")]
        private string Name;

        /// <summary> The type safe name. </summary>
        [JsonProperty(PropertyName = "safeName")]
        private string SafeName;

        /// <summary> The subtype of the parameter type. </summary>
        [JsonProperty(PropertyName = "type")]
        private Subtype Type;

        /// <summary> The enumeration of type types. </summary>
        public enum Subtype : byte
        {
            ItemList = 0,
            Slider = 1
        }

        /// <summary> The type metadata. </summary>
        [JsonProperty(PropertyName = "meta"), JsonConverter(typeof(NullStringConverter))]
        private string Meta
        {
            get => _meta;
            set
            {
                _meta = value;
                switch (Type)
                {
                    case Subtype.Slider:
                        _meta_obj = JsonConvert.DeserializeObject<SliderMeta>(Meta);
                        break;
                }
            }
        }
        [JsonIgnore] private string _meta;
        [JsonIgnore] private object _meta_obj;

        public bool TryParse(object value, out long result)
        {
            switch (Type)
            {
                case Subtype.Slider:
                    if (!long.TryParse(value.ToString(), out result)) { return false; }
                    if (result < ((SliderMeta)_meta_obj).min) { return false; }
                    if (result > ((SliderMeta)_meta_obj).max) { return false; }
                    return true;
                default:
                    throw new InvalidOperationException("Cannot perform parse on non-slider types.");
            }
        }

        /// <summary> Represents the object as a string. </summary>
        public override string ToString() => Name;

        [JsonConstructor]
        public ItemType(ParameterEntry entry)
        {
            ID = entry.ID;
            Name = entry.Name;
            SafeName = entry.ID.ToString();

            if (entry.ParamKind == ParameterEntry.Kind.Quantity)
            {
                Type = Subtype.Slider;
                Meta = "{\"min\":" + entry.Min + ",\"max\":" + entry.Max + "}";
                return;
            }

            Type = Subtype.ItemList;
        }

        public ItemType(uint id, string name)
        {

        }

        private class SliderMeta
        {
            public long min;
            public long max;
        }
    }
}
