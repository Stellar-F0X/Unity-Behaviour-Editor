using UnityEditor;

namespace BehaviourSystemEditor.BT
{
    [CustomEditor(typeof(BehaviourTreeEditorSettings))]
    public class BehaviourTreeEditorSettingsCustomInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            SerializedProperty iterator = serializedObject.FindProperty("m_Script");
            
            while (iterator.NextVisible(false))
            {
                EditorGUILayout.PropertyField(iterator);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}