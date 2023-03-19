using UnityEngine;

namespace WarpWorld.CrowdControl 
{
    [System.Serializable]
    public class BidWarEntry
    {
        /// <summary>The Bid War's ID.</summary>
        public uint ID { get; protected set; }
        /// <summary>The name of the Bid War.</summary>
        public string Name { get { return m_name; } }
        /// <summary>The sprite associated with this Bid War.</summary>
        public Sprite Sprite { get { return m_sprite; } }
        /// <summary>The color tint of this Bid War's sprite.</summary>
        public Color Tint { get { return m_tint; } }

        [SerializeField] private string m_name;
        [SerializeField] private Sprite m_sprite;
        [SerializeField] private Color m_tint = Color.white;

        public BidWarEntry(uint key, string paramName, Sprite sprite = null)
        {
            ID = key;
            m_name = paramName;
            m_sprite = sprite;
        }

        public BidWarEntry(uint key, string paramName, Color tint, Sprite sprite = null)
        {
            ID = key;
            m_name = paramName;
            m_sprite = sprite;
            m_tint = tint;
        }

        public void SetID(uint id)
        {
            ID = id;
        }
    }
}
