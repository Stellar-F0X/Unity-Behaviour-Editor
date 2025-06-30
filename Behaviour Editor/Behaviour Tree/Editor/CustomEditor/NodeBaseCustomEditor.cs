using System;
using System.Linq;
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
            SerializedProperty startProp = serializedObject.FindProperty("_parent");

            if (this.HasRemainingPropertiesAfter(startProp))
            {
                this.DrawHeader(this.target.name, 10f, 2f);
                this.DrawPropertiesRange(startProp, startInclusive: false);
            }
            
            serializedObject.ApplyModifiedProperties();
        }


        protected virtual SerializedProperty DrawBasedSerializedField()
        {
            _headerLabelStyle = new GUIStyle(EditorStyles.toolbar)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                fontSize = 13,
            };

            this.DrawHeader("Information", endSpacing: 2);

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

            return desProp;
        }


        protected virtual void DrawHeader(string header, float startSpacing = 0f, float endSpacing = 0f)
        {
            if (Mathf.Approximately(startSpacing, 0f) == false)
            {
                EditorGUILayout.Space(startSpacing);
            }

            using (new GUIColorScope(new Color32(255, 255, 255, 255), GUIColorScope.EGUIColorScope.Background))
            {
                EditorGUILayout.LabelField(header, _headerLabelStyle);
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
                }
                
                started = true;
            }
            while (start.NextVisible(false));
        }


        protected bool HasRemainingPropertiesAfter(SerializedProperty startProperty)
        {
            if (startProperty == null)
            {
                return false;
            }

            SerializedProperty iterator = startProperty.Copy();

            int propertyCount = 0;

            while (iterator.NextVisible(false))
            {
                propertyCount++;
            }

            return propertyCount > 0;
        }
    }
}