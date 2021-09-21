using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

#pragma warning disable 1591
namespace WarpWorld.CrowdControl.Overlay {
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public abstract class EffectUINode : MonoBehaviour
    {
        internal protected CanvasGroup group;

        internal protected CCEffectInstance effectInstance;

        internal protected virtual void SetVisibility(DisplayFlags displayFlags) {}
        internal protected abstract void Setup(CCEffectInstance effectInstance);

        internal protected virtual void Add(CCEffectInstance effectInstance) {}
        internal protected virtual bool Remove() => true;

        protected void Awake() => group = GetComponent<CanvasGroup>();
    }

    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Crowd Control/Effect UI Panel")]
    public class EffectPanelUI : GridLayoutGroup
    {
        private Dictionary<uint, EffectUINode> activeEffects = new Dictionary<uint, EffectUINode>();
        private Dictionary<uint, Queue<EffectUINode>> nodePool = new Dictionary<uint, Queue<EffectUINode>>();

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

            uint id = effectInstance.effectID;

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

            node.group.alpha = 1;

            node.Setup(effectInstance);
            node.transform.SetAsFirstSibling();
            node.effectInstance = effectInstance;

            activeEffects.Add(m_uiType != UIType.Log ? effectInstance.effectID : effectInstance.id, node);

            return node;
        }

        internal void Add<T>(T source, CCEffectInstance effectInstance, DisplayFlags displayFlags) where T : EffectUINode
        {
            uint id = effectInstance.effect.identifier;

            if (!activeEffects.ContainsKey(id))
            {
                Setup(source, effectInstance, displayFlags);
                return;
            }

            EffectUINode effectNode = activeEffects[id];
            effectNode.Add(effectInstance);
        }

        internal void Remove(uint effectID)
        {
            if (!activeEffects.ContainsKey(effectID))
                return;

            EffectUINode node = activeEffects[effectID];

            if (!node.Remove())
                return;

            uint id = node.effectInstance.effectID;

            if (!nodePool.ContainsKey(id))
                nodePool.Add(id, new Queue<EffectUINode>());

            nodePool[id].Enqueue(node);
            node.group.alpha = 0;
            node.transform.SetAsLastSibling();
            activeEffects.Remove(effectID);
        }
        #endregion

        #region Layout

        public override void SetLayoutVertical()
        {
            base.SetLayoutVertical();
        }

        #endregion
    }
}
