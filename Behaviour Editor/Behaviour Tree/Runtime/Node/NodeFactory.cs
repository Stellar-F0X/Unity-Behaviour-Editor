using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace BehaviourSystem.BT
{
    public static class NodeFactory
    {
        public static void EditNodeName(NodeBase targetNode, string newName, bool automaticSpacing = true)
        {
            if (string.IsNullOrEmpty(newName))
            {
                throw new ArgumentException($"{typeof(NodeFactory)}: NodeName is null or empty");
            }

            if (automaticSpacing)
            {
                targetNode.name = Regex.Replace(newName, "(?<!^)([A-Z])", " $1");
            }
            else
            {
                targetNode.name = newName;
            }
        }
        
        
        public static NodeBase CreateNode(Type nodeType)
        {
            if (typeof(NodeBase).IsAssignableFrom(nodeType))
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
            NodeFactory.EditNodeName(newNode, newNode.name.Replace("Node", string.Empty));
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