#if !UNITY_STANDALONE_WIN

using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace WarpWorld.CrowdControl { 
    [CustomEditor(typeof(CrowdControl))]
    class CrowdControlEditor : CCEditor {
        CrowdControl cc => target as CrowdControl;

        [HideInInspector] public bool queueMenuRefresh = false;

        public override void OnInspectorGUI() { 
            Rect rt = GUILayoutUtility.GetRect(new GUIContent("some button"), GUIStyle.none);
            InitCoords(rt.y);

            if (!Application.isPlaying)
            {
                AddProperty(ValueType._int, "_gameKey", "Game Key", 77.5f, 35.0f);
                AddProperty(ValueType._string, "_gameName", "Game Name", 87.5f, 200.0f);
                NewRow();
            }

            AddPropertyWithSlider(ValueType._int, "_reconnectRetryCount", "Reconnect Retry Count", 200.0f, 35.0f, -1, 100);
            AddPropertyWithSlider(ValueType._float, "_reconnectRetryDelay", "Reconnect Retry Delay", 200.0f, 35.0f, 0.0f, 60.0f);

            NewRow();
            AddPropertyWithSlider(ValueType._float, "delayBetweenEffects", "Delay Between Effects", 200.0f, 35.0f, 0, 10);
            AddEnumField("_broadcasterType", "Broadcaster Type", 200.0f);

            NewRow();
            AddLabel("Icons", 100.0f, 1.2f, FontStyle.Bold);
            NewRow();

            AddSpriteWithTint("_tempUserIcon", "_tempUserColor", "Temp User Icon", 125.0f, 100.0f);
            AddSpriteWithTint("_crowdUserIcon", "_crowdUserColor", "Crowd User Icon", 125.0f, 100.0f);
            AddSpriteWithTint("_errorUserIcon", "_errorUserColor", "Error User Icon", 125.0f, 100.0f); 

            NewRow();
            AddProperty(ValueType._bool, "_staging", "Staging", 100.0f, 50.0f);
            AddProperty(ValueType._bool, "_dontDestroyOnLoad", "Don't Destroy on Load", 300.0f, 50.0f);

            SetNextOffset(45.0f, true);
            AddLabel("Debug Output", 150.0f, 1.2f, FontStyle.Bold, true);

            NewRow();

            AddProperty(ValueType._bool, "_debugLog", "Log", 45.0f, 50.0f);
            AddProperty(ValueType._bool, "_debugWarning", "Warning", 110.0f, 50.0f);
            AddProperty(ValueType._bool, "_debugError", "Error", 50.0f, 50.0f);
            AddProperty(ValueType._bool, "_debugExceptions", "Exception", 100.0f, 50.0f);

            GUILayout.Space(375);

            if (Application.isPlaying) {
                GUILayout.Space(45);
                SetNextOffset(65.0f, true);

                GUI.enabled = cc.HasRunningEffects();
                
                if (AddButton("Stop all Effects", 150.0f))
                    cc.StopAllEffects();

                if (cc.isConnected) {
                    if (AddButton("Refresh Online Menu", 150.0f)) {
                        queueMenuRefresh = true;
                    }
                }
            }

            if (!cc.isConnected) {
                if (!Application.isPlaying) {
                    GUILayout.Space(45);
                    SetNextOffset(65.0f, true);
                }

                if (AddButton("Clear Saved Tokens", 150.0f)) {
                    uint gameKey = (uint)serializedObject.FindProperty("_gameKey").intValue;
                    PlayerPrefs.SetString($"CCToken{gameKey}False", string.Empty);
                    PlayerPrefs.SetString($"CCToken{gameKey}True", string.Empty);
                    Debug.Log("[CC] Cleared Saved Tokens");
                }
            }

            EditorGUILayout.Space();
            GUI.enabled = !Application.isPlaying;

            if (Application.isPlaying) {
                EditorGUILayout.Space();

                GUI.enabled = cc.HasRunningEffects();
                if (GUILayout.Button("Stop All Effects"))
                    cc.StopAllEffects();
            } else
            {
                if (GUILayout.Button("Effect JSON")) GenerateServerData();
            }

            EditorGUI.BeginChangeCheck();
            serializedObject.ApplyModifiedProperties();

            if (queueMenuRefresh) {
                cc.ReRegisterEffects();
                queueMenuRefresh = false;
            }
        }

        void GenerateServerData() {
            string gameName = serializedObject.FindProperty("_gameName").stringValue;

            CCEffectEntries effectEntries = cc.gameObject.GetComponent<CCEffectEntries>();
            Dictionary<uint, CCEffectBase> effectsByID = new Dictionary<uint, CCEffectBase>();

            effectEntries.PrivateResetDictionary();
            effectEntries.PrivatePopulateDictionary();

            CCEffectBase [] effectBases = FindObjectsOfType<CCEffectBase>();

            foreach (CCEffectBase effectBase in effectBases) {
                effectEntries.PrivateAddEffect(effectBase);

                if (!effectsByID.ContainsKey(effectBase.identifier)) {
                    effectBase.SetIdentifier();
                    effectsByID.Add(effectBase.identifier, effectBase);
                    effectBase.RegisterParameters(effectEntries);
                }
            }

            CCJsonBlock jsonBlock = new CCJsonBlock(gameName, effectsByID, effectEntries);
            effectEntries.PrivateResetDictionary();

            foreach (CCEffectBase effectBase in effectBases) {
                if (effectBase is CCEffectBidWar) {
                    CCEffectBidWar bidWar = (CCEffectBidWar)effectBase;
                    bidWar.BidWarEntries.Clear();
                }

                if (effectBase is CCEffectParameters) {
                    CCEffectParameters paramEffect = (CCEffectParameters)effectBase;
                    paramEffect.ParameterEntries.Clear();
                }
            }

            if (jsonBlock.jsonStrings.Count < 2) {
                return;
            }

            JSONWindow jSONWindow = (JSONWindow)EditorWindow.GetWindow(typeof(JSONWindow), true, "JSON");
            JSONWindow.Init(jsonBlock.jsonStrings[0] + ",\n" + jsonBlock.jsonStrings[1]);
        }
    }
}
#endif
