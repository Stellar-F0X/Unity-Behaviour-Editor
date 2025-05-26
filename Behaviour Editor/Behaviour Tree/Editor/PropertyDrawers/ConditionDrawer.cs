using System;
using System.Collections.Generic;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    [CustomPropertyDrawer(typeof(BlackboardBasedCondition))]
    public class ConditionDrawer : PropertyDrawer
    {
        private readonly List<IBlackboardProperty> _cachedPropertyList = new List<IBlackboardProperty>();

        private readonly List<string> _conditionTypes = new List<string>();

        private bool _canDraw = false;

        private bool _initialized = false;

        private Rect _rect;

        private GUIStyle _popupStyle;


        private void Initialize()
        {
            _initialized = true;

            _popupStyle = new GUIStyle(EditorStyles.popup);
            _popupStyle.fontSize = 17;
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + 4;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (BehaviourTreeEditor.Instance is null)
            {
                return;
            }

            if (_initialized == false)
            {
                this.Initialize();
            }

            Blackboard data = BehaviourTreeEditor.Instance.Tree.blackboard;
            SerializedProperty blackboardProp = property.FindPropertyRelative("property");

            _rect = new Rect(position.x, position.y, position.width - 10, EditorGUIUtility.singleLineHeight);

            Rect dropdownRect = new Rect(_rect.x, _rect.y + 2, _rect.width / 5, _rect.height);
            Rect compareRect = new Rect(_rect.x + _rect.width / 5 + 5, _rect.y + 2, _rect.width / 5, _rect.height);
            Rect valueRect = new Rect(_rect.x + _rect.width / 5 * 2 + 10, _rect.y + 2, _rect.width / 5 * 3, _rect.height);

            this.DrawBlackboardProperty(data, blackboardProp, dropdownRect);

            if (_canDraw)
            {
                this.DrawCompareValueField(property, blackboardProp, compareRect, valueRect);
            }
        }


        private void DrawBlackboardProperty(Blackboard data, SerializedProperty blackboardProp, Rect dropdownRect)
        {
            if (data.properties.Count == 0)
            {
                GUIContent warningIcon = EditorGUIUtility.IconContent("console.warnicon");
                EditorGUI.LabelField(_rect, new GUIContent("No blackboard properties found.", warningIcon.image));
                _canDraw = false;
                return;
            }

            IBlackboardProperty[] properties = this.GetUsableBlackboardProperties(data);

            if (properties.Length == 0)
            {
                GUIContent warningIcon = EditorGUIUtility.IconContent("console.warnicon");
                EditorGUI.LabelField(_rect, new GUIContent("Cannot find a blackboard property with the given key ID.", warningIcon.image));
                _canDraw = false;
                return;
            }

            string[] dropdownOptions = new string[properties.Length];

            for (int i = 0; i < properties.Length; i++)
            {
                dropdownOptions[i] = properties[i].key;
            }

            int selected = 0;

            if (blackboardProp.boxedValue is IBlackboardProperty prop)
            {
                selected = Array.IndexOf(dropdownOptions, prop.key);
                _canDraw = (prop.comparableConditions & EConditionType.Trigger) == 0;
                
                selected = Mathf.Clamp(selected, 0, dropdownOptions.Length - 1);
                selected = EditorGUI.Popup(dropdownRect, selected, dropdownOptions);
                blackboardProp.boxedValue = properties[selected];
                return;
            }

            _canDraw = false;
        }


        private void DrawCompareValueField(SerializedProperty property, SerializedProperty blackboardProp, Rect compareRect, Rect valueRect)
        {
            SerializedProperty targetValue = property.FindPropertyRelative("comparableValue");
            SerializedProperty sourceValueTypeName = blackboardProp.FindPropertyRelative("_typeName");

            if (targetValue.boxedValue is null)
            {
                targetValue.boxedValue = IBlackboardProperty.Create(Type.GetType(sourceValueTypeName.stringValue));
            }
            else
            {
                SerializedProperty targetValueTypeName = targetValue.FindPropertyRelative("_typeName");

                if (string.Compare(targetValueTypeName.stringValue, sourceValueTypeName.stringValue, StringComparison.Ordinal) != 0)
                {
                    targetValue.boxedValue = IBlackboardProperty.Create(Type.GetType(sourceValueTypeName.stringValue));
                }
            }

            this.DrawCompareCondition(property, blackboardProp.boxedValue as IBlackboardProperty, compareRect);

            EditorGUI.PropertyField(valueRect, targetValue.FindPropertyRelative("_value"), GUIContent.none);
        }


        private void DrawCompareCondition(SerializedProperty property, IBlackboardProperty sourceType, Rect compareRect)
        {
            if ((sourceType.comparableConditions & EConditionType.None) == EConditionType.None)
            {
                return;
            }

            SerializedProperty conditionType = property.FindPropertyRelative("conditionType");
            int selected = conditionType.enumValueIndex - 1;

            _conditionTypes.Clear();

            for (int i = (int)EConditionType.None; i <= (int)sourceType.comparableConditions; i <<= 1)
            {
                EConditionType condition = (EConditionType)i;

                if ((condition & sourceType.comparableConditions) == condition)
                {
                    switch (condition)
                    {
                        case EConditionType.Trigger: _conditionTypes.Add("Trigger"); break;

                        case EConditionType.Equal: _conditionTypes.Add("="); break;

                        case EConditionType.NotEqual: _conditionTypes.Add("≠"); break;

                        case EConditionType.GreaterThan: _conditionTypes.Add(">"); break;

                        case EConditionType.GreaterThanOrEqual: _conditionTypes.Add("≥"); break;

                        case EConditionType.LessThan: _conditionTypes.Add("<"); break;

                        case EConditionType.LessThanOrEqual: _conditionTypes.Add("≤"); break;
                    }
                }
            }

            selected = Mathf.Clamp(selected, 0, _conditionTypes.Count - 1);
            conditionType.enumValueIndex = EditorGUI.Popup(compareRect, selected, _conditionTypes.ToArray(), _popupStyle) + 1;
        }


        private IBlackboardProperty[] GetUsableBlackboardProperties(Blackboard data)
        {
            _cachedPropertyList.Clear();

            foreach (var prop in data.properties)
            {
                if ((prop.comparableConditions & EConditionType.None) == EConditionType.None)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(prop.key) == false)
                {
                    _cachedPropertyList.Add(prop);
                }
            }

            return _cachedPropertyList.ToArray();
        }
    }
}