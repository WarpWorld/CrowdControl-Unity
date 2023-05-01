using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarpWorld.CrowdControl
{
    public abstract class CCEffectItem : CCEffectBase
    {
        public CCEffectItem(string name, string effectKey)
        {
            displayName = name;
            effectKey = this.effectKey;
        }

        protected internal sealed override EffectResult OnTriggerEffect(CCEffectInstance effectInstance)
        {
            return EffectResult.Failure;
        }
    }
}
