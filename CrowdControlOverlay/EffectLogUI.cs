using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

#pragma warning disable 1591
namespace WarpWorld.CrowdControl.Overlay {
    [AddComponentMenu("Crowd Control/Effect Log UI")]
    public class EffectLogUI : EffectUINode {
        [SerializeField] protected TMP_Text effectName;
        [SerializeField] protected Image effectIcon;

        [Space]
        [SerializeField] protected TMP_Text userName;
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
        [SerializeField] protected TMP_Text specialStatus;
        [SerializeField] protected TMP_Text coinsSpent;
        [SerializeField] protected RectTransform underlay;
        [SerializeField] protected RectTransform mainContent;
        [SerializeField] protected RectTransform container;

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

            if (effectName.gameObject.activeSelf)
                effectName.text = effectInstance.effect.Name;

            userIcon.sprite = effectInstance.user.profileIcon;
            userName.text = effectInstance.user.name;

            if (specialStatusContainer != null) {
                if (effectInstance.user.roles.Length == 0) {
                    specialStatus.text = effectInstance.user.roles[0].ToUpper();
                }
                else {
                    specialStatusContainer.SetActive(false);
                }
            }

            if (coinsSpent != null)
                coinsSpent.text = effectInstance.effect.price.ToString(); 

            if (textSizeFitter.gameObject.activeSelf)
                textSizeFitter.UpdateLayout();

            //StartCoroutine(TweenGroups());
        }

        private IEnumerator Tween(RectTransform rectTrans) {
            Vector3 targetPosition = new Vector3(rectTrans.position.x + container.rect.width, rectTrans.position.y, rectTrans.position.z);

            while (rectTrans.position.x < targetPosition.x) {
                rectTrans.transform.position = new Vector3(Mathf.Lerp(rectTrans.transform.position.x, targetPosition.x + 100.0f, 1.0f * Time.deltaTime), rectTrans.transform.position.y, rectTrans.transform.position.z);
                yield return new WaitForSeconds(1); 
            }

            //rectTrans.transform.position = new Vector3(rectTrans.transform.position.x - container.rect.width, rectTrans.transform.position.y, rectTrans.transform.position.z);
            yield break;
        }

        private IEnumerator TweenGroups() {
            yield return new WaitForEndOfFrame();
            underlay.transform.position = new Vector3(underlay.transform.position.x - container.rect.width, underlay.transform.position.y, underlay.transform.position.z);
            mainContent.transform.position = new Vector3(mainContent.transform.position.x - container.rect.width, mainContent.transform.position.y, mainContent.transform.position.z);
            StartCoroutine(Tween(underlay));
            yield return new WaitForSeconds(1);
            StartCoroutine(Tween(mainContent)); 
        }
    }
}
