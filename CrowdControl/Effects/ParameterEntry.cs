﻿using UnityEngine;
using System.Linq;

namespace WarpWorld.CrowdControl {
    [System.Serializable]
    public class ParameterEntry : BidWarEntry {
        public enum Kind {
            Item,
            Quantity
        }

        /// <summary> What kind of parameter is this. </summary>
        public Kind ParamKind { get { return m_paramKind; } }

        /// <summary> Minimum quantity allowed for this effect. </summary>
        public uint Min { get { return m_min; } }

        /// <summary> Maximum quantity allowed for this effect. </summary>
        public uint Max { get { return m_max; } }

        /// <summary> The choices for an item list. </summary>
        public ParameterOption[] Options { get; private set; }

        [SerializeField] private Kind m_paramKind;
        [SerializeField] private string[] m_options;
        [SerializeField] private uint m_min;
        [SerializeField] private uint m_max;

        public ParameterEntry(string id, string paramName) : base(id, paramName) {
            ID = id;
            m_paramKind = Kind.Item;
        }

        public ParameterEntry(string id, string paramName, uint min, uint max) : base(id, paramName) {
            ID = id;
            m_paramKind = Kind.Quantity;
            m_min = min;
            m_max = max;

            InitOptions();
        }

        public void InitOptions() {
            if (Options != null)
                return;

            Options = new ParameterOption[m_options.Length];

            for (int i = 0; i < m_options.Length; i++) {
                Options[i] = new ParameterOption(m_options[i], ID);
            }
        }

        public string GetOptionName(string key) {
            return Options.FirstOrDefault(option => string.Equals(key, option.ID)).Name;
        }
    }
}
