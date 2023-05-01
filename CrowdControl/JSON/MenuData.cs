using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.JsonCC; 

namespace WarpWorld.CrowdControl {
    [Serializable]
    public class MenuData
    {
        private static readonly DateTimeOffset EPOCH = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        [JsonProperty(PropertyName = "id")]
        private Guid ID = Guid.NewGuid();

        [JsonProperty(PropertyName = "stamp")]
        private ulong Stamp = (ulong)(DateTimeOffset.UtcNow.ToUniversalTime() - EPOCH).TotalSeconds;

        [JsonProperty(PropertyName = "type")]
        private byte BlockType = 0xD1;

        [JsonProperty(PropertyName = "gamename")]
        public string GameName;

        [JsonProperty(PropertyName = "items")]
        public List<EffectDescription> Items = new List<EffectDescription>();

        [JsonProperty(PropertyName = "itemTypes")]
        public List<ItemType> ItemTypes = new List<ItemType>();

        [JsonProperty(PropertyName = "loadType")]
        public MenuLoadType? LoadType = MenuLoadType.Overwrite;

        public enum MenuLoadType
        {
            Overwrite = 0,
            Append = 1
        }

        public MenuData() { }
        public MenuData(IEnumerable<EffectDescription> items) => Items = items.ToList();
    }

}
