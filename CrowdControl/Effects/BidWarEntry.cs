using UnityEngine;

namespace WarpWorld.CrowdControl
{
    [System.Serializable]
    public class BidWarEntry
    {
        public uint ID { get; protected set; }
        public string Name { get { return m_name; } }
        public Sprite Sprite { get { return m_sprite; } }
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
