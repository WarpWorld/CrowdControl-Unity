using UnityEditor;
using UnityEngine;

#if !UNITY_STANDALONE_WIN

namespace WarpWorld.CrowdControl {
    class JSONWindow : EditorWindow {
        private bool enabled = false;
        private static string jsonManifest = "";

        public static void Init(string manifest) {
            JSONWindow window = GetWindow<JSONWindow>("Game Effect JSON Manifest");

            int width = 300;
            int height = 350;
            int x = (Screen.currentResolution.width - width) / 2;
            int y = (Screen.currentResolution.height - height) / 2;

            window.position = new Rect(x, y, width, height);
            window.enabled = true;

            jsonManifest = manifest;
        }

        void OnGUI() {
            if (!enabled) {
                return;
            }

            titleContent.text = "Game JSON";

            GUILayout.Label("Game Effect JSON Manifest", EditorStyles.boldLabel);

            bool wordWrap = EditorStyles.textField.wordWrap;

            EditorStyles.textField.wordWrap = true;
            EditorGUILayout.TextArea(jsonManifest, GUILayout.Height(300));

            EditorStyles.textField.wordWrap = wordWrap;

            if (GUILayout.Button("Close Window")) {
                Close();
                enabled = false;
                return;
            }
        }
    }
}
#endif