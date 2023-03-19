using System;
using System.Collections.Generic;
using UnityEngine;

namespace WarpWorld.CrowdControl
{
    /// <summary> Base effect for bid war effects. </summary>
    public abstract class CCEffectBidWar : CCEffectBase
    {
        /// <summary>A list of Bid War entries that viewers can choose from. </summary>
        [HideInInspector] public Dictionary<uint, BidWarEntry> BidWarEntries { get; private set; } = new Dictionary<uint, BidWarEntry>();

        [SerializeField]
        [HideInInspector]
        [Tooltip("A list of bid names for this bid war")]
        private List<BidWarEntry> m_bidWarEntries = new List<BidWarEntry>();
        private CCBidWarLibrary m_bidWarLibrary = new CCBidWarLibrary();
        private BidWarEntry m_winnerEntry = null;

        /// <summary>The winning Bid War Entry's name. If there's no winner, this will return the name of this Bid War Effect</summary>
        public override string Name {
            get {
                if (m_winnerEntry == null)
                {
                    return displayName;
                }

                return string.Format("{0}: {1}", displayName, m_winnerEntry.Name);
            }
        }

        /// <summary>The winning Bid War Entry's icon. If there's no winner, this will return the icon of this Bid War effect</summary>
        public override Sprite Icon {
            get {
                return m_winnerEntry != null && m_winnerEntry.Sprite != null ? m_winnerEntry.Sprite : icon;
            }
        }

        /// <summary>The winning Bid War Entry's icon. If there's no winner, this will return the icon's tint of this Bid War effect</summary>
        public override Color IconColor {
            get {
                return m_winnerEntry != null && m_winnerEntry.Tint != null ? m_winnerEntry.Tint : iconColor;
            }
        }

        /// <summary> Takes the list of this effect's parameters and adds them to the effect list</summary>
        public override void RegisterParameters(CCEffectEntries effectEntries) {
            foreach (BidWarEntry entry in m_bidWarEntries) {
                RegisterBidWarEntry(entry, effectEntries);
            }
        }

        /// <summary>All Parameters for this effect as a string.</summary>
        public override string Params() {
            List<string> names = new List<string>();

            foreach (BidWarEntry entry in m_bidWarEntries) {
                names.Add(entry.Name);
            }

            return string.Join(",", names.ToArray());
        }

        /// <summary>Adds a new paramter to the bid war list</summary>
        public void RegisterBidWarEntry(BidWarEntry entry, CCEffectEntries effectEntries)
        {
            uint startIndex = Convert.ToUInt32(effectEntries.Count);

            uint key = Utils.ComputeMd5Hash(entry.Name.ToString() + identifier);
            entry.SetID(key);

            BidWarEntries.Add(key, entry);
            effectEntries.AddParameter(entry.ID, entry.Name, identifier, ItemKind.BidWarValue);
            CrowdControl.instance?.Log("Registered Paramter {0} for {1} index {2}", entry.Name, displayName, entry.ID);
        }

        /// <summary>Place a bid towards one of the bid war entries. Returns true if this causes a new winner.</summary>
        public bool PlaceBid(uint bidID, uint amount)
        {
            bool newWinner = m_bidWarLibrary.PlaceBid(bidID, amount);

            if (newWinner)
            {
                m_winnerEntry = BidWarEntries[bidID];
            }

            return newWinner;
        }

        /// <summary>Returns true if any of the Bid War Entries contains an internal ID.</summary>
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
            identifier = Utils.ComputeMd5Hash(Name + "-" + GetType().ToString());

            if (CrowdControl.instance != null && CrowdControl.instance.EffectIsRegistered(this))
            {
                return;
            }

            StartCoroutine(RegisterEffect());
        }
    }
}
