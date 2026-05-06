using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace WarpWorld.CrowdControl.Overlay {
    public class LoginUI : MonoBehaviour {
        [SerializeField] private GameObject m_view;

        [Tooltip("Preferred: one button that opens Crowd Control auth so the user picks Twitch, YouTube, or Discord in the browser.")]
        [SerializeField] private Button m_loginCrowdControl;

        [Tooltip("Optional legacy per-platform buttons (only wired when m_loginCrowdControl is not assigned).")]
        [SerializeField] public Button m_twitch;
        [SerializeField] public Button m_youtube;
        [SerializeField] public Button m_discord;

        public void Init() {
            //CrowdControl.instance.OnDisconnected += Test;
            CrowdControl.instance.OnLoggedOut += delegate { LoginVisible(true); };
            CrowdControl.instance.OnLoggedIn += delegate { LoginVisible(false); };

            CrowdControl.instance.OnSubscribed += delegate { LoginVisible(false); };
            CrowdControl.instance.OnSubscribeFail += delegate { LoginVisible(true); };

            if (m_loginCrowdControl != null)
                m_loginCrowdControl.onClick.AddListener(CrowdControl.instance.LoginWithCrowdControl);
            else {
                if (m_twitch != null)
                    m_twitch.onClick.AddListener(CrowdControl.instance.LoginTwitch);
                if (m_youtube != null)
                    m_youtube.onClick.AddListener(CrowdControl.instance.LoginYoutube);
                if (m_discord != null)
                    m_discord.onClick.AddListener(CrowdControl.instance.LoginDiscord);
            }

            //CrowdControl.instance.OnTempTokenFailure += Test;
            //CrowdControl.instance.OnConnecting += Test;
            //CrowdControl.instance.OnSubmitTempToken += Test;
            //CrowdControl.instance.OnAuthenticated += Test;
        }

        public void LoginVisible(bool state) {
            if (m_view.active == state)
                return;

            m_view.SetActive(state);
        }
    }
}
