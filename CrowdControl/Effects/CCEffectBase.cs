using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using Newtonsoft.JsonCC;

namespace WarpWorld.CrowdControl {
    /// <summary> Basic Crowd Control effect properties. </summary>
    [System.Serializable]
    public abstract class CCEffectBase : MonoBehaviour {
#pragma warning disable 1591
#pragma warning disable 1587
        [SerializeField] [HideInInspector] private bool noPooling = false;
        [SerializeField] [HideInInspector] private bool sellable = true;
        [SerializeField] [HideInInspector] private bool visible = true;
        [SerializeField] [HideInInspector] private Morality morality = Morality.Neutral;
        [SerializeField] [HideInInspector] private Orderliness orderliness = Orderliness.Neutral;
        [SerializeField] [HideInInspector] private Sprite icon;
        [SerializeField] [HideInInspector] private Color iconColor = Color.white;
        [SerializeField] [HideInInspector] protected string id;
        [SerializeField] [HideInInspector] protected string displayName; 
        [SerializeField] [HideInInspector] private string description; 
        [SerializeField] [HideInInspector] private uint price = 10;
        [Range(0, 60)] [SerializeField] [HideInInspector] private int maxRetries = 3;
        [Range(0, 10)] [SerializeField] [HideInInspector] private float retryDelay = 5;
        [Range(0, 10)] [SerializeField] [HideInInspector] private float pendingDelay = .5f;
        [SerializeField] [HideInInspector] private uint sessionMax = 0;
#pragma warning restore 1587
#pragma warning restore 1591

        /// <summary>Wait until this time before triggering the next effect instance. Used by CrowdControl.TryStart. </summary>
        internal float delayUntilUnscaledTime = 0.0f;

        /// <summary>The effect's icon.</summary>
        public virtual Sprite Icon { get { return icon; } }

        /// <summary>The effect's unique id.</summary>
        public string ID { get { return id; } }

        /// <summary>The name of this effect.</summary>
        public virtual string Name { get { return displayName; } }

        /// <summary>Information about the effect, displayed in the extension. </summary>
        public string Description { get { return description; } }

        /// <summary>Categories for this effect.</summary>
        public string [] Categories;

        /// <summary>Disables pooling for this effect.</summary>
        public bool NoPooling { get { return noPooling; } }

        // <summary>Denotes whether this effect intends to help the player, hurt the player, or act as neutral.</summary>
        public Morality Morality { get { return morality; } }
        // <summary>Denotes whether this effect intends to help the player, hurt the player, or act as neutral.</summary>
        public Orderliness Orderliness { get { return orderliness; } }

        /// <summary>If true, the effect will be inaccessible to everyone but Warp World staff, unless turned off later.</summary>
        public bool Visible { get { return visible; } }

        /// <summary>If true, this effect will appear but not be sellable unless turned off later.</summary>
        public bool Sellable { get { return sellable; } }

        /// <summary>The color tint of this effect's icon</summary>
        public virtual Color IconColor { get { return iconColor; } }

        /// <summary>Number of retries before the effect instance fails. </summary>
        public int MaxRetries { get { return maxRetries; } }

        /// <summary>Delay in seconds before retrying to trigger an effect instance. </summary>
        public float RetryDelay { get { return retryDelay; } }

        /// <summary>Delay in seconds to wait before triggering the next effect instance. </summary>
        public float PendingDelay { get { return pendingDelay; } }

        /// <summary>Delay in seconds to wait before triggering the next effect instance. </summary>
        public uint Price { get { return price; } }

        /// <summary>How many times can this effect be used during one session? </summary>
        public uint SessionMax { get { return sessionMax; } }

        /// <summary>Toggles whether this effect can currently be sold during this session.</summary>
        public void ToggleSellable(bool sellable) {
            if (sellable) {
                SendUpdate("menuAvailable");
                this.sellable = true;
                return;
            }

            SendUpdate("menuUnavailable");
            this.sellable = false;
        }

        /// <summary>Toggles whether this effect is visible in the menu during this session.</summary>
        public void ToggleVisible(bool visible)  {
            if (visible) {
                SendUpdate("menuVisible");
                this.visible = true;
                return;
            }

            SendUpdate("menuHidden");
            this.visible = false;
        }

        /// <summary>Updates the price on the effect menu during runtime.</summary>
        public void UpdatePrice(uint newPrice) {
            price = newPrice;
            ServerMessages.SendPost("menu/effects", null, new JSONEffectChangePrice(id, price), false);
        }

        /// <summary>Updates whether this effect is poolable or not.</summary>
        public void UpdateNonPoolable(bool newNonPoolable) {
            noPooling = newNonPoolable;
            ServerMessages.SendPost("menu/effects", null, new JSONEffectChangeNonPoolable(id, noPooling), false);
        }

        /// <summary>Updates the amount of times you can use this effect during runtime.</summary>
        public void UpdateSessionMax(uint newSessionMax) {
            sessionMax = newSessionMax;
            ServerMessages.SendPost("menu/effects", null, new JSONEffectChangeSessionMax(id, sessionMax), false);
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

        protected IEnumerator RegisterEffect() {
            while (CrowdControl.instance == null)
                yield return new WaitForSeconds(1.0f);

            CrowdControl.instance.RegisterEffect(this);
        }

        /// <summary>Sets the internal ID of this effect.</summary>
        public string SetIdentifier() {
            Regex rgx = new Regex("[^a-z0-9-]");
            id = Name.ToLower();
            id = rgx.Replace(id, "");
            return id;
        }

        private void SendUpdate(string command) {
            JSONEffectReport effectReport = new JSONEffectReport(CrowdControl.instance.CurrentUserHash, this, command);
            CrowdControl.instance.SendJSON(new JSONData("rpc", JsonConvert.SerializeObject(effectReport)));
        }

        private void Awake() {
            SetIdentifier();

            if (CrowdControl.instance != null && CrowdControl.instance.EffectIsRegistered(this)) 
                return;

            StartCoroutine(RegisterEffect());
        }
    }
}
