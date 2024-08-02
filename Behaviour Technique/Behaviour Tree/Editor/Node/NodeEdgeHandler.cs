using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public class NodeEdgeHandler
    {
        public void ConnectEdges(BehaviourTreeView treeView, Node parentNode, List<Node> childrenNodes)
        {
            foreach (var child in childrenNodes)
            {
                NodeView parentView = treeView.FindNodeView(parentNode);
                NodeView childView = treeView.FindNodeView(child);

                if (parentView == null || childView == null)
                {
                    continue;
                }

                treeView.AddElement(parentView.output.ConnectTo(childView.input));
            }
        }


        public void ConnectEdges(BehaviourTree tree, List<Edge> edges)
        {
            foreach (var edge in edges)
            {
                NodeView parentView = edge.output.node as NodeView;
                NodeView childView = edge.input.node as NodeView;

                if (parentView == null || childView == null)
                {
                    continue;
                }
                
                tree.AddChild(parentView?.node, childView?.node);
            }
        }


        public void DeleteEdges(BehaviourTree tree, Edge edge)
        {
            NodeView parentView = edge.output.node as NodeView;
            NodeView childView = edge.input.node as NodeView;
            
            if (parentView == null || childView == null)
            {
                return;
            }
            
            tree.RemoveChild(parentView?.node, childView?.node);
        }
    }
}
