using BehaviourSystem.BT;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    [CustomEditor(typeof(BehaviourTreeRunner))]
    public class BehaviourTreeRunnerCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SerializedProperty treeAsset = serializedObject.FindProperty("_runtimeTree");
            SerializedProperty useFixedUpdate = serializedObject.FindProperty("useFixedUpdate");
            SerializedProperty useGizmos = serializedObject.FindProperty("useGizmos");

            treeAsset.objectReferenceValue = EditorGUILayout.ObjectField("Tree Asset", treeAsset.objectReferenceValue, typeof(BehaviourTree), false);
            useFixedUpdate.boolValue = EditorGUILayout.Toggle("Use Fixed Update", useFixedUpdate.boolValue);
            useGizmos.boolValue = EditorGUILayout.Toggle("Use Gizmos Update", useGizmos.boolValue);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}