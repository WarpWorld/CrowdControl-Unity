#if !UNITY_STANDALONE_WIN

using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace WarpWorld.CrowdControl {
    [CustomEditor(typeof(CCEffectBase), true)]
    class EffectEditor : CCEditor {
        CCEffectBase effect => target as CCEffectBase;

        public override void OnInspectorGUI() {
            Rect rt = GUILayoutUtility.GetRect(new GUIContent(""), GUIStyle.none);
            InitCoords(rt.y);

            AddProperty(ValueType._string, "displayName", "Name", 50.0f, 250.0f);

            if (!(effect is CCEffectBidWar))
                AddProperty(ValueType._int, "price", "Cost", 50.0f, 50.0f);

            NewRow();
            AddLabel("Description", 300.0f);
            AddSpriteWithTint("icon", "iconColor", "Icon", 125.0f, 100.0f);
            SetNextOffset(22.5f, true);
            AddProperty(ValueType._string, "description", "", 0.0f, 290.0f, 100.0f);
            SetNextOffset(112.5f, true);

            AddEnumField("morality", "Morality Type", 200.0f);


            // AddProperty(ValueType._string, "folderPath", "Folder Path", 80.0f, 130.0f);
            //SetNextOffset(-10.0f, true);
            AddPropertyWithSlider(ValueType._int, "maxRetries", "Max Retries", 220.0f, 210.0f, 0, 60);
            
            NewRow();
            AddPropertyWithSlider(ValueType._float, "retryDelay", "Retry Delay", 220.0f, 210.0f, 0, 10);
            AddPropertyWithSlider(ValueType._float, "pendingDelay", "Pending Delay", 220.0f, 210.0f, 0, 10);

            

            SetNextOffset(50.5f, true);

            SerializedProperty CategoryList = serializedObject.FindProperty("Categories");
            AddProperty(ValueType._bool, "inactive", "Inactive", 300.0f, 75.0f);
            AddProperty(ValueType._bool, "disabled", "Disabled", 300.0f, 75.0f);
            AddProperty(ValueType._bool, "noPooling", "Non-Poolable", 300.0f, 130.0f);
            AddArraySizeProperty(CategoryList, 75.0f, "Categories", 100.0f, 99, 17.5f);
            //IncreasePosition(100.0f);

            GUILayout.Space(45.0f);
            SetNextOffset(45.0f, true);

            if (CategoryList.arraySize == 0) {
                GUILayout.Space(25.0f);
            }


            for (int i = 0; i < CategoryList.arraySize; i++) {
                SerializedProperty property = CategoryList.GetArrayElementAtIndex(i);
                AddProperty(ValueType._string, property, string.Empty, 0.0f, 150.0f);

                if (i % 3 == 0)
                    GUILayout.Space(25.0f);

                if (i >= 2) {
                    
                    if (i % 3 == 2)
                        SetNextOffset(25.0f, true);
                }
            }

            GUILayout.Space(245);

            if (effect is CCEffectTimed) {
                DrawTimedEffect();
            }
            else if (effect is CCEffectParameters)  {
                DrawParamEffect();
            }
            else if (effect is CCEffectBidWar) {
                DrawBidWar();
            }

            EditorGUI.BeginChangeCheck();
            serializedObject.ApplyModifiedProperties();

            DrawDefaultInspector();
        }

        private List<bool> m_paramFoldout = new List<bool>();

        private void DrawBidWar() {
            SetNextOffset(40.0f, true);
            GUILayout.Space(65.0f);
            SerializedProperty ThisList = serializedObject.FindProperty("m_bidWarEntries");
            AddArraySizeProperty(ThisList, 50.0f, "Bid Wars", 100.0f, 99, 17.5f);

            float increaseX = 0.0f;

            for (int i = 0; i < ThisList.arraySize; i++) {
                IncreasePosition(increaseX);

                SerializedProperty property = ThisList.GetArrayElementAtIndex(i);

                NewRow();
                IncreasePosition(increaseX);
                AddProperty(ValueType._string, property.FindPropertyRelative("m_name"), string.Empty, 0.0f, 100.0f);
                NewRow();
                IncreasePosition(increaseX);
                AddSpriteWithTint(property.FindPropertyRelative("m_sprite"), property.FindPropertyRelative("m_tint"), "Icon", 90.0f, 100.0f);

                if (i % 3 == 2) {
                    SetNextOffset(100.0f, true);
                    increaseX = 0.0f;
                }
                else {
                    increaseX += 125.0f;
                    SetNextOffset(-50.0f, true);

                    if (i % 3 == 0) {
                        GUILayout.Space(150.0f);
                    }
                }
            }
        }

        private void DrawParamEffect() {
            SetNextOffset(40.0f, true);

            SerializedProperty ThisList = serializedObject.FindProperty("m_parameterEntries");

            AddLabel("Parameter types", 150.0f, 1.2f, FontStyle.Bold);
            AddArraySizeProperty(ThisList, 50.0f, 5, 17.5f);
            NewRowWithSpace();

            if (m_paramFoldout.Count > ThisList.arraySize) {
                m_paramFoldout.RemoveRange(ThisList.arraySize, m_paramFoldout.Count - ThisList.arraySize);
            }

            while (m_paramFoldout.Count < ThisList.arraySize) {
                m_paramFoldout.Add(false);
            }

            for (int i = 0; i < ThisList.arraySize; i++) {
                SerializedProperty property = ThisList.GetArrayElementAtIndex(i);
                m_paramFoldout[i] = AddFoldout(property, m_paramFoldout[i], "m_name");
                NewRowWithSpace();

                if (!m_paramFoldout[i])
                {
                    continue;
                }

                AddProperty(ValueType._string, property.FindPropertyRelative("m_name"), "Name", 50.0f, 225.0f);
                NewRow();
                AddSpriteWithTint(property.FindPropertyRelative("m_sprite"), property.FindPropertyRelative("m_tint"), "Icon", 90.0f, 100.0f);

                if (property.FindPropertyRelative("m_paramKind").intValue == 1) // Quantity
                {
                    GUILayout.Space(150);
                    AddEnumField(property.FindPropertyRelative("m_paramKind"), "Param Type", 160.0f);
                    SetNextOffset(45.0f, true);
                    IncreasePosition(110.0f);
                    AddProperty(ValueType._int, property.FindPropertyRelative("m_min"), "Min", 40.0f, 50.0f);
                    NewRow();
                    IncreasePosition(110.0f);
                    AddProperty(ValueType._int, property.FindPropertyRelative("m_max"), "Max", 40.0f, 50.0f);
                    SetNextOffset(30.0f, true);
                    NewRow();
                }
                else
                {
                    GUILayout.Space(150);
                    AddEnumField(property.FindPropertyRelative("m_paramKind"), "Param Type", 100.0f);
                    SetNextOffset(45.0f);

                    SerializedProperty options = property.FindPropertyRelative("m_options");

                    AddArraySizeProperty(options, 50.0f, 99, 35.5f);
                    NewRow();

                    for (int j = 0; j < options.arraySize; j++)
                    {
                        IncreasePosition(110.0f);
                        AddProperty(ValueType._string, options.GetArrayElementAtIndex(j), string.Empty, 0.0f, 160.0f);
                        NewRow();

                        if (j > 2)
                        {
                            GUILayout.Space(25);
                        }
                    }

                    if (options.arraySize <= 3)
                    {
                        NewRow(Convert.ToUInt32(3 - options.arraySize));
                    }

                    SetNextOffset(5.0f, true);
                }
            }

            GUILayout.Space(30);
        }

        private void DrawTimedEffect()
        {
            NewRow();

            CCEffectTimed effectTimed = effect as CCEffectTimed;

            AddPropertyWithSlider(ValueType._float, "duration", "Duration", 220.0f, 210.0f, 0, 600);
            AddEnumField("displayType", "Dispaly Type", 200.0f);

            GUILayout.Space(40);

            if (Application.isPlaying)
            {
                NewRow();

                bool running = CrowdControl.instance.IsRunning(effectTimed);
                bool paused = CrowdControl.instance.IsPaused(effectTimed);

                if (AddButton("Start Locally", 100.0f)) { TestEffectLocally(); }

                if (running || paused) {
                    if (AddButton("Stop", 100.0f)) { CrowdControl.instance.StopOne(effectTimed); }
                }

                if (!paused && running) {
                    if (AddButton("Pause", 100.0f)) { CrowdControl.DisableEffect(effectTimed); }
                }

                if (paused) {
                    if (AddButton("Resume", 100.0f)) { CrowdControl.EnableEffect(effectTimed); }
                }

                if (running) {
                    if (AddButton("Reset", 100.0f)) { CrowdControl.ResetEffect(effectTimed); }
                }

                GUILayout.Space(65);
            }
        }

        /// <summary>Returns <see langword="true"/> when the incomplete warning box should be displayed.</summary>
        protected virtual bool IsInformationComplete() => effect.icon != null &&
                !string.IsNullOrEmpty(effect.effectKey) &&
                !string.IsNullOrEmpty(effect.displayName);

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
#endif
