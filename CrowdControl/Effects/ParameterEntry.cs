using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

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

        [HideInInspector] public string testParamName;

        /// <summary> Get the name of an option based off its id. </summary>
        public string GetOptionName(string id) {
            return Options.FirstOrDefault(option => string.Equals(id, option.ID)).Name;
        }

        public ParameterEntry(string id, string paramName, uint min, uint max)  {
            ID = id;
            m_paramKind = Kind.Quantity;
            m_min = min;
            m_max = max;

            InitOptions();
        }

        public ParameterEntry(string id, string paramName) {
            ID = id;
            m_paramKind = Kind.Quantity;

            InitOptions();
        }

        internal void SetID(string parentID) {
            Regex rgx = new Regex("[^a-z0-9-]");

            string effectID = Name.ToString().ToLower();
            ID = parentID + "_" + rgx.Replace(effectID, "");
        }

        private void InitOptions() {
            if (Options != null)
                return;

            Options = new ParameterOption[m_options.Length];

            for (int i = 0; i < m_options.Length; i++) {
                Options[i] = new ParameterOption(m_options[i], ID);
            }
        }
    }
}
