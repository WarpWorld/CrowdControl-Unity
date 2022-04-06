using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarpWorld.CrowdControl
{
    public abstract class CCEffectItem : CCEffectBase
    {
        public CCEffectItem(string name, uint identifier)
        {
            displayName = name;
            identifier = this.identifier;
        }

        protected internal sealed override EffectResult OnTriggerEffect(CCEffectInstance effectInstance)
        {
            return EffectResult.Failure;
        }
    }
}
