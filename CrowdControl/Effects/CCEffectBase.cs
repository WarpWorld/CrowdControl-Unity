using System;
using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

namespace WarpWorld.CrowdControl
{
    /// <summary> Basic Crowd Control effect properties. </summary>
    [System.Serializable]
    public abstract class CCEffectBase : MonoBehaviour
    {
#pragma warning disable 1591
#pragma warning disable 1587
        [Tooltip("Image to display in the CrowdControl Twitch extension and in the onscreen overlay.")]
        /// <summary>Disables pooling for this effect.</summary>
        [HideInInspector] public bool noPooling = false;

        /// <summary>If true, the effect will be inaccessible to everyone but Warp World staff.</summary>
        [HideInInspector] public bool disabled = false;

        // <summary>Denotes whether this effect intends to help the player, hurt the player, or act as neutral.</summary>
        [HideInInspector] public Morality morality;

        /// <summary>Whether this effect is available to the streamer by default or not.</summary>
        [HideInInspector] public bool inactive = false;

        /// <summary>Image to display in the CrowdControl Twitch extension and in the onscreen overlay. </summary>
        [HideInInspector] public Sprite icon;

        [Tooltip("Color used to tint the effect's icon.")]
        /// <summary>Color used to tint the effect's icon. </summary>
        [HideInInspector] public Color iconColor = Color.white;

        /// <summary>Unique identifier of the effect. </summary>
        [HideInInspector]
        public string effectKey;

        [Tooltip("Name of the effect displayed to the users.")]
        /// <summary>Name of the effect displayed to the users. </summary>
        [HideInInspector] public string displayName;

        [TextArea]
        [Tooltip("Information about the effect, displayed in the extension.")]
        /// <summary>Information about the effect, displayed in the extension. </summary>
        [HideInInspector] public string description;

        [Tooltip("The price it costs to activate this effect")]
        /// <summary>Information about the effect, displayed in the extension. </summary>
        [HideInInspector] public uint price = 10;

        [Range(0, 60)]
        [Tooltip("Number of retries before the effect instance fails.")]
        /// <summary>Number of retries before the effect instance fails. </summary>
        [HideInInspector] public int maxRetries = 3;

        [Range(0, 10)]
        [Tooltip("Delay in seconds before retrying to trigger an effect instance.")]
        /// <summary>Delay in seconds before retrying to trigger an effect instance. </summary>
        [HideInInspector] public float retryDelay = 5;

        [Range(0, 10)]
        [Tooltip("Delay in seconds to wait before triggering the next effect instance.")]
        /// <summary>Delay in seconds to wait before triggering the next effect instance. </summary>
        [HideInInspector] public float pendingDelay = .5f;
#pragma warning restore 1587
#pragma warning restore 1591

        // Wait until this time before triggering the next effect instance. Used by CrowdControl.TryStart.
        internal float delayUntilUnscaledTime = 0.0f;

        /// <summary>The effect's icon.</summary>
        [HideInInspector] public virtual Sprite Icon { get { return icon; } }

        /// <summary>The name of this effect.</summary>
        [HideInInspector] public virtual string Name { get { return displayName; } }

        /// <summary>Categories for this effect.</summary>
        [HideInInspector] public string [] Categories;

        /// <summary>If true, the effect will be unavailable to the streamer by default.</summary>
        [HideInInspector] public virtual bool Inactive { get { return inactive; } }

        /// <summary>Disables pooling for this effect.</summary>
        [HideInInspector] public bool NoPooling { get { return noPooling; } }

        // <summary>Denotes whether this effect intends to help the player, hurt the player, or act as neutral.</summary>
        [HideInInspector] public Morality Morality { get { return morality; } }

        /// <summary>If true, the effect will be inaccessible to everyone but Warp World staff.</summary>
        [HideInInspector] public virtual bool Disabled { get { return disabled; } }

        /// <summary>The color tint of this effect's icon</summary>
        [HideInInspector] public virtual Color IconColor { get { return iconColor; } }

        /// <summary>Additional Info for the effect. Can be overridden by a derived class.</summary>
        public virtual string Params() { return string.Empty; }

        /// <summary>Toggles whether this effect can currently be sold during this session.</summary>
        public void ToggleSellable(bool sellable)
        {
            CrowdControl.instance.ToggleEffectSellable(effectKey, sellable);
        }

        /// <summary>Toggles whether this effect is visible in the menu during this session.</summary>
        public void ToggleVisible(bool visible)
        {
            CrowdControl.instance.ToggleEffectVisible(effectKey, visible);
        }

        /// <summary>Determines whether this effect can be ran right now or not. Overridable</summary>
        public virtual bool CanBeRan() { return true; }

        /// <summary>Registers a paramter for this effect. Overridable</summary>
        public virtual void RegisterParameters(CCEffectEntries effectEntries) { }

        /// <summary>Returns true if this bid war is the parent of the parameter ID. Overridable</summary>
        public virtual bool HasParameterID(string id) { return false; }

        /// <summary>
        /// Called when an effect instance is scheduled for execution. The returned value is communicated back to the server.
        /// <para>If <see cref="EffectResult.Retry"/> is returned, will be called again in <see cref="retryDelay"/> seconds,
        /// unless <see cref="maxRetries"/> is reached.</para>
        /// </summary>
        protected internal abstract EffectResult OnTriggerEffect(CCEffectInstance effectInstance);

        protected IEnumerator RegisterEffect()
        {
            while (CrowdControl.instance == null)
                yield return new WaitForSeconds(1.0f);

            CrowdControl.instance.RegisterEffect(this);
        }

        /// <summary>Sets the internal ID of this effect.</summary>
        public string SetIdentifier() {
            Regex rgx = new Regex("[^a-z0-9-]");
            effectKey = Name.ToLower();
            effectKey = rgx.Replace(effectKey, "");
            return effectKey;
        }

        // Register the effect
        private void Awake() {
            SetIdentifier();

            if (CrowdControl.instance != null && CrowdControl.instance.EffectIsRegistered(this)) {
                return;
            }

            StartCoroutine(RegisterEffect());
        }
    }
}
