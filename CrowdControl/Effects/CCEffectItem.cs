﻿namespace WarpWorld.CrowdControl {
    public abstract class CCEffectItem : CCEffectBase {
        public CCEffectItem(string name, string effectKey) {
            displayName = name;
            effectKey = this.key;
        }

        protected internal sealed override EffectResult OnTriggerEffect(CCEffectInstance effectInstance) {
            return EffectResult.Failure;
        }
    }
}
