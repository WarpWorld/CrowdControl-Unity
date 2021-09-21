using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace WarpWorld.CrowdControl.Overlay
{
    [AddComponentMenu("Crowd Control/Message UI")]
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class MessageUI : MonoBehaviour
    {
        private class MessageEntry
        {
            public string text;
            public Sprite sprite;
            public float lifeSpan;

            public MessageEntry(string text, Sprite sprite, float lifeSpan)
            {
                this.text = text;
                this.sprite = sprite;
                this.lifeSpan = lifeSpan;
            }
        }

        [SerializeField] private Image icon;
        [SerializeField] private Text content;
        [SerializeField] private GameObject container;
        [SerializeField] private GameObject iconContainer;
        [SerializeField] private CanvasGroup canvasGroup;

        private float timeRemaining = 0.0f;

        private Queue<MessageEntry> messageEntries = new Queue<MessageEntry>();
        private MessageEntry activeMessageEntry;

        void Awake()
        {
            canvasGroup.alpha = 0;
        }

        public void SetVisibility(DisplayFlags displayFlags)
        {
            canvasGroup.alpha = (displayFlags & DisplayFlags.Messages) != 0 ? 1 : 0;
        }

        public void Add(string message, Sprite sprite, float time)
        {
            MessageEntry newMessageEntry = new MessageEntry(message, sprite, time);

            if (activeMessageEntry == null)
                SetupMessage(newMessageEntry);
            else
                messageEntries.Enqueue(newMessageEntry);
        }

        public void Update()
        {
            if (timeRemaining <= 0.0f)
                return;

            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 0.0f)
            {
                if (messageEntries.Count > 0)
                    SetupMessage(messageEntries.Dequeue());
                else
                    ClearMessage();
            }
        }

        private void SetupMessage(MessageEntry messageEntry)
        {
            activeMessageEntry = messageEntry;
            timeRemaining = messageEntry.lifeSpan;
            content.text = messageEntry.text;

            bool hasIcon = messageEntry.sprite != null;

            if (hasIcon)
                icon.sprite = messageEntry.sprite;

            iconContainer.gameObject.SetActive(hasIcon);
        }

        private void ClearMessage()
        {
            activeMessageEntry = null;
            canvasGroup.alpha = 0;
        }
    }
}
