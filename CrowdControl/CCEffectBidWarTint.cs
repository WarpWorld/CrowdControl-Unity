using UnityEngine;

namespace WarpWorld.CrowdControl
{
    /// <summary>The name of a bid paired up with a tint to apply for Bid War notifications.</summary>
    [System.Serializable]
    public class CCEffectBidWarTint
    {
        /// <summary>Name of this entry.</summary>
        public string bidName;
        /// <summary>Color to tint the into when <see cref="bidName"/> is the winning bid.</summary>
        public Color tint;
    }
}
