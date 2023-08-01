using UnityEngine;

#pragma warning disable 1591 
namespace WarpWorld.CrowdControl.Overlay {
    [AddComponentMenu("Crowd Control/Effect Buff UI")]
    public abstract class EffectBuffUI : EffectLogUI {
        protected internal abstract void UpdateEffectTimer();

        protected virtual void Update() {
            UpdateEffectTimer();
        }
    }
}
