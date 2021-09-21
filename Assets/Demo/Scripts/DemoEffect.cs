using WarpWorld.CrowdControl;
using UnityEngine;
using System.Collections;

class DemoEffect : CCEffect {
    void Start()
    {
        StartCoroutine(StartEffectDelay());
    }

    IEnumerator StartEffectDelay()
    {
        yield return new WaitForSeconds(3.0f);
        CrowdControl.instance.TestEffect(this);
    }

    protected override EffectResult OnTriggerEffect(CCEffectInstance effectInstance) {
        return TriggerEffect(effectInstance, EffectResult.Success);
    }

    EffectResult TriggerEffect(CCEffectInstance effectInstance, EffectResult result) {
        Debug.LogFormat("[CC DEMO EFFECT]: Triggered Effect: {0} {1}", displayName, result);
        return result;
    }
}
