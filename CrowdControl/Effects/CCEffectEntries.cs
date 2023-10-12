using UnityEngine;
using System.Collections.Generic;

namespace WarpWorld.CrowdControl {
    /// <summary>A database of every Crowd Control Effect that can be used on this game. </summary>
    public class CCEffectEntries : MonoBehaviour {
        [HideInInspector]
        public Dictionary<string, CCEffectEntry> EffectDictionary { get; private set; } = new Dictionary<string, CCEffectEntry>();

        private void Awake() {
            PrivateResetDictionary();
        }

        public void PrivateResetDictionary() {
            EffectDictionary = new Dictionary<string, CCEffectEntry>();
        }

        public void PrivatePopulateDictionary() {
            foreach (CCEffectBase effect in FindObjectsOfType<CCEffectBase>()) {
                string effectID = effect.SetIdentifier();
                EffectDictionary.Add(effectID, new CCEffectEntry(effectID));
            }
        }

        public bool PrivateAddEffect(CCEffectBase effect) {
            if (EffectDictionary.ContainsKey(effect.ID)) {
                CrowdControl.Log(effect.ID + " shares a keyname with another effect.");
                return false;
            }

            EffectDictionary.Add(effect.ID, new CCEffectEntry(effect.ID));
            return true;
        }

        /// <summary>Retrieve an effect based on it's ID.</summary>
        public CCEffectEntry this[string i] { get { return EffectDictionary[i]; } }
        /// <summary>How many effects does the game have?</summary>
        public int Count { get { return EffectDictionary.Count; } }

        public void AddParameter(string id, string paramName, string parentID) {
            EffectDictionary.Add(id, new CCEffectEntry(id, parentID));
        }
    }
}
