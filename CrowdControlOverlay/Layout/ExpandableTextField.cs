using UnityEngine;
using UnityEngine.UI;

namespace CrowdControlOverlay {
    [RequireComponent(typeof(Text))]
    public class ExpandableTextField : MonoBehaviour {
        private Text textField;
        private RectTransform rectTransform;

        void Awake() {
            textField = GetComponent<Text>();
            rectTransform = GetComponent<RectTransform>();
        }

        void Update() {
            float preferredWidth = textField.preferredWidth;
            rectTransform.sizeDelta = new Vector2(preferredWidth, rectTransform.sizeDelta.y);
        }
    }
}
