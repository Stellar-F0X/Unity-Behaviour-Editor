using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public static class NodeFactory
    {
        public static string ApplySpacing(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName))
            {
                throw new ArgumentException($"{typeof(NodeFactory)}: NodeName is null or empty");
            }

            if (nodeName.EndsWith("Node"))
            {
                nodeName = nodeName.Replace("Node", string.Empty);
            }
            else if (nodeName.EndsWith("State"))
            {
                nodeName = nodeName.Replace("State", string.Empty);
            }

            return Regex.Replace(nodeName, "(?<=[a-z])(?=[A-Z])", " ");
        }
        
        
        public static NodeBase CreateNode(Type nodeType, Vector2Int position = default)
        {
            if (typeof(NodeBase).IsAssignableFrom(nodeType) == false)
            {
                throw new ArgumentException($"{typeof(NodeFactory)}: NodeType is not NodeBase");
            }
            
            NodeBase newNode = ScriptableObject.CreateInstance(nodeType) as NodeBase;

            if (newNode is null)
            {
                throw new Exception($"{typeof(NodeFactory)}: Failed to create node of type {nodeType}");
            }
            
            newNode.guid = UGUID.Create();
            newNode.hideFlags = HideFlags.HideInHierarchy;
            newNode.name = ApplySpacing(nodeType.Name);
            newNode.position = position;
            return newNode;
        }
    }
}