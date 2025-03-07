using UnityEngine;
using UnityEngine.UI; // Add this for Image component

#pragma warning disable 1591 
namespace WarpWorld.CrowdControl.Overlay
{
    [AddComponentMenu("Crowd Control/Effect Buff UI")]
    public abstract class EffectBuffUI : EffectLogUI
    {
        protected RectTransform timeBar;
        protected RectTransform timeBarParent;
        protected RectTransform contentContainer;
        protected Image timeBarImage;
        protected const float TIME_BAR_WIDTH = 80f; // Adjust this value as needed
        private bool initialized = false;

        protected internal abstract void UpdateEffectTimer();

        internal protected override void Setup(CCEffectInstance effectInstance)
        {
            base.Setup(effectInstance);
            Debug.Log("EffectBuffUI Setup called");
            InitializeTimeBar();
        }

        protected virtual void Start()
        {
            Debug.Log("EffectBuffUI Start called");
            InitializeTimeBar();
        }

        protected virtual void OnEnable()
        {
            Debug.Log("EffectBuffUI OnEnable called");
            InitializeTimeBar();
        }

        private void InitializeTimeBar()
        {
            if (initialized) return;
            initialized = true;

            Debug.Log("Initializing time bar...");

            // Find the root canvas
            Canvas rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas != null)
            {
                Debug.Log($"Found root canvas: {rootCanvas.name}");
            }
            else
            {
                Debug.LogError("Could not find root canvas!");
            }

            // Log the entire parent hierarchy to help debug
            Transform current = transform;
            string hierarchy = "UI Hierarchy: ";
            while (current != null)
            {
                hierarchy += current.name + " -> ";
                current = current.parent;
            }
            Debug.Log(hierarchy);

            // Initialize components
            timeBar = GetComponentInChildren<RectTransform>(true).Find("TimeBar") as RectTransform;
            if (timeBar == null)
            {
                Debug.LogError("Could not find TimeBar!");
                return;
            }

            timeBarParent = timeBar.parent as RectTransform;
            if (timeBarParent == null)
            {
                Debug.LogError("TimeBar has no parent!");
                return;
            }

            contentContainer = transform.Find("Content") as RectTransform;
            timeBarImage = timeBar.GetComponent<Image>();

            Debug.Log($"Found components: TimeBar={timeBar != null}, Parent={timeBarParent != null}, Image={timeBarImage != null}");

            if (timeBarImage != null)
            {
                // Make the timer bar invisible while keeping everything else active
                timeBarImage.color = new Color(0, 0, 0, 0); // Fully transparent
            }

            Debug.Log("Time bar initialization complete (only progress bar hidden)");
        }

        protected virtual void Update()
        {
            UpdateEffectTimer();

            if (!initialized)
            {
                InitializeTimeBar();
            }

            // Ensure the time bar (progress bar) remains invisible
            if (timeBarImage != null)
            {
                timeBarImage.color = new Color(0, 0, 0, 0);
            }
        }

        protected internal override void SetVisibility(DisplayFlags displayFlags)
        {
            if (!initialized)
            {
                InitializeTimeBar();
            }

            var userElements = GetComponentsInChildren<RectTransform>(true);
            foreach (var element in userElements)
            {
                if (element.gameObject.name.Contains("User"))
                {
                    bool showUserElements =
                        (displayFlags & (DisplayFlags.UserIcon | DisplayFlags.UserName)) != 0;
                    element.gameObject.SetActive(showUserElements);
                }
            }
        }
    }
}
