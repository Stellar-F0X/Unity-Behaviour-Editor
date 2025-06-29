using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public abstract class Graph : ScriptableObject, IDisposable
    {
        [HideInInspector]
        public NodeBase entry; //root node Or default node

        [HideInInspector]
        public List<NodeBase> nodes = new List<NodeBase>();
        
        
        public abstract Graph CloneGraph(Blackboard clonedBlackboard);

        
        public abstract EStatus UpdateGraph();
        
        
        public abstract void ResetGraph();


        public abstract void StopGraph();


        public bool TryGetNodeByGuid(UGUID uguid, out NodeBase node)
        {
            for (int i = 0; i < nodes.Count; ++i)
            {
                if (nodes[i].guid == uguid)
                {
                    node = nodes[i];
                    return true;
                }
            }

            node = null;
            return false;
        }
        
        
        public void Dispose()
        {
            nodes.Clear();
            nodes = null;
            entry = null;
        }
        
        
#if UNITY_EDITOR
        public NodeBase CreateNode(Type nodeType, Vector2Int position = default)
        {
            NodeBase node = NodeFactory.CreateNode(nodeType, position);

            if (node is null)
            {
                throw new Exception("Node is null");
            }

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(this, "Behaviour Tree (CreateNode)");
            }

            nodes.Add(node);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");
                AssetDatabase.AddObjectToAsset(node, this);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }

            return node;
        }

        
        public void DeleteNode(NodeBase node)
        {
            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(this, "Behaviour Tree (DeleteNode)");
            }

            nodes.Remove(node);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.DestroyObjectImmediate(node);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }
#endif
    }
}