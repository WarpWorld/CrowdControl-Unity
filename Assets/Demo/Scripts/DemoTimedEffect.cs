using WarpWorld.CrowdControl;
using UnityEngine;

class DemoTimedEffect : CCEffectTimed {
    void Start() {
        CrowdControl.instance.TestEffect(this);
    }

    protected override EffectResult OnStartEffect(CCEffectInstanceTimed effectInstance) {
        return StartEffect(effectInstance, EffectResult.Success);
    }

    protected override bool OnStopEffect(CCEffectInstanceTimed effectInstance, bool force) {
        return StopEffect(effectInstance, force);
    }

    protected override void OnPauseEffect()
    {
        Debug.LogFormat("[CC DEMO EFFECT]: Pause Timer: {0}", displayName);
    }

    protected override void OnResumeEffect()
    {
        Debug.LogFormat("[CC DEMO EFFECT]: Resume Timer: {0}", displayName);
    }

    protected override void OnResetEffect()
    {
        Debug.LogFormat("[CC DEMO EFFECT]: Reset Timer: {0}", displayName);
    }

    EffectResult StartEffect(CCEffectInstance effectInstance, EffectResult result) {
        Debug.LogFormat("[CC DEMO EFFECT]: Start Timer: {0}", displayName);

        return result;
    }

    bool StopEffect(CCEffectInstance effectInstance, bool success) {
        Debug.LogFormat("[CC DEMO EFFECT]: End Timer: {0}", displayName);

        return success;
    }
}
