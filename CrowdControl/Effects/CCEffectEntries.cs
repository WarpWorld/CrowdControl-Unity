using UnityEngine;
using System.Collections.Generic;
using System;

namespace WarpWorld.CrowdControl
{
    /// <summary>A database of every Crowd Control Effect that can be used on this game. </summary>
    public class CCEffectEntries : MonoBehaviour
    {
        /// <summary>An array of every effect.</summary>
        [SerializeField]
        private CCEffectEntry[] effectArray;

        private Dictionary<uint, CCEffectEntry> effectDictionary = new Dictionary<uint, CCEffectEntry>();

        private void Awake()
        {
            effectDictionary = new Dictionary<uint, CCEffectEntry>();

            for (int i = 0; i < effectArray.Length; i++)
            {
                effectDictionary.Add(Utils.ComputeMd5Hash(effectArray[i].ClassName), effectArray[i]);
            }

            effectArray = null;
        }

        /// <summary>Retrieve an effect based on it's ID.</summary>
        public CCEffectEntry this[uint i] { get { return effectDictionary[i]; } }
        /// <summary>How many effects does the game have?</summary>
        public int Count { get { return effectDictionary.Count; } }

        public void AddParameter(uint key, string paramName, uint parentID, ItemKind type)
        {
            effectDictionary.Add(key, new CCEffectEntry(key, type.ToString(), parentID));
        }
    }
}
