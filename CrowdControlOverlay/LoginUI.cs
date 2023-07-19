using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace WarpWorld.CrowdControl.Overlay {
    public class LoginUI : MonoBehaviour {
        [SerializeField] private GameObject m_view;

        [SerializeField] public Button m_twitch;
        [SerializeField] public Button m_youtube;
        [SerializeField] public Button m_discord;

        public void Init() {
            //CrowdControl.instance.OnDisconnected += Test;
            CrowdControl.instance.OnLoggedOut += delegate { LoginVisible(true); };
            CrowdControl.instance.OnLoggedIn += delegate { LoginVisible(false); };

            CrowdControl.instance.OnSubscribed += delegate { LoginVisible(false); };
            CrowdControl.instance.OnSubscribeFail += delegate { LoginVisible(true); };

            m_twitch.onClick.AddListener(CrowdControl.instance.LoginTwitch);
            m_youtube.onClick.AddListener(CrowdControl.instance.LoginYoutube);
            m_discord.onClick.AddListener(CrowdControl.instance.LoginDiscord);

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
