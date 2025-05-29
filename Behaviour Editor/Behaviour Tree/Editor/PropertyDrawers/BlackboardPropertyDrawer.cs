using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace BehaviourSystemEditor.BT
{
    [CustomPropertyDrawer(typeof(BlackboardProperty<>))]
    public class BlackboardPropertyDrawer : PropertyDrawer
    {
        private const BindingFlags _BINDING_FLAG = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private const float _ICON_SIZE = 12f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (BehaviourTreeEditor.Instance is null)
            {
                return;
            }

            Blackboard blackboard = BehaviourTreeEditor.Instance.Tree.blackboard;

            bool exception = blackboard is null || blackboard.properties is null || blackboard.properties.Count == 0;

            float width = EditorGUIUtility.labelWidth;
            float height = EditorGUIUtility.singleLineHeight;

            Rect labelRect = new Rect(position.x, position.y, width, height);
            Rect fieldRect = new Rect(position.x + width + 2, position.y, position.width - width - 2, height);

            List<IBlackboardProperty> properties = ListPool<IBlackboardProperty>.Get();
            
            if (exception == false && this.GetProperties(blackboard, property, properties))
            {
                if (property.boxedValue is null)
                {
                    property.boxedValue = properties.First();
                }

                SerializedProperty keyProp = property.FindPropertyRelative("_key");
                string[] keyNames = properties.ConvertAll(key => key.key).ToArray();
                int selectedIndex = string.IsNullOrEmpty(keyProp.stringValue) ? 0 : Array.IndexOf(keyNames, keyProp.stringValue);
                selectedIndex = Mathf.Clamp(selectedIndex, 0, keyNames.Length - 1);

                using (new EditorGUI.PropertyScope(position, label, property))
                {
                    using (new EditorGUI.DisabledScope(BehaviourTreeEditor.CanEditTree == false))
                    {
                        EditorGUI.PrefixLabel(labelRect, label);
                        selectedIndex = EditorGUI.Popup(fieldRect, selectedIndex, keyNames);
                        property.boxedValue = properties[selectedIndex];
                    }
                }

                property.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                using (new EditorGUI.PropertyScope(position, label, property))
                {
                    EditorGUI.PrefixLabel(labelRect, label);

                    Rect iconRect = new Rect(fieldRect.x, fieldRect.y + (fieldRect.height - _ICON_SIZE) * 0.5f, _ICON_SIZE, _ICON_SIZE);
                    Rect textRect = new Rect(fieldRect.x + _ICON_SIZE + 2f, fieldRect.y, fieldRect.width - _ICON_SIZE - 2f, fieldRect.height);

                    Texture warningImg = EditorGUIUtility.IconContent("console.warnicon").image;
                    GUI.DrawTexture(iconRect, warningImg, ScaleMode.ScaleToFit);

                    EditorGUI.LabelField(textRect, "No blackboard properties found.");
                }
            }
            
            ListPool<IBlackboardProperty>.Release(properties);
        }


        private bool GetProperties(Blackboard blackboard, SerializedProperty property, List<IBlackboardProperty> properties)
        {
            if (property.serializedObject.targetObject == null)
            {
                return false;
            }

            Type targetType = this.GetPropertyType(property);

            if (targetType == null)
            {
                return false;
            }

            for (int i = 0; i < blackboard.properties.Count; i++)
            {
                if (string.IsNullOrEmpty(blackboard.properties[i].key))
                {
                    continue;
                }

                Type propertyType = blackboard.properties[i].type;

                if (targetType.IsAssignableFrom(propertyType))
                {
                    properties.Add(blackboard.properties[i]);
                }
            }

            return properties.Count > 0;
        }



        private Type GetPropertyType(SerializedProperty property)
        {
            if (property.boxedValue is IBlackboardProperty castedProperty && castedProperty.type != null)
            {
                return castedProperty.type;
            }

            FieldInfo info = property.serializedObject.targetObject.GetType()?.GetField(property.name, _BINDING_FLAG);

            if (info is not null)
            {
                return info.FieldType;
            }

            return null;
        }
    }
}