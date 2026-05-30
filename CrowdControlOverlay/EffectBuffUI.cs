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

        [SerializeField] float pausedAlpha = 0.45f;

        private CCEffectInstanceTimed effectInstanceTimed;
        private CCEffectTimed effect;
        private Image timeFillImage;
        private bool barSized;

        protected void Update() {
            UpdateEffectTimer();
        }

        protected internal void UpdateEffectTimer() {
            if (effectInstanceTimed == null || effect == null)
                return;

            UpdatePausedVisual();

            if (timeContainer == null || !timeContainer.activeSelf)
                return;

            EnsureBarSized();
            UpdateTimerDisplay();
        }

        void UpdatePausedVisual() {
            group.alpha = effect.Paused ? pausedAlpha : 1f;
        }

        void EnsureBarSized() {
            if (barSized || timeBar == null || container == null)
                return;

            LayoutRebuilder.ForceRebuildLayoutImmediate(container);
            float width = container.rect.width;
            timeBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            barSized = true;
        }

        void UpdateTimerDisplay() {
            float timeLeft = effectInstanceTimed.unscaledTimeLeft;
            int seconds = Convert.ToInt32(timeLeft) % 60;
            int minutes = Convert.ToInt32(timeLeft) / 60;

            if (timeLabel != null)
                timeLabel.text = string.Format("{0}:{1}", minutes, seconds.ToString("D2"));

            float percentLeft = effect.Duration > 0f
                ? Mathf.Clamp01(timeLeft / effect.Duration)
                : 0f;

            if (timeFillImage != null && timeFillImage.type == Image.Type.Filled)
                timeFillImage.fillAmount = percentLeft;
            else if (timeFillBar != null && timeBar != null) {
                float width = timeBar.rect.width;
                timeFillBar.sizeDelta = new Vector2(width * percentLeft, timeFillBar.sizeDelta.y);
            }

            if (progress != null)
                progress.fillAmount = percentLeft;
        }

        protected internal override void SetVisibility(DisplayFlags displayFlags) {
            base.SetVisibility(displayFlags);
            gameObject.SetActive((displayFlags & DisplayFlags.Buff) != 0);
        }

        protected internal override void Setup(CCEffectInstance effectInstance) {
            base.Setup(effectInstance);

            effectInstanceTimed = effectInstance as CCEffectInstanceTimed;
            if (effectInstanceTimed == null)
                return;

            effect = effectInstanceTimed.effect;
            barSized = false;

            if (timeFillBar != null)
                timeFillImage = timeFillBar.GetComponent<Image>();

            if (progress != null)
                progress.fillAmount = 1f;

            if (timeFillImage != null)
                timeFillImage.fillAmount = 1f;

            if (timeContainer != null)
                timeContainer.SetActive(true);

            group.alpha = 1f;
        }
    }
}
