using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

#pragma warning disable 1591 
namespace WarpWorld.CrowdControl {
    public class EnumFlag : PropertyAttribute
    {
        public EnumFlag() { }
    }

    [CustomPropertyDrawer(typeof(Attributes.EnumFlag))]
    public class EnumFlagsAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            _property.intValue = EditorGUI.MaskField(_position, _label, _property.intValue, _property.enumNames);
        }
    }
}
