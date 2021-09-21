using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

#pragma warning disable 1591
namespace WarpWorld.CrowdControl {
    [CustomPropertyDrawer(typeof(Attributes.EnumFlag))]
    public class EnumFlagDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var current = GetValue<Enum>(property);
            var newEnum = EditorGUI.EnumFlagsField(position, label, current);

            property.intValue = (int) Convert.ChangeType(newEnum, typeof(int));

            EditorGUI.EndProperty();
        }

        static T GetValue<T>(SerializedProperty property) {
            var target = property.serializedObject.targetObject as object;
            foreach (var path in property.propertyPath.Split('.')) {
                target = target.GetType().GetField(path, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);
            }
            return (T)target;
        }
    }
}
