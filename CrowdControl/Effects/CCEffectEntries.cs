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
                string effectKey = effect.SetIdentifier();
                EffectDictionary.Add(effectKey, new CCEffectEntry(effectKey));
            }
        }

        public bool PrivateAddEffect(CCEffectBase effect) {
            if (EffectDictionary.ContainsKey(effect.Key)) {
                CrowdControl.Log(effect.Key + " shares a keyname with another effect.");
                return false;
            }

            EffectDictionary.Add(effect.Key, new CCEffectEntry(effect.Key));
            return true;
        }

        /// <summary>Retrieve an effect based on it's ID.</summary>
        public CCEffectEntry this[string i] { get { return EffectDictionary[i]; } }
        /// <summary>How many effects does the game have?</summary>
        public int Count { get { return EffectDictionary.Count; } }

        public void AddParameter(string key, string paramName, string parentID) {
            EffectDictionary.Add(key, new CCEffectEntry(key, parentID));
        }
    }
}
