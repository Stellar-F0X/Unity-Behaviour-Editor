using BehaviourSystem.BT;
using UnityEditor;

namespace BehaviourSystemEditor.BT
{
    [CustomEditor(typeof(BehaviourTreeRunner))]
    public class BehaviourTreeRunnerCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SerializedProperty treeAsset = serializedObject.FindProperty("_runtimeTree");
            SerializedProperty useGizmos = serializedObject.FindProperty("useGizmos");
            SerializedProperty useFixedUpdate = serializedObject.FindProperty("useFixedUpdate");
            SerializedProperty tickUpdateMode = serializedObject.FindProperty("tickUpdateMode");
            
            treeAsset.objectReferenceValue = EditorGUILayout.ObjectField("Tree Asset", treeAsset.objectReferenceValue, typeof(BehaviourTree), false);
            
            tickUpdateMode.enumValueIndex = EditorGUILayout.Popup("Tick Update Mode", tickUpdateMode.enumValueIndex, tickUpdateMode.enumDisplayNames);
            useFixedUpdate.boolValue = EditorGUILayout.Toggle("Use Fixed Update", useFixedUpdate.boolValue);
            useGizmos.boolValue = EditorGUILayout.Toggle("Use Gizmos Update", useGizmos.boolValue);
            
            serializedObject.ApplyModifiedProperties();

            if (treeAsset.objectReferenceValue is BehaviourTree convertedTreeAsset)
            {
                int nodeCount = convertedTreeAsset.nodeSet?.nodeList?.Count ?? 0;
                int propertyCount = convertedTreeAsset.blackboard?.properties?.Count ?? 0;
                
                EditorGUILayout.HelpBox($"Total Behaviour Nodes: {nodeCount} \nBlackboard Variables: {propertyCount}", MessageType.Info);
            }
        }
    }
}