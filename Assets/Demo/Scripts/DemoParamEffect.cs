using WarpWorld.CrowdControl;
using UnityEngine;
using System;

[System.Serializable]
public class DemoParamEffect : CCEffectParameters
{
    // If the Effect Entries are correct set, these will be written to upon receiving.
    public string m_itemName;
    public uint m_price;

    void Start() {
        Type t = typeof(DemoParamEffect);
        CrowdControl.instance.TestEffect(this);
    }

    protected override EffectResult OnTriggerEffect(CCEffectInstance effectInstance) {
        return TriggerEffect(effectInstance, EffectResult.Success);
    }

    EffectResult TriggerEffect(CCEffectInstance effectInstance, EffectResult result) {
        Debug.LogFormat("[CC DEMO EFFECT]: Triggered Parameter Effect: {0} {1}", displayName, result);
        return result;
    }

    public override void AssignParameters(string[] prms)
    {
        m_itemName = prms[0];
        m_price = Convert.ToUInt32(prms[1]);

        Debug.LogFormat("[CC DEMO EFFECT]: Name is {0} and its price is {1}", m_itemName, m_price);
    }
}
