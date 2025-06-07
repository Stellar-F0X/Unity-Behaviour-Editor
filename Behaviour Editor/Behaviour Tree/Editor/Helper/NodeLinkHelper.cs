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
        public static void CreateVisualEdgesFromNodeData(BehaviourTreeView treeView, NodeBase parentNodeBase, IBehaviourIterable iterable)
        {
            if (iterable is null || iterable.childCount == 0)
            {
                return;
            }

            int count = iterable.childCount;
            List<NodeBase> children = iterable.GetChildren();

            for (int i = 0; i < count; ++i)
            {
                NodeView parentView = treeView.FindNodeView(parentNodeBase);
                NodeView childView = treeView.FindNodeView(children[i]);

                if (TryConnectNodesByEdge(parentView, childView, out Edge newEdge))
                {
                    if (Application.isPlaying)
                    {
                        newEdge.pickingMode = PickingMode.Ignore;
                    }
                    else
                    {
                        newEdge.pickingMode = PickingMode.Position;
                    }
                }
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

                childView.parentConnectionEdge = edge;
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

            nodeSet.RemoveChild(parentView.targetNode, childView.targetNode);
            edge.RemoveFromHierarchy();
        }


        /// <summary> 자식 노드에서 부모 노드와의 연결을 끊는다. </summary>
        public static void TryDisconnectChildToParent(NodeView childNodeView)
        {
            if (childNodeView.inputPort is null || childNodeView.inputPort.connected == false)
            {
                return;
            }

            if (childNodeView.parentConnectionEdge?.output.node is NodeView view)
            {
                BehaviourTreeEditor.Instance.Tree.nodeSet.RemoveChild(view.targetNode, childNodeView.targetNode);
                view.outputPort.Disconnect(childNodeView.parentConnectionEdge);
                childNodeView.parentConnectionEdge.RemoveFromHierarchy();
            }
        }


        /// <summary> 부모 노드에서 자식 노드와의 연결을 끊는다. </summary>
        public static void TryDisconnectParentToChild(NodeView parentNodeView)
        {
            if (parentNodeView.outputPort is null || parentNodeView.outputPort.connected == false)
            {
                return;
            }

            bool isSingleChildNode = parentNodeView.targetNode.nodeType is NodeBase.ENodeType.Decorator or NodeBase.ENodeType.Root;
            
            //부모 노드에서 Edge 연결을 시작할 경우로, 부모 노드가 하나의 자식만 가질 수 있으며, 이미 자식으로 연결된 노드가 있다면 그 노드와의 연결을 해제한다.
            if (isSingleChildNode && parentNodeView.outputPort.connections.First()?.input.node is NodeView existingChildView)
            {
                BehaviourTreeEditor.Instance.Tree.nodeSet.RemoveChild(parentNodeView.targetNode, existingChildView.targetNode);
                parentNodeView.outputPort.Disconnect(existingChildView.parentConnectionEdge);
                existingChildView.parentConnectionEdge.RemoveFromHierarchy();
            }
        }


        public static bool TryConnectNodesByEdge(NodeView connectionSource, NodeView connectionTarget, out Edge linkedEdge)
        {
            if (connectionSource is null || connectionTarget is null || connectionSource.outputPort is null || connectionTarget.inputPort is null)
            {
                linkedEdge = null;
                return false;
            }
            
            linkedEdge = connectionSource.outputPort.ConnectTo(connectionTarget.inputPort);
            connectionTarget.parentConnectionEdge = linkedEdge;
            BehaviourTreeEditor.Instance.View.AddElement(linkedEdge);
            return true;
        }
    }
}