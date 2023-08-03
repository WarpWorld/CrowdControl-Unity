using UnityEngine;

namespace WarpWorld.CrowdControl  {
    [System.Serializable]
    public class BidWarEntry {
        /// <summary>The Bid War's ID.</summary>
        public string ID { get; protected set; }
        /// <summary>The name of the Bid War.</summary>
        public string Name { get { return m_name; } }
        /// <summary>The sprite associated with this Bid War.</summary>
        public Sprite Sprite { get { return m_sprite; } }
        /// <summary>The color tint of this Bid War's sprite.</summary>
        public Color Tint { get { return m_tint; } }

        [SerializeField] private string m_name;
        [SerializeField] private Sprite m_sprite;
        [SerializeField] private Color m_tint = Color.white;
    }
}
