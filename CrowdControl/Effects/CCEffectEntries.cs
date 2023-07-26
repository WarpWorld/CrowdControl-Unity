using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

namespace WarpWorld.CrowdControl {
    /// <summary>A database of every Crowd Control Effect that can be used on this game. </summary>
    public class CCEffectEntries : MonoBehaviour {
        [HideInInspector]
        public Dictionary<string, CCEffectEntry> EffectDictionary = new Dictionary<string, CCEffectEntry>();

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
            if (EffectDictionary.ContainsKey(effect.effectKey)) {
                CrowdControl.Log(effect.effectKey + " shares a keyname with another effect.");
                return false;
            }

            EffectDictionary.Add(effect.effectKey, new CCEffectEntry(effect.effectKey));
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
