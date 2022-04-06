using UnityEditor;
using UnityEngine;
using System;

namespace WarpWorld.CrowdControl
{
    public class CCEditor : Editor
    {
        protected Vector2 m_coords = Vector2.zero;
        private float m_nextRowOffset = 25.0f;

        public enum ValueType
        {
            _int,
            _string,
            _float, 
            _bool,
            _sprite,
            _enum
        }

        protected Rect GetRect(float x, float y, float width, float height)
        {
            return new Rect(x + 20.0f, y - 10.0f, width, height);
        }

        protected void AddLabel(string content, float x, float y, float width, float fontScale = 1.0f, FontStyle fontStyle = FontStyle.Normal)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontStyle = fontStyle;
            style.fontSize = Convert.ToInt32(style.fontSize * fontScale);
            GUI.Label(GetRect(x, y, width - x - 12.5f + x, 50.0f), new GUIContent(content), style);
        }

        protected void AddProperty(ValueType valueType, string key, string name, float x, float y, float propertyX, float propertyWidth, float height = 20.0f)
        {
            AddProperty(serializedObject.FindProperty(key), valueType, name, x, y, propertyX, propertyWidth, height);
        }

        protected void AddArraySizeProperty(SerializedProperty property, float propertyWidth, int max = 99, float height = 20.0f)
        {
            Rect rect = GetRect(m_coords.x, m_coords.y + 35.5f, propertyWidth, height);
            property.arraySize = EditorGUI.IntField(rect, property.arraySize);

            if (property.arraySize > max)
            {
                property.arraySize = max;
            }
        }

        protected void AddProperty(SerializedProperty property, ValueType valueType, string name, float x, float y, float propertyX, float propertyWidth, float height = 20.0f)
        {
            Rect rect = GetRect(x + propertyX, y + 17f, propertyWidth, height);

            if (!string.IsNullOrEmpty(name)) {
                AddLabel(name, x, y, propertyX);
            } else {
                propertyX = 0;
            }

            SetNextRowOffset(height);

            switch (valueType)
            {
                case ValueType._int:
                    property.intValue = EditorGUI.IntField(rect, property.intValue);
                    break;
                case ValueType._string:
                    GUIStyle style = new GUIStyle(GUI.skin.textArea);
                    property.stringValue = EditorGUI.TextField(rect, property.stringValue, GUI.skin.textArea);
                    break;
                case ValueType._float:
                    property.floatValue = EditorGUI.FloatField(rect, property.floatValue);
                    break;
                case ValueType._sprite:
                    y += 20.0f;
                    SetNextRowOffset(propertyWidth + 15.0f + height);
                    rect = GetRect(x, y + 15.5f, propertyWidth, propertyWidth);
                    property.objectReferenceValue = EditorGUI.ObjectField(rect, property.objectReferenceValue, typeof(Sprite), false);
                    break;
                case ValueType._bool:
                    y += 5.0f;
                    SetNextRowOffset(propertyWidth + 15.0f + height);
                    rect = GetRect(x, y + 15.5f, propertyWidth, propertyWidth);
                    property.boolValue = EditorGUI.Toggle(rect, property.boolValue);
                    break;
            }
        }

        protected void AddEnumField(SerializedProperty property, string name, float labelWidth)
        {
            AddLabel(name, m_coords.x, m_coords.y, labelWidth);
            Rect rect = GetRect(m_coords.x, m_coords.y + 35.0f, labelWidth, 20.0f);
            property.intValue = EditorGUI.Popup(rect, property.intValue, property.enumNames);

            SetNextRowOffset(40.0f);

            IncreasePosition(labelWidth + 5.0f);
        }

        protected void AddEnumField(string key, string name, float labelWidth)
        {
            AddEnumField(serializedObject.FindProperty(key), name, labelWidth);
        }

        private void SetNextRowOffset(float newValue)
        {
            m_nextRowOffset = Mathf.Max(m_nextRowOffset, newValue);
        }

        protected int GetInt(ValueType valueType, string key)
        {
            return serializedObject.FindProperty(key).intValue;
        }

        protected void AddProperty(ValueType valueType, SerializedProperty serializedProperty, string name, float labelWidth, float entryWidth, float height = 20.0f)
        {
            AddProperty(serializedProperty, valueType, name, m_coords.x, m_coords.y, labelWidth, entryWidth, height);

            if (valueType == ValueType._sprite || valueType == ValueType._bool)
            {
                labelWidth = 0.0f;
            }

            IncreasePosition(labelWidth + entryWidth + 5.0f);
        }

        protected void AddProperty(ValueType valueType, string key, string name, float labelWidth, float entryWidth, float height = 20.0f)
        {
            AddProperty(valueType, key, name, m_coords.x, m_coords.y, labelWidth, entryWidth, height);

            if (valueType == ValueType._sprite || valueType == ValueType._bool)
            {
                labelWidth = 0.0f;
            }

            IncreasePosition(labelWidth + entryWidth + 5.0f);
        }

        protected void AddSlider(ValueType valueType, string key, float width, float min, float max)
        {
            SerializedProperty property = serializedObject.FindProperty(key);

            Rect rect = GetRect(m_coords.x, m_coords.y + 35.0f, width, 20.0f);

            SetNextRowOffset(40.0f);

            switch (valueType)
            {
                case ValueType._int:
                    property.intValue = EditorGUI.IntSlider(rect, property.intValue, Convert.ToInt32(min), Convert.ToInt32(max));
                    break;
                case ValueType._float:
                    property.floatValue = EditorGUI.Slider(rect, property.floatValue, min, max);
                    break;
            }
        }

        protected void AddPropertyWithSlider(ValueType valueType, string key, string name, float labelWidth, float entryWidth, float min, float max)
        {
            AddLabel(name, m_coords.x, m_coords.y, labelWidth);
            AddSlider(valueType, key, labelWidth, min, max);
            IncreasePosition(labelWidth);
        }

        protected void AddProperty(ValueType valueType, string key, float entryWidth)
        {
            AddProperty(valueType, key, string.Empty, m_coords.x, m_coords.y, 0.0f, entryWidth);
        }

        protected void AddLabel(string content, float labelWidth, float fontScale = 1.0f, FontStyle fontStyle = FontStyle.Normal)
        {
            AddLabel(content, m_coords.x, m_coords.y, labelWidth, fontScale, fontStyle);
            IncreasePosition(labelWidth + 5.0f);
        }

        protected bool AddButton(string content, float width)
        {
            Rect rect = GetRect(m_coords.x, m_coords.y + 20.0f, width, 20.0f);
            IncreasePosition(width + 5.0f);
            return GUI.Button(rect, content);
        }

        protected void IncreasePosition(float increase)
        {
            m_coords = new Vector2(m_coords.x + increase + 5.0f, m_coords.y);
        }

        protected void InitCoords()
        {
            m_coords = Vector2.zero;
            SetNextOffset();
        }

        protected void NewRow(uint count = 1)
        {
            m_coords = new Vector2(0.0f, m_coords.y + (m_nextRowOffset * count));
            SetNextOffset();
        }

        protected void SetNextOffset(float amount = 25.0f, bool autoUpdate = false)
        {
            m_nextRowOffset = amount;

            if (autoUpdate)
            {
                NewRow();
            }
        }

        protected void AddSpriteWithTint(string key, string tintKey, string name, float labelWidth, float entryWidth)
        {

            AddSpriteWithTint(serializedObject.FindProperty(key), serializedObject.FindProperty(tintKey), name, labelWidth, entryWidth);
        }

        protected void AddSpriteWithTint(SerializedProperty sprite, SerializedProperty tint, string name, float labelWidth, float entryWidth)
        {
            DrawRect(m_coords.x, m_coords.y + 5.0f, labelWidth, labelWidth + 20.0f);

            AddProperty(sprite, ValueType._sprite, name, m_coords.x, m_coords.y, labelWidth, entryWidth);


            Rect rect = GetRect(m_coords.x, m_coords.y + 35.0f + entryWidth, entryWidth, 20.0f);
            tint.colorValue = EditorGUI.ColorField(rect, tint.colorValue);
            SetNextRowOffset(30.0f + entryWidth);
            IncreasePosition(labelWidth + 20.0f);
        }

        protected void AddDividerBar(float offSet = 0.0f)
        {
            Rect rect = new Rect(0.0f, m_coords.y + offSet + 4.5f, float.MaxValue, 1.0f);
            EditorGUI.DrawRect(rect, Color.grey);
        }

        private void DrawRect(float x, float y, float height, float width)
        {
            return;
            EditorGUI.DrawRect(new Rect(x + 15.0f, y - 10.0f, height + 20.0f, width + 20.0f), Color.black);
            EditorGUI.DrawRect(new Rect(x + 17.5f, y - 7.5f, height + 15.0f, width + 15.0f), Color.grey);
        }
    }
}
