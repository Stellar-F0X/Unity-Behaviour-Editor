using System;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    [CustomEditor(typeof(NodeBase), true)]
    public class NodeBaseCustomEditor : Editor
    {
        private GUIStyle _headerLabelStyle;
        

        public override void OnInspectorGUI()
        {
            this.ParentSerializedField();

            this.ChildSerializedFields(serializedObject.FindProperty("_parent"));
        }

        
        protected virtual void ParentSerializedField()
        {
            _headerLabelStyle = new GUIStyle(EditorStyles.toolbar)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                fontSize = 13,
            };
            
            using (new GUIColorScope(new Color32(255, 255, 255, 255), GUIColorScope.EGUIColorScope.Background))
            {
                EditorGUILayout.LabelField("Information", _headerLabelStyle);
            }

            EditorGUILayout.Space(2);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("_guid"));
            }

            SerializedProperty nameProp = serializedObject.FindProperty("m_Name");
            SerializedProperty tagProp = serializedObject.FindProperty("_tag");
            SerializedProperty desProp = serializedObject.FindProperty("_description");

            using (new EditorGUI.DisabledScope(!BehaviorEditor.canEditGraph))
            {
                nameProp.stringValue = EditorGUILayout.TextField("Name", nameProp.stringValue);
                tagProp.stringValue = EditorGUILayout.TextField("Tag", tagProp.stringValue);

                EditorGUILayout.LabelField("Description");
                desProp.stringValue = EditorGUILayout.TextArea(desProp.stringValue, GUILayout.Height(EditorGUIUtility.singleLineHeight * 3));
            }
        }


        protected virtual void ChildSerializedFields(SerializedProperty iterator)
        {
            EditorGUILayout.Space(10);

            if (iterator.NextVisible(false))
            {
                using (new GUIColorScope(new Color32(255, 255, 255, 255), GUIColorScope.EGUIColorScope.Background))
                {
                    EditorGUILayout.LabelField(this.target.name, _headerLabelStyle);
                }

                EditorGUILayout.Space(2);

                do
                    EditorGUILayout.PropertyField(iterator, true);
                while (iterator.NextVisible(false));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}