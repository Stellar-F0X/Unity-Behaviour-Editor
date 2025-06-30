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
            this.DrawBasedSerializedField();

            this.DrawHeader(10f, 2f);

            this.DrawPropertiesRange(serializedObject.FindProperty("_parent"), startInclusive: false);
        }


        protected virtual void DrawBasedSerializedField()
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


        protected virtual void DrawHeader(float startSpacing = 0f, float endSpacing = 0f)
        {
            if (Mathf.Approximately(startSpacing, 0f) == false)
            {
                EditorGUILayout.Space(startSpacing);
            }

            using (new GUIColorScope(new Color32(255, 255, 255, 255), GUIColorScope.EGUIColorScope.Background))
            {
                EditorGUILayout.LabelField(this.target.name, _headerLabelStyle);
            }

            if (Mathf.Approximately(endSpacing, 0f) == false)
            {
                EditorGUILayout.Space(endSpacing);
            }
        }

        protected virtual void DrawPropertiesRange(SerializedProperty start, SerializedProperty stop = null, bool includeChildren = true, bool startInclusive = true)
        {
            bool started = false;
            
            do
            {
                if (stop != null && SerializedProperty.EqualContents(start, stop))
                {
                    break;
                }

                if (started || startInclusive)
                {
                    EditorGUILayout.PropertyField(start, includeChildren);
                    started = true;
                }
            }
            while (start.NextVisible(false));

            serializedObject.ApplyModifiedProperties();
        }
    }
}