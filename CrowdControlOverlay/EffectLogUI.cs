using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 1591
namespace WarpWorld.CrowdControl.Overlay {
    [AddComponentMenu("Crowd Control/Effect Log UI")]
    public class EffectLogUI : EffectUINode {
        [SerializeField] protected Text effectName;
        [SerializeField] protected Image effectIcon;

        [Space]
        [SerializeField] protected Text userName;
        [SerializeField] protected Image userIcon;
        [SerializeField] protected Image userFrame;

        [Space]
        [SerializeField] protected RectTransform effectNameElement;
        [SerializeField] protected RectTransform effectIconElement;
        [SerializeField] protected RectTransform userNameElement;
        [SerializeField] protected RectTransform userIconElement;
        [SerializeField] protected GameObject textContainer;
        [SerializeField] protected EffectTextSizeFitter textSizeFitter;
        [SerializeField] protected GameObject specialStatusContainer;
        [SerializeField] protected Text specialStatus;
        [SerializeField] protected Text coinsSpent;
        [SerializeField] protected RectTransform underlay;
        [SerializeField] protected RectTransform mainContent;
        [SerializeField] protected RectTransform container;

        protected internal override void SetVisibility(DisplayFlags displayFlags) {
            effectNameElement.gameObject.SetActive((displayFlags & DisplayFlags.EffectName) != 0);
            effectIconElement.gameObject.SetActive((displayFlags & DisplayFlags.EffectIcon) != 0);
            userNameElement.gameObject.SetActive((displayFlags & DisplayFlags.UserName) != 0);
            userIconElement.gameObject.SetActive((displayFlags & DisplayFlags.UserIcon) != 0);
            textContainer.SetActive(effectNameElement.gameObject.activeSelf || userNameElement.gameObject.activeSelf);
        }

        protected internal override void Setup(CCEffectInstance effectInstance) {
            effectIcon.sprite = effectInstance.effect.Icon;
            effectIcon.color = effectInstance.effect.IconColor;

            if (effectName.gameObject.activeSelf)
                effectName.text = effectInstance.effect.Name;

            userIcon.sprite = effectInstance.user.profileIcon;
            userName.text = effectInstance.user.name;

            if (specialStatusContainer != null) {
                if (effectInstance.user.roles.Length > 0) {
                    specialStatus.text = effectInstance.user.roles[0].ToUpper();
                    specialStatusContainer.SetActive(true);
                }
                else {
                    specialStatusContainer.SetActive(false);
                }
            }

            if (coinsSpent != null)
                coinsSpent.text = effectInstance.effect.Price.ToString();

            if (textSizeFitter != null && textSizeFitter.gameObject.activeSelf)
                textSizeFitter.UpdateLayout();
        }
    }
}
