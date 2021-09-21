using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using WarpWorld.CrowdControl;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(CCEffectEntry))]
public class CCEffectEntryDrawer : PropertyDrawer {
    
	public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
		return 20f;
	}

    
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        
        int oldIndentLevel = EditorGUI.indentLevel;
        float oldLabelWidth = EditorGUIUtility.labelWidth;

        EditorGUIUtility.labelWidth = 1;
        
        label = EditorGUI.BeginProperty(position, label, property);
		Rect contentPosition = EditorGUI.PrefixLabel(position, label);

        contentPosition.width *= 0.2f;
		EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("id"), GUIContent.none);
		contentPosition.x += contentPosition.width;
		contentPosition.width *= 3f;
        List<string> options = AllEffects();

        float oldWidth = EditorStyles.popup.fixedWidth;

        EditorStyles.popup.fixedWidth = contentPosition.width;
        GUIContent[] comboItemDatabaseGUIContents = Array.ConvertAll(options.ToArray(), i => new GUIContent(i));

        SerializedProperty sp = property.FindPropertyRelative("className");
        int selectedIndex = EditorGUI.Popup(contentPosition, label, CurrentSelectedIndex(sp, options), comboItemDatabaseGUIContents);
        sp.stringValue = options[selectedIndex];

        EditorGUI.EndProperty();
		EditorGUI.indentLevel = oldIndentLevel;
        EditorGUIUtility.labelWidth = oldLabelWidth;
        EditorStyles.popup.fixedWidth = oldWidth;
    }

    private int CurrentSelectedIndex(SerializedProperty sp, List<string> options)
    {
        for (int i = 0; i < options.Count; i++)
        {
            if (sp.stringValue == options[i])
                return i;
        }

        return 0;
    }

    private List<string> AllEffects()
    {
        Type type = typeof(CCEffectBase);
        List<string> effectNames = new List<string>();
        Assembly asm = Assembly.Load("Assembly-CSharp");

        Type[] baseEffects = asm.GetTypes();

        foreach (Type baseEffect in baseEffects)
        {
            if (type.IsAssignableFrom(baseEffect))
                effectNames.Add(baseEffect.ToString());
        }

        return effectNames;
    }
}