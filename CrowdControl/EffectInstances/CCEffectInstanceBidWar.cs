using System;

namespace WarpWorld.CrowdControl
{
    public class CCEffectInstanceBidWar : CCEffectInstance
    {
        public uint BidKey { get { return Convert.ToUInt32(Parameters[0]); } }
        public uint BidAmount { get { return Convert.ToUInt32(Parameters[1]); } }
        public string BidName { get { return (effect as CCEffectBidWar).BidWarEntries[BidKey].Name; } }

        public void Init(string bidName, uint amount)
        {
            Parameters = new string[] { bidName, amount.ToString() };
        }
    }
}
