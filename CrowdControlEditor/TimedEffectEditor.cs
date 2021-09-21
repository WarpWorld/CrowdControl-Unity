using UnityEditor;
using UnityEngine;

#pragma warning disable 1591
namespace WarpWorld.CrowdControl {
    [CustomEditor(typeof(CCEffectTimed), true)]
    public class TimedEffectEditor : EffectEditor {
        protected override void OnInstanceGUI() {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("duration"));
        }

        // TODO update
        protected override void OnInvokeGUI() {
            EditorGUILayout.BeginHorizontal();

            CCEffectTimed effectTimed = effect as CCEffectTimed;

            bool running = CrowdControl.instance.IsRunning(effectTimed);
            bool paused = CrowdControl.instance.IsPaused(effectTimed);

            GUI.enabled = true;

            if (GUILayout.Button("Start Locally")) TestEffectLocally();
            //if (GUILayout.Button("Start Remotely")) TestEffectRemotely();

            GUI.enabled = running || paused;

            if (GUILayout.Button("Stop")) CrowdControl.instance.StopOne(effectTimed);

            GUI.enabled = !paused && running;

            if (GUILayout.Button("Pause")) CrowdControl.DisableEffect(effectTimed);

            GUI.enabled = paused;

            if (GUILayout.Button("Resume")) CrowdControl.EnableEffect(effectTimed);

            GUI.enabled = running;

            if (GUILayout.Button("Reset")) CrowdControl.ResetEffect(effectTimed);

            EditorGUILayout.EndHorizontal();
        }
    }
}
