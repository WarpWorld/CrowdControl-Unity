using UnityEngine;

#pragma warning disable 1591
namespace WarpWorld.CrowdControl.Overlay
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public abstract class EffectUINode : MonoBehaviour
    {
        internal protected CanvasGroup group;

        internal protected CCEffectInstance effectInstance;

        internal protected virtual void SetVisibility(DisplayFlags displayFlags) { }
        internal protected abstract void Setup(CCEffectInstance effectInstance);

        internal protected virtual void Add(CCEffectInstance effectInstance) { }
        internal protected virtual bool Remove() => true;

        protected void Awake() => group = GetComponent<CanvasGroup>();
    }
}
