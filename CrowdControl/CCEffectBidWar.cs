using System;
using System.Collections.Generic;
using UnityEngine;

namespace WarpWorld.CrowdControl
{
    /// <summary> Base effect for bid war effects. </summary>
    public abstract class CCEffectBidWar : CCEffectBase
    {
        /// <summary>Name of what the user is bidding towards.</summary>
        [SerializeField]
        protected string bidFor = "";
        /// <summary>How many coins will be bid towards <see cref="bidFor"/></summary>
        [SerializeField]
        protected uint cost = 0;
        /// <summary>A list of tints to be applied to the icon based on if there's a new bid winner.</summary>
        [SerializeField]
        protected List<CCEffectBidWarTint> iconTints = new List<CCEffectBidWarTint>();

        private Dictionary<string, Color> tintDictionary = new Dictionary<string, Color>();

        private string formattedString;
        private Color defaultTint = Color.clear;

        private void Awake()
        {
            if (CrowdControl.instance != null && CrowdControl.instance.EffectIsRegistered(this))
            {
                return;
            }

            StartCoroutine(RegisterEffect());

            foreach (CCEffectBidWarTint tint in iconTints)
                tintDictionary.Add(tint.bidName, tint.tint);
        }

        /// <summary>Size of the playload for this effect.</summary>
        public override ushort PayloadSize(string userName)
        {
            formattedString = String.Format("{0},{1}", bidFor, cost); 
            return Convert.ToUInt16(3 + 4 + 4 + 4 + userName.Length + 1 + formattedString.Length + 1);
        }

        /// <summary>All Parameters for this effect as a string.</summary>
        public override string Params()
        {
            return String.Format("{0},{1}", bidFor, cost);
        }

        /// <summary>Retrieve the tint associated with what's being bid and apply it to the icon.</summary>
        public void AssignTint(string bidName)
        {
            if (tintDictionary.ContainsKey(bidName))
            {
                if (defaultTint.Equals(Color.clear))
                    defaultTint = iconColor;

                iconColor = tintDictionary[bidName];
                return;
            }

            if (defaultTint.Equals(Color.clear))
                return;

            iconColor = defaultTint;
            defaultTint = Color.clear;
        }
    }
}
