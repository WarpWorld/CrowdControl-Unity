using UnityEngine;
using System.Collections;

#pragma warning disable 1591
namespace WarpWorld.CrowdControl.Overlay {
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public abstract class EffectUINode : MonoBehaviour {
        protected internal abstract void SetVisibility(DisplayFlags displayFlags);

        internal protected CanvasGroup group;
        internal protected CCEffectInstance effectInstance; 

        internal protected virtual void Setup(CCEffectInstance effectInstance) {

        }

        internal protected virtual void Add(CCEffectInstance effectInstance) { }
        internal protected virtual bool Remove()  {
            return true;
        }

        protected void Awake() => group = GetComponent<CanvasGroup>();

        private bool m_alphaOff;

        public void SetAlphaOff() {
            m_alphaOff = true;

            group.gameObject.SetActive(false);
        }

        public void SetAlphaOn() {
            group.gameObject.SetActive(true);
        }

        private bool m_lateUpdate = false;

        protected IEnumerator SetAlphaOffRoutine() {
            group.alpha = 0;

            while (m_alphaOff) {
                CrowdControl.Log(group.alpha);
                yield return new WaitForSeconds(0.1f);
                group.alpha = 0;
            }
        }
    }
}
