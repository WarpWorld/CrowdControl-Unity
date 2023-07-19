using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

#pragma warning disable 1591 
namespace WarpWorld.CrowdControl.Overlay {
    [AddComponentMenu("Crowd Control/Effect Buff UI")]
    public class EffectBuffUI : EffectLogUI { 
        [Space]
        [SerializeField] protected Image progress;
        [SerializeField] protected GameObject timeContainer;
        [SerializeField] protected TMP_Text timeLabel;
        [SerializeField] protected RectTransform textPanel;
        [SerializeField] protected RectTransform timePanel;
        [SerializeField] protected RectTransform timeBar;
        [SerializeField] protected RectTransform timeFillBar;

        private CCEffectInstanceTimed effectInstanceTimed;
        private CCEffectTimed effect;

        private float totalWidth = 0;
        private bool setPos = false;
        
        protected void LateUpdate() {
            if (effectInstanceTimed == null || effect == null || effect.paused)
                return;

            if (!setPos) {
                totalWidth = effectIconElement.rect.width + userIconElement.rect.width + textPanel.rect.width + timePanel.rect.width;
                mainContent.transform.localPosition = new Vector3(-5.0f - totalWidth, mainContent.transform.localPosition.y, mainContent.transform.localPosition.z);
                timeBar.transform.localPosition = new Vector3(-5.0f - totalWidth, mainContent.transform.localPosition.y, mainContent.transform.localPosition.z);
                timeBar.sizeDelta = new Vector2(totalWidth, 5);
                setPos = true;
            }

            PrintTime();
        }

        protected internal override void SetVisibility(DisplayFlags displayFlags) {
            base.SetVisibility(displayFlags);
            gameObject.SetActive((displayFlags & DisplayFlags.Buff) != 0);
        }

        protected internal override void Setup(CCEffectInstance effectInstance) {
            base.Setup(effectInstance);
            
            effectInstanceTimed = effectInstance as CCEffectInstanceTimed;
            effect = effectInstanceTimed.effect;
            progress.fillAmount = 0;
            
            if (effectInstanceTimed.effect.displayType != CCEffectTimed.DisplayType.Timer)
                return;

            timeContainer.gameObject.SetActive(true);
        }

        private void PrintTime() {
            int seconds = Convert.ToInt32(effectInstanceTimed.unscaledTimeLeft) % 60;
            int minutes = Convert.ToInt32(effectInstanceTimed.unscaledTimeLeft) / 60;

            timeLabel.text = String.Format("{0}:{1}", minutes, seconds.ToString("D2"));

            float percentLeft = 1.0f - (effect.duration - effectInstanceTimed.unscaledTimeLeft) / effect.duration;

            timeFillBar.sizeDelta = new Vector2(totalWidth * percentLeft, 5.0f);
        }
    }
}
