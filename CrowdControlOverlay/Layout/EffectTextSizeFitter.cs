using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 1591
namespace WarpWorld.CrowdControl.Overlay {
    [AddComponentMenu("Crowd Control/Effect Text Size Fitter")]
    public class EffectTextSizeFitter : EffectSizeFitter {
        protected override float GetWidth() {
            var width = 0f;
            for (var i = 0; i < transform.childCount; i++) {
                var child = transform.GetChild(i) as RectTransform;
                width = Mathf.Max(width, LayoutUtility.GetPreferredWidth(child) + child.offsetMin.x - child.offsetMax.x);
            }

            return width;
        }
    }
}
