using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System;

#pragma warning disable 1591
namespace WarpWorld.CrowdControl.Overlay {
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public abstract class EffectSizeFitter : MonoBehaviour, ILayoutSelfController {
        protected RectTransform rectTransform => transform as RectTransform;

        protected DrivenRectTransformTracker tracker;

        public Action OnAdjusted;
        public CanvasGroup m_viewCanvas;

        protected new void OnEnable() {
            tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
        }

        protected new void OnDisable() {
            tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        protected abstract float GetWidth();

        void ILayoutController.SetLayoutVertical() { }

        void ILayoutController.SetLayoutHorizontal()
        {
            StartCoroutine(SetLayout());
        }

        private IEnumerator SetLayout() {
            if (Application.isPlaying && m_viewCanvas != null)
                m_viewCanvas.alpha = 0;

            yield return new WaitForEndOfFrame();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, GetWidth());

            if (m_viewCanvas != null)
                m_viewCanvas.alpha = 1;

            OnAdjusted?.Invoke();
        }

        public void UpdateLayout()
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, GetWidth());
            //StartCoroutine(SetLayout());
        }
    }
}
