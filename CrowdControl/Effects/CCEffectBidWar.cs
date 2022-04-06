using System;
using System.Collections.Generic;
using UnityEngine;

namespace WarpWorld.CrowdControl
{
    /// <summary> Base effect for bid war effects. </summary>
    public abstract class CCEffectBidWar : CCEffectBase
    {
        public Dictionary<uint, BidWarEntry> BidWarEntries { get; private set; } = new Dictionary<uint, BidWarEntry>();

        /// <summary>A list of bid names for this bid war</summary>
        [SerializeField]
        [Tooltip("A list of bid names for this bid war")]
        private List<BidWarEntry> m_bidWarEntries = new List<BidWarEntry>();
        private List<string> m_bidWarEntryStrings = new List<string>();
        private Dictionary<string, Color> m_tintDictionary = new Dictionary<string, Color>();
        private Color m_defaultTint = Color.clear;

        public override Sprite Icon
        {
            get
            {
                return icon;
            }
        }

        /// <summary> Takes the list of this effect's parameters and adds them to the effect list. </summary>
        public override void RegisterParameters(CCEffectEntries effectEntries)
        {
            foreach (BidWarEntry entry in m_bidWarEntries)
            {
                AddParameter(entry.Name, effectEntries);
            }
        }

        /// <summary>All Parameters for this effect as a string.</summary>
        public override string Params()
        {
            return string.Join(",", m_bidWarEntryStrings.ToArray());
        }

        /// <summary>Retrieve the tint associated with what's being bid and apply it to the icon.</summary>
        public void AssignTint(string bidName)
        {
            if (m_tintDictionary.ContainsKey(bidName))
            {
                if (m_defaultTint.Equals(Color.clear))
                    m_defaultTint = iconColor;

                iconColor = m_tintDictionary[bidName];
                return;
            }

            if (m_defaultTint.Equals(Color.clear))
                return;

            iconColor = m_defaultTint;
            m_defaultTint = Color.clear;
        }

        /// <summary>Adds a new paramter to the bid war list</summary>
        public void AddParameter(string bidName, CCEffectEntries effectEntries)
        {
            uint startIndex = Convert.ToUInt32(effectEntries.Count);

            uint key = Utils.ComputeMd5Hash(bidName.ToString() + identifier);

            BidWarEntry newBidEntry = new BidWarEntry(key, bidName);
            BidWarEntries.Add(key, newBidEntry);
            AddParameter(newBidEntry, effectEntries);
        }

        public void PlaceBid(string bidName, uint amount)
        {
            CrowdControl.instance.PlaceBid(identifier, bidName, amount);
        }
        
        public override bool HasParameterID(uint id)
        {
            return BidWarEntries.ContainsKey(id);
        }

        protected internal sealed override EffectResult OnTriggerEffect(CCEffectInstance effectInstance)
        {
            return OnTriggerEffect(effectInstance as CCEffectInstanceBidWar);
        }

        /// <summary> Invoked when an effect instance is scheduled to start. The effect should only be applied when <see cref="EffectResult.Success"/> is returned. </summary>
        protected internal abstract EffectResult OnTriggerEffect(CCEffectInstanceBidWar effectInstance);

        private void Awake()
        {
            if (CrowdControl.instance != null && CrowdControl.instance.EffectIsRegistered(this))
            {
                return;
            }

            StartCoroutine(RegisterEffect());
        }

        private void AddParameter(BidWarEntry entry, CCEffectEntries effectEntries)
        {
            effectEntries.AddParameter(entry.ID, BidWarEntries[entry.ID].Name, identifier, ItemKind.BidWarValue);
            CrowdControl.instance.Log("Registered Paramter {0} for {1} index {2}", entry.Name, displayName, entry.ID);
        }
    }
}
