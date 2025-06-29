using BehaviourSystem.BT;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    [CustomPropertyDrawer(typeof(Transition))]
    public class TransitionDrawer : PropertyDrawer
    {
        private readonly float _LINE_HEIGHT = EditorGUIUtility.singleLineHeight;

        private readonly float _LINE_OFFSET = 5f;


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                float currentY = this.DrawUnconditionalField(position, property, position.y);
                currentY = this.DrawTargetStateGuidField(position, property, currentY);
                this.DrawConditionsField(position, property, currentY);
            }
        }


        private float DrawUnconditionalField(Rect position, SerializedProperty property, float startY)
        {
            SerializedProperty isUnconditionalProp = property.FindPropertyRelative("isUnconditional");
            Rect unconditionalRect = new Rect(position.x, startY, position.width, _LINE_HEIGHT);
            EditorGUI.PropertyField(unconditionalRect, isUnconditionalProp, new GUIContent("Unconditional"));
            return startY + _LINE_HEIGHT + _LINE_OFFSET;
        }


        private float DrawTargetStateGuidField(Rect position, SerializedProperty property, float startY)
        {
            SerializedProperty nextStateGuidProp = property.FindPropertyRelative("nextStateNodeGuid");
            Rect guidRect = new Rect(position.x, startY, position.width, _LINE_HEIGHT);
            EditorGUI.PropertyField(guidRect, nextStateGuidProp, new GUIContent("Target State"));
            return startY + _LINE_HEIGHT + _LINE_OFFSET;
        }


        private void DrawConditionsField(Rect position, SerializedProperty property, float startY)
        {
            SerializedProperty isUnconditionalProp = property.FindPropertyRelative("isUnconditional");

            if (isUnconditionalProp.boolValue == false)
            {
                SerializedProperty conditionsProp = property.FindPropertyRelative("conditions");
                float conditionsHeight = EditorGUI.GetPropertyHeight(conditionsProp, true);
                Rect conditionsRect = new Rect(position.x, startY, position.width, conditionsHeight);
                EditorGUI.PropertyField(conditionsRect, conditionsProp, new GUIContent("Conditions"), true);
            }
        }
        
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = _LINE_HEIGHT;
            height += _LINE_HEIGHT;

            SerializedProperty isUnconditionalProp = property.FindPropertyRelative("isUnconditional");

            if (isUnconditionalProp.boolValue == false)
            {
                SerializedProperty conditionsProp = property.FindPropertyRelative("conditions");
                height += EditorGUI.GetPropertyHeight(conditionsProp, true);
            }

            return height + _LINE_OFFSET * 2;
        }
    }
}