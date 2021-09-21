using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace WarpWorld.CrowdControl.Overlay
{
    public class TokenUI : MonoBehaviour
    {
        public Button button;
        public Text text;

        public Action<string> onSubmit;

        void Start()
        {
            button.onClick.AddListener(Submit);
        }

        public void Submit()
        {
            onSubmit?.Invoke(text.text);
        }
    }
}
