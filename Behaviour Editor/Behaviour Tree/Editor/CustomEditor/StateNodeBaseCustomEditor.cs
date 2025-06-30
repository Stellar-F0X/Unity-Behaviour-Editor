using BehaviourSystem.BT.State;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    [CustomEditor(typeof(StateNodeBase), true)]
    public class StateNodeBaseCustomEditor : NodeBaseCustomEditor
    {
        private ReorderableList _transitionsList;

        protected virtual float propertyHeight
        {
            get { return 5; }
        }


        public override void OnInspectorGUI()
        {
            this.DrawBasedSerializedField();
            SerializedProperty startProp = serializedObject.FindProperty("position");

            if (this.HasRemainingPropertiesAfter(startProp))
            {
                this.DrawHeader(this.target.name, 10f, 4f);
                SerializedProperty stopProp = serializedObject.FindProperty("transitions");

                base.DrawPropertiesRange(startProp, stopProp, startInclusive: false);
                this.DrawTransitionList();

                // transitions 리스트 그리기
                serializedObject.Update();
                _transitionsList.DoLayoutList();

                if (stopProp.NextVisible(false))
                {
                    base.DrawPropertiesRange(stopProp);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }


        protected virtual void DrawTransitionList()
        {
            //transitions 리스트를 위한 ReorderableList 생성.
            SerializedProperty transitionsPro = serializedObject.FindProperty("transitions");
            _transitionsList = new ReorderableList(serializedObject, transitionsPro, true, true, false, false);

            //대리자에 함수 할당.
            _transitionsList.drawHeaderCallback = this.DrawTransitionHeader;
            _transitionsList.drawElementBackgroundCallback = this.DrawTransitionElementBackground;
            _transitionsList.drawElementCallback = this.DrawTransitionElement;
            _transitionsList.elementHeightCallback = this.GetTransitionElementHeight;
        }


        protected virtual void DrawTransitionHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Transitions");
        }

        
        protected virtual void DrawTransitionElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Color backgroundColor;

                if (index % 2 == 0)
                {
                    backgroundColor = new Color(0.4f, 0.4f, 0.4f, 0.2f);
                }
                else
                {
                    backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.2f);
                }

                EditorGUI.DrawRect(rect, backgroundColor);
            }
        }


        protected virtual void DrawTransitionElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty transitionsPro = serializedObject.FindProperty("transitions");
            SerializedProperty element = transitionsPro.GetArrayElementAtIndex(index);
            rect = new Rect(rect.x, rect.y + 2, rect.width, EditorGUI.GetPropertyHeight(element));
            EditorGUI.PropertyField(rect, element, new GUIContent($"Transition {index}"), true);
        }


        protected virtual float GetTransitionElementHeight(int index)
        {
            SerializedProperty transitionsPro = serializedObject.FindProperty("transitions");
            SerializedProperty element = transitionsPro.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element) + propertyHeight;
        }
    }
}