using System;
using System.Collections.Generic;
using BehaviourSystem.BT;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    public class NodeEdgeHandler
    {
        public void ConnectEdges(BehaviourTreeView treeView, NodeBase parentNodeBase, IBehaviourIterable childrenNodes)
        {
            if (childrenNodes is null || childrenNodes.childCount == 0)
            {
                return;
            }

            List<NodeBase> children = childrenNodes.GetChildren();
            int count = children.Count;

            for (int i = 0; i < count; ++i)
            {
                NodeView parentView = treeView.FindNodeView(parentNodeBase);
                NodeView childView = treeView.FindNodeView(children[i]);

                if (parentView == null || childView == null)
                {
                    continue;
                }

                Edge newEdge = parentView.outputPort.ConnectTo(childView.inputPort);
                newEdge.pickingMode = Application.isPlaying ? PickingMode.Ignore : PickingMode.Position;
                childView.parentConnectionEdge = newEdge;
                treeView.AddElement(newEdge);
            }
        }


        public void ConnectEdges(BehaviourTree tree, List<Edge> edges)
        {
            if (edges is null || edges.Count == 0)
            {
                return;
            }

            foreach (var edge in edges)
            {
                NodeView parentView = edge.output.node as NodeView;
                NodeView childView = edge.input.node as NodeView;

                if (parentView == null || childView == null)
                {
                    continue;
                }

                tree.nodeSet.AddChild(parentView.targetNode, childView.targetNode);
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

            tree.nodeSet.RemoveChild(parentView?.targetNode, childView?.targetNode);
            edge.RemoveFromHierarchy();
        }
    }
}