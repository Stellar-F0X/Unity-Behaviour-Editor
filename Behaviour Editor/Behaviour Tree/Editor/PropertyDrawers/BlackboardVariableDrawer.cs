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
    [CustomPropertyDrawer(typeof(BlackboardVariable<>))]
    public class BlackboardVariableDrawer : PropertyDrawer
    {
        private const BindingFlags _BINDING_FLAG = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private const float _ICON_SIZE = 12f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (BehaviorEditor.Instance is null)
            {
                return;
            }

            BlackboardAsset blackboardAsset = BehaviorEditor.Instance.graph.blackboardAsset;

            bool exception = blackboardAsset is null || blackboardAsset.variables is null || blackboardAsset.variables.Count == 0;

            float width = EditorGUIUtility.labelWidth;
            float height = EditorGUIUtility.singleLineHeight;

            Rect labelRect = new Rect(position.x, position.y, width, height);
            Rect fieldRect = new Rect(position.x + width + 2, position.y, position.width - width - 2, height);

            List<BlackboardVariable> variables = ListPool<BlackboardVariable>.Get();

            if (exception == false && this.GetAssignableVariable(blackboardAsset, property, variables))
            {
                this.DrawPopup(position, labelRect, fieldRect, property, label, variables);
            }
            else
            {
                this.DrawNoPropertiesWarning(position, labelRect, fieldRect, property, label);
            }

            ListPool<BlackboardVariable>.Release(variables);
        }


        private bool GetAssignableVariable(BlackboardAsset blackboardAsset, SerializedProperty property, List<BlackboardVariable> variables)
        {
            if (property.serializedObject.targetObject is null)
            {
                return false;
            }

            Type targetType = fieldInfo.FieldType;

            for (int i = 0; i < blackboardAsset.variables.Count; i++)
            {
                if (string.IsNullOrEmpty(blackboardAsset.variables[i].name))
                {
                    continue;
                }

                Type propertyType = blackboardAsset.variables[i].GetType();

                if (targetType.IsAssignableFrom(propertyType))
                {
                    variables.Add(blackboardAsset.variables[i]);
                }
            }

            return variables.Count > 0;
        }


        //Blackboard Variable로 구성된 팝업을 그린다. 
        private void DrawPopup(Rect position, Rect labelRect, Rect fieldRect, SerializedProperty property, GUIContent label, List<BlackboardVariable> variables)
        {
            List<string> keyNamesList = ListPool<string>.Get();
            keyNamesList.Add("None");
            keyNamesList.AddRange(variables.Select(key => key.name));
            string[] keyNames = keyNamesList.ToArray();
            ListPool<string>.Release(keyNamesList);

            int selectedIndex = 0; // 기본값은 None.

            if (property.boxedValue is not null)
            {
                SerializedProperty keyProp = property.FindPropertyRelative("_name");

                if (string.IsNullOrEmpty(keyProp.stringValue) == false)
                {
                    //property.boxedValue is not null가 True였다면 무조건 0보다 큰 인덱스일 것이므로 None을 제외하기 위해, 1을 빼준다.
                    int valueIndex = Array.IndexOf(keyNames, keyProp.stringValue) - 1;

                    //property가 이미 값이 있다면 None이라는 허상의 값이 Popup 리스트에서 첫번째 위치에 자리하고 있으므로 +1로 자리를 맞춰줌.
                    selectedIndex = valueIndex >= 0 ? valueIndex + 1 : valueIndex;
                }
            }

            selectedIndex = Mathf.Clamp(selectedIndex, 0, keyNames.Length - 1);

            using (new EditorGUI.PropertyScope(position, label, property))
            {
                using (new EditorGUI.DisabledScope(!BehaviorEditor.canEditGraph))
                {
                    EditorGUI.PrefixLabel(labelRect, label);
                    int newSelectedIndex = EditorGUI.Popup(fieldRect, selectedIndex, keyNames);
                    property.boxedValue = newSelectedIndex == 0 ? null : variables[newSelectedIndex - 1]; //값을 더할 땐, 다시 뺌.
                }
            }
        }


        private void DrawNoPropertiesWarning(Rect position, Rect labelRect, Rect fieldRect, SerializedProperty property, GUIContent label)
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
    }
}