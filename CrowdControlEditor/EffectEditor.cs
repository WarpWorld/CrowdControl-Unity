using UnityEditor;
using UnityEngine;
using System.Reflection;

#pragma warning disable 1591
namespace WarpWorld.CrowdControl {
    [CustomEditor(typeof(CCEffectBase), true)]
    public class EffectEditor : Editor {
        static readonly MethodInfo renderStaticPreview;

        static EffectEditor() {
            var editorAssembly = Assembly.Load("UnityEditor");
            renderStaticPreview = editorAssembly
                .GetType("UnityEditor.SpriteUtility")
                .GetMethod("RenderStaticPreview", new[] { typeof(Sprite), typeof(Color), typeof(int), typeof(int) });
        }

        protected static GUIStyle boldFoldoutStyle;

        protected CCEffectBase effect => target as CCEffectBase;

        bool showInst;
        bool showInfo;

        protected void OnEnable() => showInfo = !IsInformationComplete();

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            // NOTE: can't initialize this in OnEnable because EditorStyles might be created after this editor.
            if (boldFoldoutStyle == null) {
                boldFoldoutStyle = new GUIStyle(EditorStyles.foldout) {
                    font = EditorStyles.boldFont
                };
            }

            showInst = EditorGUILayout.Foldout(showInst, "Instance", boldFoldoutStyle);
            if (showInst) {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxRetries"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("retryDelay"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pendingDelay"));
                OnInstanceGUI();
            }

            showInfo = EditorGUILayout.Foldout(showInfo, "Information", boldFoldoutStyle);
            if (showInfo) {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("iconColor"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("identifier"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
                OnInformationGUI();

                var sprite = effect.icon;
                if (sprite != null) {
                    EditorGUILayout.Space();
                    var sz = sprite.rect;
                    var rc = EditorGUILayout.GetControlRect(false, Mathf.Min(128, sz.height));
                    if (Event.current.type == EventType.Repaint) {
                        rc.width = rc.height * sz.width / sz.height;
                        EditorStyles.textField.Draw(rc, false, false, false, false);

                        var tex = renderStaticPreview.Invoke(null, new object[] {
                            sprite, effect.iconColor, (int)sz.width, (int)sz.height
                        });
                        EditorGUI.DrawTextureTransparent(rc, tex as Texture2D, ScaleMode.StretchToFill);
                    }
                }
            }

            if (showInst || showInfo) serializedObject.ApplyModifiedProperties();

            if (!IsInformationComplete())
                EditorGUILayout.HelpBox("Effect is incomplete.", UnityEditor.MessageType.Warning);

            if (Application.isPlaying) {
                EditorGUILayout.Space();
                GUI.enabled = effect.isActiveAndEnabled;
                OnInvokeGUI();
            }
        }

        /// <summary>Returns <see langword="true"/> when the incomplete warning box should be displayed.</summary>
        protected virtual bool IsInformationComplete() => effect.icon != null &&
                effect.identifier > 0 &&
                !string.IsNullOrEmpty(effect.displayName) &&
                !string.IsNullOrEmpty(effect.description);

        protected virtual void OnInstanceGUI() {}
        protected virtual void OnInformationGUI() {}

        /// <summary>Override to customize the effect test buttons displayed in play mode.</summary>
        protected virtual void OnInvokeGUI() {
            if (GUILayout.Button("Trigger Locally")) TestEffectLocally();
            //if (GUILayout.Button("Trigger Remotely")) TestEffectRemotely();
        }

        /// <summary>Triggers a test instance of the effect. Only works in play mode.</summary>
        protected void TestEffectLocally() => CrowdControl.instance?.TestEffect(effect);
        //protected void TestEffectRemotely() => CrowdControl.instance?.SendCCEffectThroughServer(effect, "Test");
    }
}
