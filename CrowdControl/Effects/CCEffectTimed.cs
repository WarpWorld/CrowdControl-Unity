using UnityEngine;

namespace WarpWorld.CrowdControl {
    /// <summary> A Crowd Control effect active for a given duration of time. </summary>
    public abstract class CCEffectTimed : CCEffectBase {
#pragma warning disable 1591
        /// <summary>Options of how this timer displays time left. </summary>
        public enum DisplayType
        {
            Fill,
            Timer
        }

        [Range(1, 600)]
        [Tooltip("Duration in seconds before the effect is automatically ended.")]
        /// <summary>Duration in seconds before the effect is automatically ended. </summary>
        public float duration = 60;

        /// <summary>Is the timer paused?</summary>
        public bool paused { get; internal set; }

        /// <summary> Whether the timer displays text or a fill graphic. </summary>
        public DisplayType displayType;

#pragma warning disable 1591
        /// <summary>Ran when the effect is enabled. Can be overridden by a derived class.</summary>
        protected virtual void OnEnable () => CrowdControl.EnableEffect (this);
        /// <summary>Ran when the effect is disabled. Can be overridden by a derived class.</summary>
        protected virtual void OnDisable() => CrowdControl.DisableEffect(this);
#pragma warning restore 1591
        /// <summary> Handles starting the timed effect. Override <see cref="OnStartEffect(EffectInstance)"/> instead. </summary>
        protected internal sealed override EffectResult OnTriggerEffect(CCEffectInstance effectInstance) {
            paused = false;

            CCEffectInstanceTimed effectInstanceTimed = effectInstance as CCEffectInstanceTimed;
            effectInstanceTimed.unscaledEndTime = effectInstance.unscaledStartTime + duration;
            effectInstanceTimed.unscaledTimeLeft = duration;

            var result = OnStartEffect(effectInstanceTimed);
            if (result != EffectResult.Success) return result;
            return EffectResult.Running;
        }

        /// <summary> Invoked when an effect instance is scheduled to start. The effect should only be applied when <see cref="EffectResult.Success"/> is returned. </summary>
        protected internal abstract EffectResult OnStartEffect(CCEffectInstanceTimed effectInstance);

        /// <summary>
        /// Invoked when an effect instance is scheduled to stop after its time is expired. Return false
        /// if the effect cannot be stopped at the current time to retry in <see cref="CCEffectBase.retryDelay"/>
        /// seconds.
        /// </summary>
        /// <param name="effectInstance">The target instance.</param>
        /// <param name="force">Set to true when called from <see cref="CrowdControl.StopAllEffects"/>,
        /// in which case failure to stop the effect is ignored.</param>
        protected internal abstract bool OnStopEffect(CCEffectInstanceTimed effectInstance, bool force);

        /// <summary> Invoked when an effect instance is paused. </summary>
        protected internal abstract void OnPauseEffect();
        /// <summary> Invoked when an effect instance is resumed. </summary>
        protected internal abstract void OnResumeEffect();
        /// <summary> Invoked when an effect instance is reset. </summary>
        protected internal abstract void OnResetEffect();

        /// <summary> Checks if the effect should be running or not, then applies the paused state based on it. </summary>
        public bool ShouldBeRunning()
        {
            return RunningCondition();
        }

        /// <summary> Invoked when the behaviour is paused. </summary>
        protected internal void Pause(CCEffectInstance effectInstance)
        {
            paused = true;
        }

        /// <summary> Invoked when the behaviour is resumed. </summary>
        protected internal void Resume(CCEffectInstance effectInstance)
        {
            CCEffectInstanceTimed effectInstanceTimed = effectInstance as CCEffectInstanceTimed;
            effectInstanceTimed.unscaledTimeLeft += Time.unscaledDeltaTime;
            paused = false;
        }

        /// <summary> Invoked when the behaviour is Reset. </summary>
        protected internal void Reset(CCEffectInstance effectInstance)
        {
            CCEffectInstanceTimed effectInstanceTimed = effectInstance as CCEffectInstanceTimed;
            effectInstanceTimed.unscaledTimeLeft = duration;
            paused = false;
        }

        /// <summary> Conditions to be ran. </summary>
        protected virtual bool RunningCondition()
        {
            return paused;
        }
    }
}
