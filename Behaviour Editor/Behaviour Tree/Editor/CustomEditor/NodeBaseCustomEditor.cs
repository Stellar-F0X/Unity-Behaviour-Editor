using System;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    [CustomEditor(typeof(NodeBase), true)]
    public class NodeBaseCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUIStyle headerLabelStyle = new GUIStyle(EditorStyles.toolbar);
            headerLabelStyle.alignment = TextAnchor.MiddleLeft;
            headerLabelStyle.fontStyle = FontStyle.Bold;
            headerLabelStyle.fontSize = 13;

            using (new GUIColorScope(new Color32(255, 255, 255, 255), GUIColorScope.EGUIColorScope.Background))
            {
                EditorGUILayout.LabelField("Information", headerLabelStyle);
            }

            EditorGUILayout.Space(2);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("guid"));
            }
            
            SerializedProperty nameProp = serializedObject.FindProperty("m_Name");
            SerializedProperty tagProp = serializedObject.FindProperty("tag");
            SerializedProperty desProp = serializedObject.FindProperty("description");
            SerializedProperty iterator = desProp;

            using (new EditorGUI.DisabledScope(!BehaviourTreeEditor.CanEditTree))
            {
                nameProp.stringValue = EditorGUILayout.TextField("Name", nameProp.stringValue);
                tagProp.stringValue = EditorGUILayout.TextField("Tag", tagProp.stringValue);

                EditorGUILayout.LabelField("Description");
                desProp.stringValue = EditorGUILayout.TextArea(desProp.stringValue, GUILayout.Height(EditorGUIUtility.singleLineHeight * 3));
            }

            EditorGUILayout.Space(10);

            while (iterator.NextVisible(false) && iterator.name.Equals("position", StringComparison.Ordinal) == false) ;

            if (iterator.NextVisible(false))
            {
                using (new GUIColorScope(new Color32(255, 255, 255, 255), GUIColorScope.EGUIColorScope.Background))
                {
                    EditorGUILayout.LabelField(this.target.name, headerLabelStyle);
                }
                
                EditorGUILayout.Space(2);
                
                do EditorGUILayout.PropertyField(iterator);
                while (iterator.NextVisible(false));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}