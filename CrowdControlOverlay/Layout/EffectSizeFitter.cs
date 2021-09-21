using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

#pragma warning disable 1591
namespace WarpWorld.CrowdControl.Overlay {
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public abstract class EffectSizeFitter : UIBehaviour, ILayoutSelfController {
        [SerializeField] HorizontalLayoutGroup horizontalLayoutGroup;

        protected RectTransform rectTransform => transform as RectTransform;

        protected DrivenRectTransformTracker tracker;

        protected new void OnValidate() => SetDirty();

        protected override void OnRectTransformDimensionsChange() => SetDirty();

        protected new void OnEnable() {
            base.OnEnable();
            tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
            SetDirty();
        }

        protected new void OnDisable() {
            tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        protected void SetDirty() {
            if (IsActive()) LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        protected abstract float GetWidth();

        void ILayoutController.SetLayoutVertical() { }

        void ILayoutController.SetLayoutHorizontal()
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, GetWidth());

            if (horizontalLayoutGroup == null)
            {
                return;
            }

            horizontalLayoutGroup.SetLayoutHorizontal();
            StartCoroutine(SetLayout());
        }

        private IEnumerator SetLayout()
        {
            yield return new WaitForEndOfFrame();
            horizontalLayoutGroup.SetLayoutHorizontal();
        }
    }
}
