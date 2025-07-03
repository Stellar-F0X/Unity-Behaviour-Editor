using System;
using System.Collections.Generic;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace BehaviourSystemEditor.BT
{
    [CustomPropertyDrawer(typeof(BlackboardBasedCondition))]
    public class BlackboardBasedConditionDrawer : PropertyDrawer
    {
        private Rect _rect;

        private readonly GUIStyle _popupStyle = new GUIStyle(EditorStyles.popup) { fontSize = 16 };


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 4;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (BehaviorEditor.Instance is not null)
            {
                BlackboardAsset blackboardAssetData = BehaviorEditor.Instance.graph.blackboardAsset;
                _rect = new Rect(position.x, position.y, position.width - 10, EditorGUIUtility.singleLineHeight);

                float width = _rect.width / 3;

                Rect dropdownRect = new Rect(_rect.x, _rect.y + 2, width, _rect.height);
                SerializedProperty blackboardVariable = property.FindPropertyRelative("blackboardVariable");

                if (this.TryDrawBlackboardProperty(blackboardAssetData, blackboardVariable, dropdownRect))
                {
                    if (blackboardVariable.boxedValue is BlackboardVariable variable)
                    {
                        SerializedProperty conditionType = property.FindPropertyRelative("comparableType");
                        this.DrawCompareCondition(conditionType, variable, new Rect(_rect.x + width + 5, _rect.y + 2, width, _rect.height));
                        SerializedProperty targetComparableProp = property.FindPropertyRelative("comparableValue");
                        SerializedProperty internalVariableProp = blackboardVariable.FindPropertyRelative("_variable");
                        this.AllocateCompareValue(targetComparableProp, internalVariableProp);

                        SerializedProperty targetPropValue = targetComparableProp.FindPropertyRelative("_value");
                        Rect valueRect = new Rect(_rect.x + width * 2 + 10, _rect.y + 2, width, _rect.height);
                        EditorGUI.PropertyField(valueRect, targetPropValue, GUIContent.none);
                    }

                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }



        private bool TryDrawBlackboardProperty(BlackboardAsset data, SerializedProperty blackboardProp, Rect dropdownRect)
        {
            if (data is null || data.variables is null || data.variables.Count == 0)
            {
                GUIContent warningIcon = EditorGUIUtility.IconContent("console.warnicon");
                EditorGUI.LabelField(_rect, new GUIContent("No blackboard variables found.", warningIcon.image));
                return false;
            }

            BlackboardVariable[] variables = this.GetUsableBlackboardProperties(data);

            if (variables.Length == 0)
            {
                GUIContent warningIcon = EditorGUIUtility.IconContent("console.warnicon");
                EditorGUI.LabelField(_rect, new GUIContent("Cannot find a blackboard property with the given key ID.", warningIcon.image));
                return false;
            }

            //None 옵션을 포함한 dropdown 옵션 생성
            string[] dropdownOptions = new string[variables.Length + 1];

            dropdownOptions[0] = "None";

            for (int i = 0; i < variables.Length; i++)
            {
                dropdownOptions[i + 1] = variables[i].name;
            }

            int selected = 0; //기본값은 None

            if (blackboardProp.boxedValue is BlackboardVariable
                prop)
            {
                int propIndex = Array.IndexOf(variables, prop);

                if (propIndex >= 0)
                {
                    selected = propIndex + 1; //None이 0번 인덱스이므로 +1
                }
            }

            EditorGUI.BeginDisabledGroup(!BehaviorEditor.canEditGraph);
            selected = Mathf.Clamp(selected, 0, dropdownOptions.Length - 1);
            selected = EditorGUI.Popup(dropdownRect, selected, dropdownOptions);

            //None이 선택된 경우 null로 설정, 그렇지 않으면 해당 property 설정
            if (selected == 0)
            {
                blackboardProp.boxedValue = null;
                EditorGUI.EndDisabledGroup();
                return false; // null이므로 조건 설정 UI를 그리지 않음
            }
            else
            {
                blackboardProp.boxedValue = variables[selected - 1]; // None이 0번이므로 -1
            }

            EditorGUI.EndDisabledGroup();
            return true;
        }


        private void AllocateCompareValue(SerializedProperty targetProperty, SerializedProperty internalVariableProp)
        {
            SerializedProperty sourceValueTypeName = internalVariableProp.FindPropertyRelative("_typeName");

            if (targetProperty.boxedValue is null) //비교할 대상이 null이면 새로 생성.
            {
                targetProperty.boxedValue = Variable.Create(Type.GetType(sourceValueTypeName.stringValue));
            }
            else //비교할 대상의 타입이 일치하지 않을 경우로, 다를 경우 새로 할당한다. 
            {
                SerializedProperty targetValueTypeName = targetProperty.FindPropertyRelative("_typeName");

                if (string.Compare(targetValueTypeName.stringValue, sourceValueTypeName.stringValue, StringComparison.Ordinal) != 0)
                {
                    targetProperty.boxedValue = Variable.Create(Type.GetType(sourceValueTypeName.stringValue));
                }
            }
        }


        private void DrawCompareCondition(SerializedProperty conditionType, BlackboardVariable sourceType, Rect compareRect)
        {
            List<string> conditionTypes = ListPool<string>.Get();
            List<int> conditionIndex = ListPool<int>.Get();

            this.GetCompatibleConditionTypes(sourceType.comparison, conditionTypes, conditionIndex);

            EditorGUI.BeginDisabledGroup(!BehaviorEditor.canEditGraph);
            int prev = Mathf.Max(conditionIndex.IndexOf(conditionType.enumValueFlag), 0);
            int index = EditorGUI.Popup(compareRect, prev, conditionTypes.ToArray(), _popupStyle);
            conditionType.enumValueFlag = conditionIndex[index];
            EditorGUI.EndDisabledGroup();

            ListPool<string>.Release(conditionTypes);
            ListPool<int>.Release(conditionIndex);
        }


        private void GetCompatibleConditionTypes(EComparisonType conditionValue, List<string> conditionTypes, List<int> conditionIndex)
        {
            for (int i = (int)EComparisonType.Equal; i <= (int)conditionValue; i <<= 1)
            {
                EComparisonType condition = (EComparisonType)i;

                if ((condition & conditionValue) == condition)
                {
                    conditionIndex.Add(i);

                    switch (condition)
                    {
                        case EComparisonType.Equal: conditionTypes.Add("=");              break;
                        case EComparisonType.NotEqual: conditionTypes.Add("≠");           break;
                        case EComparisonType.GreaterThan: conditionTypes.Add(">");        break;
                        case EComparisonType.GreaterThanOrEqual: conditionTypes.Add("≥"); break;
                        case EComparisonType.LessThan: conditionTypes.Add("<");           break;
                        case EComparisonType.LessThanOrEqual: conditionTypes.Add("≤");    break;
                    }
                }
            }
        }


        private BlackboardVariable[] GetUsableBlackboardProperties(BlackboardAsset data)
        {
            List<BlackboardVariable> cachedVariableList = ListPool<BlackboardVariable>.Get();

            foreach (var prop in data.variables)
            {
                if (prop.comparison == EComparisonType.None)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(prop.name) == false)
                {
                    cachedVariableList.Add(prop);
                }
            }

            BlackboardVariable[] resultArray = cachedVariableList.ToArray();
            ListPool<BlackboardVariable>.Release(cachedVariableList);
            return resultArray;
        }
    }
}