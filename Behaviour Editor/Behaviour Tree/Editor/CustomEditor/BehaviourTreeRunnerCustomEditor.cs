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

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            SerializedProperty useUpdateRate = serializedObject.FindProperty("useUpdateRate");
            SerializedProperty updateRate = serializedObject.FindProperty("_updateRate");
            
            useUpdateRate.boolValue = EditorGUILayout.Toggle("Use Update Rate", useUpdateRate.boolValue);

            if (useUpdateRate.boolValue)
            {
                int maxFPS = Application.targetFrameRate > 0 ? Application.targetFrameRate : (int)BehaviourTreeEditor.Settings.runnerMaxUpdateRateLimit;
                updateRate.uintValue = (uint)EditorGUILayout.IntSlider((int)updateRate.uintValue, 1, maxFPS);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.HelpBox($"This node executes with a time interval of {1f / updateRate.uintValue:F3} seconds.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}