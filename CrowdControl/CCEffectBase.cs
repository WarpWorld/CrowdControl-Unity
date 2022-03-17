﻿using System;
using UnityEngine;
using System.Collections;

namespace WarpWorld.CrowdControl
{
    /// <summary> Basic Crowd Control effect properties. </summary>
    [System.Serializable]
    public abstract class CCEffectBase : MonoBehaviour
    {
#pragma warning disable 1591
#pragma warning disable 1587
        [Tooltip("Image to display in the CrowdControl Twitch extension and in the onscreen overlay.")]
        /// <summary>Image to display in the CrowdControl Twitch extension and in the onscreen overlay. </summary>
        public Sprite icon;

        [Tooltip("Color used to tint the effect's icon.")]
        /// <summary>Color used to tint the effect's icon. </summary>
        public Color iconColor = Color.white;

        [Tooltip("Unique identifier of the effect.")]
        /// <summary>Unique identifier of the effect. </summary>
        public uint identifier;

        [Tooltip("Name of the effect displayed to the users.")]
        /// <summary>Name of the effect displayed to the users. </summary>
        public string displayName;

        [TextArea]
        [Tooltip("Information about the effect, displayed in the extension.")]
        /// <summary>Information about the effect, displayed in the extension. </summary>
        public string description;

        [Range(0, 60)]
        [Tooltip("Number of retries before the effect instance fails.")]
        /// <summary>Number of retries before the effect instance fails. </summary>
        public int maxRetries = 3;

        [Range(0, 10)]
        [Tooltip("Delay in seconds before retrying to trigger an effect instance.")]
        /// <summary>Delay in seconds before retrying to trigger an effect instance. </summary>
        public float retryDelay = 5;

        [Range(0, 10)]
        [Tooltip("Delay in seconds to wait before triggering the next effect instance.")]
        /// <summary>Delay in seconds to wait before triggering the next effect instance. </summary>
        public float pendingDelay = .5f;
#pragma warning restore 1587
#pragma warning restore 1591

        // Wait until this time before triggering the next effect instance. Used by CrowdControl.TryStart.
        internal float delayUntilUnscaledTime = 0.0f;

        /// <summary>
        /// Called when an effect instance is scheduled for execution. The returned value is communicated back to the server.
        /// <para>If <see cref="EffectResult.Retry"/> is returned, will be called again in <see cref="retryDelay"/> seconds,
        /// unless <see cref="maxRetries"/> is reached.</para>
        /// </summary>
        protected internal abstract EffectResult OnTriggerEffect(CCEffectInstance effectInstance);

        // Register the effect
        private void Awake() 
        {
            if (CrowdControl.instance != null && CrowdControl.instance.EffectIsRegistered(this))
            {
                return;
            }

            StartCoroutine(RegisterEffect());
        }

        protected IEnumerator RegisterEffect()
        {
            while (CrowdControl.instance == null)
                yield return new WaitForSeconds(1.0f);

            CrowdControl.instance.RegisterEffect(this);
        }

        /// <summary>Byte size of the playload for this effect. Can be overridden by a derived class.</summary>
        public virtual ushort PayloadSize(string userName)
        {
            return Convert.ToUInt16(3 + 4 + 4 + 4 + userName.Length + 1 + 1);
        }

        /// <summary>Additional Info for the effect. Can be overridden by a derived class.</summary>
        public virtual string Params()
        {
            return String.Empty;
        }

        /// <summary>Toggles whether this effect can currently be sold during this session.</summary>
        public void ToggleSellable(bool sellable)
        {
            CrowdControl.instance.ToggleEffectSellable(identifier, sellable);
        }

        /// <summary>Toggles whether this effect is visible in the menu during this session.</summary>
        public void ToggleVisible(bool visible)
        {
            CrowdControl.instance.ToggleEffectVisible(identifier, visible);
        }

        /// <summary>Determines whether this effect can be ran right now or not.</summary>
        public virtual bool CanBeRan()
        {
            return true;
        }
    }
}
