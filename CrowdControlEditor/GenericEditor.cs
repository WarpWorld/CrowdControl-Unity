#if !UNITY_STANDALONE_WIN

using UnityEditor;
using UnityEngine;

namespace WarpWorld.CrowdControl {
    [CustomEditor(typeof(CCGeneric), true)] 
    public class GenericEditor : CCEditor {
        CCGeneric generic => target as CCGeneric;

        public override void OnInspectorGUI() {
            Rect rt = GUILayoutUtility.GetRect(new GUIContent(""), GUIStyle.none);

            InitCoords(rt.y);
             
            AddProperty(ValueType._string, "genericName", "Name", 50.0f, 250.0f);

            SerializedProperty keyList = serializedObject.FindProperty("keys");
            SerializedProperty valuesList = serializedObject.FindProperty("values");
            AddArraySizeProperty(keyList, 65.0f, "Parameters", 100.0f, 99, 17.5f);

            NewRow();

            for (int i = 0; i < keyList.arraySize; i++) {
                AddProperty(ValueType._string, keyList.GetArrayElementAtIndex(i), string.Empty, 0.0f, 160.0f);

                if (Application.isPlaying) {
                    AddProperty(ValueType._string, valuesList.GetArrayElementAtIndex(i), string.Empty, 0.0f, 160.0f);
                }

                NewRow();
                GUILayout.Space(25);
            }

            if (Application.isPlaying) {
                if (AddButton("Send Test", 100.0f)) {
                    TestGeneric();
                }

                if (AddButton("Receive Test", 100.0f)) {
                    GetGeneric();
                }

                NewRow();
                GUILayout.Space(25);
            }

            GUILayout.Space(25);

            EditorGUI.BeginChangeCheck();
            serializedObject.ApplyModifiedProperties();
        }

        protected void TestGeneric() => CrowdControl.instance?.SendGenericTest(generic);
        protected void GetGeneric() => CrowdControl.instance?.GetGenericTest(generic);
    }
}
#endif
