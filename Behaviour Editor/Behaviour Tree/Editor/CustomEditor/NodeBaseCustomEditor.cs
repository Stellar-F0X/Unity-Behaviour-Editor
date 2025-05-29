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
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            }

            EditorGUILayout.Space(2);

            SerializedProperty tagProp = serializedObject.FindProperty("tag");
            SerializedProperty desProp = serializedObject.FindProperty("description");
            SerializedProperty iterator = desProp;

            GUIStyle boldLabelStyle = new GUIStyle(EditorStyles.label);
            boldLabelStyle.fontStyle = FontStyle.Bold;
            boldLabelStyle.fontSize = 13;
            
            tagProp.stringValue = EditorGUILayout.TextField("Tag", tagProp.stringValue);

            EditorGUILayout.LabelField("Description");
            GUILayoutOption height = GUILayout.Height(EditorGUIUtility.singleLineHeight * 3);

            desProp.stringValue = EditorGUILayout.TextArea(desProp.stringValue, height);

            while (iterator.NextVisible(false) && iterator.name.Equals("position", StringComparison.Ordinal) == false) ;

            if (iterator.NextVisible(false))
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField(this.target.name, boldLabelStyle);
                EditorGUILayout.BeginVertical(new GUIStyle(GUI.skin.box));

                do EditorGUILayout.PropertyField(iterator);
                while (iterator.NextVisible(false));

                EditorGUILayout.EndVertical();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}