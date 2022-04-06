using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace WarpWorld.CrowdControl {
    [CustomEditor(typeof(CCEffectBase), true)]
    class EffectEditor : CCEditor {
        CCEffectBase effect => target as CCEffectBase;

        public override void OnInspectorGUI() {
            InitCoords();

            AddProperty(ValueType._string, "displayName", "Name", 50.0f, 250.0f);
            AddProperty(ValueType._int, "price", "Cost", 50.0f, 50.0f);
            NewRow();
            AddLabel("Description", 300.0f);
            AddSpriteWithTint("icon", "iconColor", "Icon", 125.0f, 100.0f);
            AddDividerBar();
            SetNextOffset(22.5f);
            NewRow();
            AddProperty(ValueType._string, "description", "", 0.0f, 290.0f, 100.0f);
            NewRow();
            AddPropertyWithSlider(ValueType._int, "maxRetries", "Max Entries", 220.0f, 210.0f, 0, 60);
            NewRow();
            AddPropertyWithSlider(ValueType._float, "retryDelay", "Retry Delay", 220.0f, 210.0f, 0, 10);
            AddPropertyWithSlider(ValueType._float, "pendingDelay", "Pending Delay", 220.0f, 210.0f, 0, 10);
            GUILayout.Space(230);

            if (effect is CCEffectTimed)
            {
                DrawTimedEffect();
            }
            else if (effect is CCEffectParameters)
            {
                DrawParamEffect();
            }

            EditorGUI.BeginChangeCheck();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawParamEffect()
        {
            SetNextOffset(50.0f, true);
            AddDividerBar();
            SetNextOffset(10.0f, true);

            SerializedProperty ThisList = serializedObject.FindProperty("m_parameterEntries");
            
            for (int i = 0; i < ThisList.arraySize; i++)
            {
                SerializedProperty property = ThisList.GetArrayElementAtIndex(i);
                AddProperty(ValueType._string, property.FindPropertyRelative("m_name"), "Name", 50.0f, 200.0f);          
                NewRow();
                AddSpriteWithTint(property.FindPropertyRelative("m_sprite"), property.FindPropertyRelative("m_tint"), "Icon", 90.0f, 100.0f);
                AddEnumField(property.FindPropertyRelative("m_paramKind"), "Param Type", 100.0f);
                SetNextOffset(45.0f);

                if (property.FindPropertyRelative("m_paramKind").intValue == 1) // Quantity
                {
                    NewRow();
                    IncreasePosition(110.0f);
                    AddProperty(ValueType._int, property.FindPropertyRelative("m_min"), "Min", 50.0f, 50.0f);
                    NewRow();
                    IncreasePosition(110.0f);
                    AddProperty(ValueType._int, property.FindPropertyRelative("m_max"), "Min", 50.0f, 50.0f);
                }

                else
                {
                    SerializedProperty options = property.FindPropertyRelative("m_options");

                    AddArraySizeProperty(options, 50.0f);
                    NewRow();

                    for (int j = 0; j < options.arraySize; j++)
                    {
                        IncreasePosition(110.0f);
                        AddProperty(ValueType._string, options.GetArrayElementAtIndex(j), string.Empty, 0.0f, 160.0f);
                        NewRow();

                        if (j > 3)
                        {
                            GUILayout.Space(25);
                        }
                    }
                }


                GUILayout.Space(145);
            }

            GUILayout.Space(40);
        }

        private void DrawTimedEffect()
        {
            NewRow();
            AddDividerBar();

            CCEffectTimed effectTimed = effect as CCEffectTimed;

            AddPropertyWithSlider(ValueType._float, "duration", "Duration", 220.0f, 210.0f, 0, 600);
            AddEnumField("displayType", "Dispaly Type", 200.0f);

            GUILayout.Space(40);

            if (Application.isPlaying)
            {
                NewRow();
                AddDividerBar();

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

                GUILayout.Space(40);
            }
        }

        /// <summary>Returns <see langword="true"/> when the incomplete warning box should be displayed.</summary>
        protected virtual bool IsInformationComplete() => effect.icon != null &&
                effect.identifier > 0 &&
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
