#if !UNITY_STANDALONE_WIN

using UnityEditor;
using UnityEngine;
using System;
using WarpWorld.CrowdControl;
using System.Collections.Generic;
using UnityEditor.Compilation;

[CustomPropertyDrawer(typeof(CCEffectEntry))]
public class CCEffectEntryDrawer : PropertyDrawer {

    UnityEditor.Compilation.Assembly[] playerAssemblies = null;
    private bool assemblyLoaded = false;

    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return 20f;
	}

	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        if (!assemblyLoaded) {
            AssemblyReloadEvents.afterAssemblyReload += ReloadAssemblies;
            assemblyLoaded = true;
        }

        int oldIndentLevel = EditorGUI.indentLevel;
        float oldLabelWidth = EditorGUIUtility.labelWidth;

        EditorGUIUtility.labelWidth = 1;
        
        label = EditorGUI.BeginProperty(position, label, property);
		Rect contentPosition = EditorGUI.PrefixLabel(position, label);
        List<string> options = AllEffects();

        float oldWidth = EditorStyles.popup.fixedWidth;

        EditorStyles.popup.fixedWidth = contentPosition.width;
        GUIContent[] comboItemDatabaseGUIContents = Array.ConvertAll(options.ToArray(), i => new GUIContent(i));

        SerializedProperty sp = property.FindPropertyRelative("ClassName");
        int selectedIndex = EditorGUI.Popup(contentPosition, label, CurrentSelectedIndex(sp, options), comboItemDatabaseGUIContents);

        if (selectedIndex >= options.Count) {  
            sp.stringValue = "";
        }
        else if (selectedIndex >= options.Count) {
            sp.stringValue = options[0];
        } else  {
            sp.stringValue = options[selectedIndex];
        } 

        EditorGUI.EndProperty();
		EditorGUI.indentLevel = oldIndentLevel; 
        EditorGUIUtility.labelWidth = oldLabelWidth;
        EditorStyles.popup.fixedWidth = oldWidth;
    }

    private int CurrentSelectedIndex(SerializedProperty sp, List<string> options) {
        for (int i = 0; i < options.Count; i++) {
            if (sp.stringValue == options[i])
                return i;
        }

        return 0;
    }

    private void ReloadAssemblies() {
        playerAssemblies = CompilationPipeline.GetAssemblies();
    }

    private List<string> AllEffects() {
        Type type = typeof(CCEffectBase);
        List<string> effectNames = new List<string>();

        if (playerAssemblies == null) {
            ReloadAssemblies();
        }

        foreach (var assembly in playerAssemblies) {
            if (assembly.name.Contains("UnityEngine.") || assembly.name.Contains("UnityEditor.") || assembly.name.Contains("Unity.") || assembly.name.Contains("System."))  {
                continue;
            }

            System.Reflection.Assembly asm = System.Reflection.Assembly.Load(assembly.name.ToString());

            Type[] baseEffects = asm.GetTypes();

            foreach (Type baseEffect in baseEffects) {
                if (type.IsAssignableFrom(baseEffect))
                    effectNames.Add(baseEffect.ToString());
            }
        }

        return effectNames;
    }
}
#endif
