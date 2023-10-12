using UnityEngine;
using UnityEngine.UI;


namespace CrowdControlOverlay {
    public class LayoutUpdater : MonoBehaviour {
        public RectTransform targetLayout;

        public void Update() {
            LayoutRebuilder.ForceRebuildLayoutImmediate(targetLayout);
        }
    }
}
