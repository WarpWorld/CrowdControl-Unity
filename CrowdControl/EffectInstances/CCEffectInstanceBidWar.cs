using System;

namespace WarpWorld.CrowdControl {
    public class CCEffectInstanceBidWar : CCEffectInstance {
        /// <summary>The key of the Bid War</summary>
        public string BidKey { get { return ""; } }

        /// <summary>The total amount of coins bid towards this Bid War</summary>
        public uint BidAmount { get { return Convert.ToUInt32("0"); } }

        /// <summary>The total amount of coins bid towards this Bid War</summary>
        public string BidName { get { return (effect as CCEffectBidWar).BidWarEntries[BidKey].Name; } }

        public void Init(string bidName, uint amount) {
            //Parameters = new string[] { bidName, amount.ToString() };
        }
    }
}
