using System;
using System.Text.RegularExpressions;
using UnityEditor;
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

            return Regex.Replace(nodeName, "(?<!^)([A-Z])", " $1");
        }
        
        
        public static NodeBase CreateNode(Type nodeType)
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
            
            newNode.guid = GUID.Generate().ToString();
            newNode.hideFlags = HideFlags.HideInHierarchy;
            newNode.name = ApplySpacing(nodeType.Name);
            return newNode;
        }


        public static bool HasChild(this NodeBase targetNode, out int childCount)
        {
            if (targetNode is IBehaviourIterable iterable)
            {
                childCount = iterable.childCount;
                return true;
            }

            childCount = 0;
            return false;
        }
    }
}