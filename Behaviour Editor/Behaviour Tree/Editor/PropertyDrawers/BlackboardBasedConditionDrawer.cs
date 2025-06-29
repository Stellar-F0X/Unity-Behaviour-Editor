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
                Blackboard blackboardData = BehaviorEditor.Instance.graph.blackboard;
                _rect = new Rect(position.x, position.y, position.width - 10, EditorGUIUtility.singleLineHeight);

                float width = _rect.width / 3;

                Rect dropdownRect = new Rect(_rect.x, _rect.y + 2, width, _rect.height);
                SerializedProperty blackboardProp = property.FindPropertyRelative("property");
                
                if (this.TryDrawBlackboardProperty(blackboardData, blackboardProp, dropdownRect))
                {
                    IBlackboardProperty prop = blackboardProp.boxedValue as IBlackboardProperty;
                    bool isTriggerCondition = (prop!.comparableConditions & EConditionType.Trigger) == EConditionType.Trigger;
                    
                    SerializedProperty conditionType = property.FindPropertyRelative("conditionType");

                    if (isTriggerCondition)
                    {
                        this.DrawCompareCondition(conditionType, prop, new Rect(_rect.x + width + 5, _rect.y + 2, width * 2, _rect.height));
                    }
                    else
                    {
                        this.DrawCompareCondition(conditionType, prop, new Rect(_rect.x + width + 5, _rect.y + 2, width, _rect.height));
                        SerializedProperty targetComparableProp = property.FindPropertyRelative("comparableValue");
                        this.AllocateCompareValue(targetComparableProp, blackboardProp);

                        SerializedProperty targetPropValue = targetComparableProp.FindPropertyRelative("_value");
                        Rect valueRect = new Rect(_rect.x + width * 2 + 10, _rect.y + 2, width, _rect.height);
                        EditorGUI.PropertyField(valueRect, targetPropValue, GUIContent.none);
                    }

                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }


        
        private bool TryDrawBlackboardProperty(Blackboard data, SerializedProperty blackboardProp, Rect dropdownRect)
        {
            if (data is null || data.properties is null || data.properties.Count == 0)
            {
                GUIContent warningIcon = EditorGUIUtility.IconContent("console.warnicon");
                EditorGUI.LabelField(_rect, new GUIContent("No blackboard properties found.", warningIcon.image));
                return false;
            }

            IBlackboardProperty[] properties = this.GetUsableBlackboardProperties(data);

            if (properties.Length == 0)
            {
                GUIContent warningIcon = EditorGUIUtility.IconContent("console.warnicon");
                EditorGUI.LabelField(_rect, new GUIContent("Cannot find a blackboard property with the given key ID.", warningIcon.image));
                return false;
            }

            //None 옵션을 포함한 dropdown 옵션 생성
            string[] dropdownOptions = new string[properties.Length + 1];
            
            dropdownOptions[0] = "None";
            
            for (int i = 0; i < properties.Length; i++)
            {
                dropdownOptions[i + 1] = properties[i].key;
            }

            int selected = 0; //기본값은 None

            if (blackboardProp.boxedValue is IBlackboardProperty prop)
            {
                int propIndex = Array.IndexOf(properties, prop);
                
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
                blackboardProp.boxedValue = properties[selected - 1]; // None이 0번이므로 -1
            }
            
            EditorGUI.EndDisabledGroup();
            return true;
        }


        private void AllocateCompareValue(SerializedProperty targetProperty, SerializedProperty blackboardProp)
        {
            SerializedProperty sourceValueTypeName = blackboardProp.FindPropertyRelative("_typeName");

            if (targetProperty.boxedValue is null) //비교할 대상이 null이면 새로 생성.
            {
                targetProperty.boxedValue = IBlackboardProperty.Create(Type.GetType(sourceValueTypeName.stringValue));
            }
            else //비교할 대상의 타입이 일치하지 않을 경우로, 다를 경우 새로 할당한다. 
            {
                SerializedProperty targetValueTypeName = targetProperty.FindPropertyRelative("_typeName");

                if (string.Compare(targetValueTypeName.stringValue, sourceValueTypeName.stringValue, StringComparison.Ordinal) != 0)
                {
                    targetProperty.boxedValue = IBlackboardProperty.Create(Type.GetType(sourceValueTypeName.stringValue));
                }
            }
        }


        private void DrawCompareCondition(SerializedProperty conditionType, IBlackboardProperty sourceType, Rect compareRect)
        {
            List<string> conditionTypes = ListPool<string>.Get();
            List<int> conditionIndex = ListPool<int>.Get();

            this.GetCompatibleConditionTypes(sourceType.comparableConditions, conditionTypes, conditionIndex);
            
            EditorGUI.BeginDisabledGroup(!BehaviorEditor.canEditGraph);
            int prev = Mathf.Max(conditionIndex.IndexOf(conditionType.enumValueFlag), 0);
            int index = EditorGUI.Popup(compareRect, prev, conditionTypes.ToArray(), _popupStyle);
            conditionType.enumValueFlag = conditionIndex[index];
            EditorGUI.EndDisabledGroup();

            ListPool<string>.Release(conditionTypes);
            ListPool<int>.Release(conditionIndex);
        }


        private void GetCompatibleConditionTypes(EConditionType conditionValue, List<string> conditionTypes, List<int> conditionIndex)
        {
            for (int i = (int)EConditionType.Trigger; i <= (int)conditionValue; i <<= 1)
            {
                EConditionType condition = (EConditionType)i;

                if ((condition & conditionValue) == condition)
                {
                    conditionIndex.Add(i);

                    switch (condition)
                    {
                        case EConditionType.Trigger: conditionTypes.Add("!!"); break;

                        case EConditionType.Equal: conditionTypes.Add("="); break;

                        case EConditionType.NotEqual: conditionTypes.Add("≠"); break;

                        case EConditionType.GreaterThan: conditionTypes.Add(">"); break;

                        case EConditionType.GreaterThanOrEqual: conditionTypes.Add("≥"); break;

                        case EConditionType.LessThan: conditionTypes.Add("<"); break;

                        case EConditionType.LessThanOrEqual: conditionTypes.Add("≤"); break;
                    }
                }
            }
        }


        private IBlackboardProperty[] GetUsableBlackboardProperties(Blackboard data)
        {
            List<IBlackboardProperty> cachedPropertyList = ListPool<IBlackboardProperty>.Get();

            foreach (var prop in data.properties)
            {
                if (prop.comparableConditions == EConditionType.None)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(prop.key) == false)
                {
                    cachedPropertyList.Add(prop);
                }
            }

            IBlackboardProperty[] resultArray = cachedPropertyList.ToArray();
            ListPool<IBlackboardProperty>.Release(cachedPropertyList);
            return resultArray;
        }
    }
}