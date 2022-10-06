using UnityEngine;
using UnityEngine.UI;
using System;

#pragma warning disable 1591
namespace WarpWorld.CrowdControl.Overlay {
    [AddComponentMenu("Crowd Control/Effect Buff UI")]
    public class EffectBuffUI : EffectLogUI { 
        [Space]
        [SerializeField] protected Image progress;
        [SerializeField] protected GameObject timeContainer;
        [SerializeField] protected Text timeLabel;

        private CCEffectInstanceTimed effectInstanceTimed;
        private CCEffectTimed effect;
        
        protected void LateUpdate()
        {
            if (effectInstanceTimed == null || effect == null || effect.paused)
                return;

            if (effect.displayType == CCEffectTimed.DisplayType.Fill)
                AdjustFill();
            else
                PrintTime();
        }

        protected internal override void SetVisibility(DisplayFlags displayFlags)
        {
            base.SetVisibility(displayFlags);
            gameObject.SetActive((displayFlags & DisplayFlags.Buff) != 0);
        }

        protected internal override void Setup(CCEffectInstance effectInstance) {
            base.Setup(effectInstance);
            
            effectInstanceTimed = effectInstance as CCEffectInstanceTimed;
            effect = effectInstanceTimed.effect;
            progress.fillAmount = 0;
            
            if (effectInstanceTimed.effect.displayType != CCEffectTimed.DisplayType.Timer)
            {
                return;
            }

            timeContainer.gameObject.SetActive(true);
        }

        private void AdjustFill()
        {
            var duration = effect.duration;
            progress.fillAmount = (duration - effectInstanceTimed.unscaledTimeLeft) / duration;
        }

        private void PrintTime()
        {
            int seconds = Convert.ToInt32(effectInstanceTimed.unscaledTimeLeft) % 60;
            int minutes = Convert.ToInt32(effectInstanceTimed.unscaledTimeLeft) / 60;

            timeLabel.text = String.Format("{0}:{1}", minutes, seconds.ToString("D2"));
        }
    }
}
