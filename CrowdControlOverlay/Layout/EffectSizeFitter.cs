using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

#pragma warning disable 1591
namespace WarpWorld.CrowdControl.Overlay {
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public abstract class EffectSizeFitter : MonoBehaviour, ILayoutSelfController {
        protected RectTransform rectTransform => transform as RectTransform;

        protected DrivenRectTransformTracker tracker;

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
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, GetWidth());
            StartCoroutine(SetLayout());
        }

        private IEnumerator SetLayout()
        {
            yield return new WaitForEndOfFrame();
        }

        public void UpdateLayout()
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, GetWidth());
            //StartCoroutine(SetLayout());
        }
    }
}
