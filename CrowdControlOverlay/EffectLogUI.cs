using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

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
        
        private Dictionary<string, Sprite> _spriteDictionary = new Dictionary<string, Sprite>();

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
            effectName.text = effectInstance.effect.Name;
            userIcon.color = effectInstance.user.profileIconColor;
            userIcon.sprite = effectInstance.user.profileIcon;
            userName.text = effectInstance.user.displayName;

            if (textSizeFitter.gameObject.activeSelf)
            {
                textSizeFitter.UpdateLayout();
            }
        }
    }
}
