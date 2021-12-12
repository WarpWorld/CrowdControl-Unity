using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;

namespace WarpWorld.CrowdControl.Overlay
{
    /// <summary>Determines which parts of overlay elements to display.</summary>
    [Flags]
    public enum DisplayFlags
    {
        None = 0x00,
        /// <summary>Display the effect's name.</summary>
        EffectName = 1 << 0,
        /// <summary>Display the effect's icon.</summary>
        EffectIcon = 1 << 1,
        /// <summary>Display the user's name.</summary>
        UserName = 1 << 2,
        /// <summary>Display the user's icon.</summary>
        UserIcon = 1 << 3,
        /// <summary>Display the queue.</summary>
        Queue = 1 << 4,
        /// <summary>Display the buff.</summary>
        Buff = 1 << 5,
        /// <summary>Display the log.</summary>
        Log = 1 << 6,
        /// <summary>Display important messages.</summary>
        Messages = 1 << 7,
        All = ~0
    }

    /// <summary>
    /// Manages the Crowd Control overlay elements. Listens to events on <see cref="CrowdControl"/> and
    /// updates its UI accordingly.
    ///
    /// <para>There are three UI sections: a log panel to display messages, a buff panel to display timed effects
    /// and a queue panel to display pending effects. If both scene references for a section are unassigned, the
    /// corresponding panel is disabled.</para>
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Crowd Control/Overlay")]
    public sealed class CrowdControlOverlay : MonoBehaviour
    {
        #region Configuration

#pragma warning disable CS0649
        [Header("Scene References")]
        [SerializeField] EffectLogUI log;
        [SerializeField] EffectBuffUI buff;
        [SerializeField] EffectQueueUI queue;
        [SerializeField] MessageUI messages;
        [SerializeField] TokenUI token;

        [Space]
        [SerializeField] EffectPanelUI logPanel;
        [SerializeField] EffectPanelUI buffPanel;
        [SerializeField] EffectPanelUI queuePanel;
#pragma warning restore CS0649

        [Header("Configuration")]
        [Tooltip("Which parts to display on UI elements.")]
        
        [Attributes.EnumFlag]
        public DisplayFlags _displayFlags =
            DisplayFlags.EffectName |
            DisplayFlags.EffectIcon |
            DisplayFlags.UserName |
            DisplayFlags.UserIcon;

#pragma warning disable 1591
        public DisplayFlags displayFlags
        {
            get => _displayFlags;
            set
            {
                _displayFlags = value;
            }
        }

        [Tooltip("How long to display log elements for, in unscaled seconds.")]
        [Range(1, 30)] public float logDisplayTime = 5f;

        //[Space]
        //[Tooltip("")]
        //[Range(0, 5)] public float fadeTimeIn = 2.5f;
        //[Tooltip("")]
        //[Range(0, 5)] public float fadeTimeOut = 2f;

        [Space]
        [Range(1, 10)]
        [Tooltip("Maximum number of log elements displayed at once. Adding more elements will force the oldest to be removed.")]
        [SerializeField] int maxLogEntries = 5;

        [Range(0, 100)]
        [Tooltip("Maximum number of each prefab to keep instantiated.")]
        [SerializeField] internal int maxPoolSize = 16;
#pragma warning restore 1591

        #endregion

        #region State

        class LogEntry
        {
            public DateTime endTime;
            public uint effectID;
        }

        Queue<LogEntry> logEntries;
        LogEntry activeLogEntry;

        #endregion

        #region Unity Life Cycle

        internal static CrowdControlOverlay instance { get; private set; }

        void OnDestroy()
        {
            Assert.IsNotNull(instance);
            instance = null;
            logEntries = null;
        }

        void Awake()
        {
            Assert.IsNull(instance);
            instance = this;
            logEntries = new Queue<LogEntry>();
        }

        void Start()
        {
            var cc = CrowdControl.instance;
            if (!cc) return;

            log.gameObject.SetActive(false);
            cc.OnEffectTrigger += OnEffectTrigger;

            buffPanel.m_uiType = EffectPanelUI.UIType.Buff;
            buff.gameObject.SetActive(false);
            cc.OnEffectStart += OnEffectStart;
            cc.OnEffectStop += OnEffectStop;

            queue.gameObject.SetActive(false);
            queuePanel.m_uiType = EffectPanelUI.UIType.Queue;
            cc.OnEffectQueue += OnEffectQueue;
            cc.OnEffectDequeue += OnEffectDequeue;
            cc.OnDisplayMessage += OnDisplayMessage;
        }

        void LateUpdate()
        {
            if (activeLogEntry == null)
            {
                if (logEntries.Count == 0)
                    return;

                activeLogEntry = logEntries.Dequeue();
            }

            if (activeLogEntry.endTime.CompareTo(DateTime.Now) > 0)
                return;

            logPanel.Remove(activeLogEntry.effectID);
            activeLogEntry = null;
        }

        #endregion

        #region Effect Callbacks

        void OnEffectTrigger(CCEffectInstance effectInstance)
        {
            if ((_displayFlags & DisplayFlags.Log) == 0)
                return;

            LogEntry logEntry = new LogEntry();
            logEntry.effectID = effectInstance.id;
            logEntry.endTime = DateTime.Now.AddSeconds(logDisplayTime);

            if (logEntries.Count == (maxLogEntries - 1))
                activeLogEntry.endTime = DateTime.Now;

            logEntries.Enqueue(logEntry);
            logPanel.Setup(log, effectInstance, _displayFlags);
        }

        void OnDisplayMessage(string message, float time, Sprite sprite = null)
        {
            messages.Add(message, sprite, time);
            messages.SetVisibility(displayFlags);
        }

        void OnEffectQueue(CCEffectInstance effectInstance) => queuePanel.Add(queue, effectInstance, _displayFlags);
        void OnEffectDequeue(CCEffectInstance effectInstance, EffectResult result) => queuePanel.Remove(effectInstance.effectID);

        void OnEffectStart(CCEffectInstanceTimed effectInstance)
        {
            if (logPanel != null) OnEffectTrigger(effectInstance);
            buffPanel.Add(buff, effectInstance, _displayFlags);
        }

        void OnEffectStop(CCEffectInstanceTimed effectInstance) => buffPanel.Remove(effectInstance.effectID);

        #endregion
    }
}

// TODO
// - fade in/out
// - reset alpha when elements go back into view
// - horizontal alpha checks
