using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace WarpWorld.CrowdControl.Overlay {
    [Serializable]
    public class TokenUIView {
        public GameObject _view;
        public Button _button;

        public void AddListener(UnityAction action) {
            _button.onClick.AddListener(action);
        }
    }

    public class TokenUI : MonoBehaviour {
        public enum View {   
            None,
            Disconnected,
            InputToken,
            Connected
        }

        private Dictionary<View, TokenUIView> _viewDictionary;
        private View _view = View.Disconnected;

        [SerializeField] private TokenUIView _disconnectedView;
        [SerializeField] private TokenUIView _needTokenView;
        [SerializeField] private TokenUIView _connectedView;
        [SerializeField] private TMP_Text _tokenInputField;
        [SerializeField] private TMP_Text _tokenInstructions;

        public Action<string> onSubmit;

        public void Init() {
            _viewDictionary = new Dictionary<View, TokenUIView> {
                { View.Disconnected, _disconnectedView },
                { View.InputToken, _needTokenView },
                { View.Connected, _connectedView }
            };

            _disconnectedView.AddListener(CrowdControl.instance.Connect);
            _needTokenView.AddListener(SubmitToken);
            _connectedView.AddListener(CrowdControl.instance.Disconnect);

            CrowdControl.instance.OnDisconnected += delegate { ChangeView(View.Disconnected); };
            //CrowdControl.instance.OnNoToken += delegate { ChangeView(View.InputToken); };
            CrowdControl.instance.OnLoginFailure += delegate { ChangeView(View.InputToken); };
            CrowdControl.instance.OnConnecting += delegate { ChangeView(View.None); };
            CrowdControl.instance.OnSubscribed += delegate { ChangeView(View.Connected); };

            SetStartView();
        }

        private void SetStartView() {
            if (CrowdControl.instance.isConnected) {
                ChangeView(View.Connected);
                return;
            }

            ChangeView(View.Disconnected);
        }

        private void ChangeView(View view) {
            _view = view;

            foreach (View v in _viewDictionary.Keys) {
                _viewDictionary[v]._view.gameObject.SetActive(_view == v);
            }
        }

        private void SubmitToken() {
            //CrowdControl.instance.SubmitTempToken(_tokenInputField.text);
        }
    }
}
