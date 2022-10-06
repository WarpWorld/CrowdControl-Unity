using System;
using System.Collections.Generic;

namespace WarpWorld.CrowdControl
{
    /// <summary>Keeps information of all of the bids for a bid war effect. </summary>
    class CCBidWarLibrary 
    {
        private Dictionary<uint, uint> bids = new Dictionary<uint, uint>();

        /// <summary>Who's the current winner of this effect?</summary>
        public uint CurrentWinner { get; private set; } = 0;
        /// <summary>How many coins were spent on the winner? </summary>
        public uint HighestBid { get; private set; } = 0;

        /// <summary>Place a bid for an option. Returns true if the item sent in ends up being the highest and a different ID. </summary>
        public bool PlaceBid(uint id, uint amount)
        {
            if (amount == 0)
            {
                return false;
            }

            if (!bids.ContainsKey(id))
                bids.Add(id, amount);
            else
                bids[id] = amount;

            if (bids[id] <= HighestBid)
                return false;

            HighestBid = bids[id];

            if (id == CurrentWinner)
                return false;

            CurrentWinner = id;

            return true;
        }
    }
}
