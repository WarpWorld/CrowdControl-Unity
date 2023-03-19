using UnityEngine;
using System.Collections.Generic;
using System;

namespace WarpWorld.CrowdControl {
    /// <summary>A database of every Crowd Control Effect that can be used on this game. </summary>
    public class CCEffectEntries : MonoBehaviour {
        /// <summary>An array of every effect.</summary>
        [SerializeField]
        private CCEffectEntry[] effectArray;

        [HideInInspector]
        public Dictionary<uint, CCEffectEntry> EffectDictionary = new Dictionary<uint, CCEffectEntry>();
         
        private void Awake() {
            PrivateResetDictionary();
            PrivatePopulateDictionary();
        }

        public void PrivatePopulateDictionary() {
            for (int i = 0; i < effectArray.Length; i++) {
                if (string.IsNullOrEmpty(effectArray[i].ClassName)) {
                    continue;
                }

                EffectDictionary.Add(Utils.ComputeMd5Hash(effectArray[i].ClassName), effectArray[i]);
            }
        }

        public void PrivateResetDictionary() {
            EffectDictionary = new Dictionary<uint, CCEffectEntry>();
        }

        public bool PrivateAddEffect(CCEffectBase effect) {
            for (int i = 0; i < effectArray.Length; i++) {
                if (Equals(effect.GetType().ToString(), effectArray[i].ClassName)) {
                    if (EffectDictionary.ContainsKey(effect.identifier)) {
                        return false;
                    }

                    EffectDictionary.Add(effect.identifier, effectArray[i]);
                    return true;
                }; 
            }

            return false;
        }

        /// <summary>Retrieve an effect based on it's ID.</summary>
        public CCEffectEntry this[uint i] { get { return EffectDictionary[i]; } }
        /// <summary>How many effects does the game have?</summary>
        public int Count { get { return EffectDictionary.Count; } }

        public void AddParameter(uint key, string paramName, uint parentID, ItemKind type) {
            EffectDictionary.Add(key, new CCEffectEntry(key, type.ToString(), parentID));
        }
    }
}
