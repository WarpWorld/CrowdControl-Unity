using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 1591
namespace WarpWorld.CrowdControl.Overlay {
    [AddComponentMenu("Crowd Control/Effect Queue UI")]
    public class EffectQueueUI : EffectUINode {
        [SerializeField] protected Image effectIcon;
        [SerializeField] protected Text count;
        [SerializeField] protected GameObject container;

        private byte total;

        protected internal override void Setup(CCEffectInstance effectInstance)
        {
            this.effectInstance = effectInstance;
            effectIcon.sprite = effectInstance.effect.icon;
            effectIcon.color = effectInstance.effect.iconColor;
            total = 1;
            SetTotal();
        }

        protected internal override void SetVisibility(DisplayFlags displayFlags)
        {
            gameObject.SetActive((displayFlags & DisplayFlags.Queue) != 0);
        }

        protected internal override void Add(CCEffectInstance effectInstance)
        {
            total++;
            SetTotal();
        }

        private void SetTotal()
        {
            count.text = total.ToString();
        }

        protected internal override bool Remove()
        {
            total--;
            if (total == 0) return true;
            SetTotal();
            return false;
        }
    }
}
