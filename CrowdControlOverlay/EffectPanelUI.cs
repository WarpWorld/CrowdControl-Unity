using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;  

#pragma warning disable 1591
namespace WarpWorld.CrowdControl.Overlay {
    [RequireComponent(typeof(RectTransform), typeof(GridLayoutGroup))]
    [AddComponentMenu("Crowd Control/Effect UI Panel")]
    public class EffectPanelUI : MonoBehaviour 
    {
        private Dictionary<string, EffectUINode> activeEffects = new Dictionary<string, EffectUINode>();
        private Dictionary<string, Queue<EffectUINode>> nodePool = new Dictionary<string, Queue<EffectUINode>>();

        public enum UIType
        {
            Log,
            Queue,
            Buff
        }

        public UIType m_uiType = UIType.Log;

        #region Nodes

        internal EffectUINode Setup<T>(T source, CCEffectInstance effectInstance, DisplayFlags displayFlags) where T : EffectUINode {
            T node;

            string id = effectInstance.effectKey;

            if (nodePool.ContainsKey(id) && nodePool[id].Count > 0)
            {
                node = nodePool[id].Dequeue() as T;
            }
            else
            {
                node = Instantiate(source, transform);
                node.gameObject.SetActive(true);
                node.SetVisibility(displayFlags);
            }

            node.gameObject.SetActive(true);

            node.Setup(effectInstance);
            node.transform.SetAsFirstSibling();
            node.effectInstance = effectInstance;

            activeEffects.Add(m_uiType != UIType.Log ? effectInstance.effectKey : effectInstance.id.ToString(), node);

            return node;
        }

        internal void Add<T>(T source, CCEffectInstance effectInstance, DisplayFlags displayFlags) where T : EffectUINode
        {
            string id = effectInstance.effect.Key;

            if (!activeEffects.ContainsKey(id))
            {
                Setup(source, effectInstance, displayFlags);
                return;
            }

            EffectUINode effectNode = activeEffects[id];
            effectNode.Add(effectInstance);
        }

        internal void Remove(string effectID)
        {
            if (!activeEffects.ContainsKey(effectID))
                return;

            EffectUINode node = activeEffects[effectID];

            if (!node.Remove())
                return;

            string id = node.effectInstance.effectKey;

            if (!nodePool.ContainsKey(id))
                nodePool.Add(id, new Queue<EffectUINode>());

            nodePool[id].Enqueue(node);
            
            node.transform.SetAsLastSibling();
            node.gameObject.SetActive(false);
            activeEffects.Remove(effectID);
        }
        #endregion
    }
}
