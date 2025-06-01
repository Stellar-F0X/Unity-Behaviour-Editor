using System.Collections.Generic;
using System.Linq;
using BehaviourSystem.BT;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    public static class NodeLinkHelper
    {
        public static void CreateVisualEdgesFromNodeData(BehaviourTreeView treeView, NodeBase parentNodeBase, IBehaviourIterable childrenNodes)
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

                if (parentView is null || childView is null)
                {
                    continue;
                }

                Edge newEdge = parentView.outputPort.ConnectTo(childView.inputPort);
                newEdge.pickingMode = Application.isPlaying ? PickingMode.Ignore : PickingMode.Position;
                childView.parentConnectionEdge = newEdge;
                treeView.AddElement(newEdge);
            }
        }


        public static void UpdateNodeDataFromVisualEdges(BehaviourNodeSet nodeSet, List<Edge> edges)
        {
            if (edges is null || edges.Count == 0)
            {
                return;
            }

            foreach (Edge edge in edges)
            {
                NodeView parentView = edge.output.node as NodeView;
                NodeView childView = edge.input.node as NodeView;

                if (parentView is null || childView is null)
                {
                    continue;
                }

                nodeSet.AddChild(parentView.targetNode, childView.targetNode);
            }
        }


        public static void RemoveEdgeAndNodeConnection(BehaviourNodeSet nodeSet, Edge edge)
        {
            NodeView parentView = edge.output.node as NodeView;
            NodeView childView = edge.input.node as NodeView;

            if (parentView is null || childView is null)
            {
                return;
            }

            nodeSet.RemoveChild(parentView?.targetNode, childView?.targetNode);
            edge.RemoveFromHierarchy();
        }


        public static void ConnectNodesByEdge(NodeView startNodeView, NodeView destinationNodeView)
        {
            if (startNodeView.inputPort is null || destinationNodeView.outputPort is null)
            {
                return;
            }

            //Edge 연결을 시작한 노드에 이미 부모로 연결된 노드가 있다면 그 노드와 연결을 해제한다. 
            if (startNodeView.inputPort.connected)
            {
                if (startNodeView.parentConnectionEdge.output.node is NodeView view)
                {
                    BehaviourTreeEditor.Instance.Tree.nodeSet.RemoveChild(view.targetNode, startNodeView.targetNode);
                    startNodeView.parentConnectionEdge.RemoveFromHierarchy();
                }
            }

            //Edge 연결을 시작한 부모 노드가 하나의 자식만 가질 수 있으며, 이미 자식으로 연결된 노드가 있다면 그 노드와의 연결을 해제한다.
            if (destinationNodeView.outputPort.connected && destinationNodeView.targetNode.nodeType == NodeBase.ENodeType.Decorator)
            {
                if (destinationNodeView.outputPort.connections.First()?.input.node is NodeView existingChildView)
                {
                    BehaviourTreeEditor.Instance.Tree.nodeSet.RemoveChild(destinationNodeView.targetNode, existingChildView.targetNode);
                    existingChildView.parentConnectionEdge.RemoveFromHierarchy();
                }
            }

            BehaviourTreeEditor.Instance.Tree.nodeSet.AddChild(startNodeView.targetNode, destinationNodeView.targetNode);
            Edge linkedEdge = startNodeView.inputPort.ConnectTo(destinationNodeView.outputPort);
            destinationNodeView.parentConnectionEdge = linkedEdge;
            BehaviourTreeEditor.Instance.View.AddElement(linkedEdge);
        }
    }
}