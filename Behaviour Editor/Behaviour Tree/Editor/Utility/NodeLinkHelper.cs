using System.Collections.Generic;
using System.Linq;
using BehaviourSystem.BT;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    public static class NodeLinkHelper
    {
        public static void CreateVisualEdgesFromNodeData(BehaviourGraphView treeView, NodeBase parentNodeBase)
        {
            IBehaviourIterable iterable = parentNodeBase as IBehaviourIterable;
            
            if (iterable is null || iterable.childCount == 0)
            {
                return;
            }

            foreach (NodeBase child in iterable.GetChildren())
            {
                NodeView parentView = treeView.FindNodeView(parentNodeBase);
                NodeView childView = treeView.FindNodeView(child);

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


        public static void UpdateNodeDataFromVisualEdges(BehaviourTree tree, List<Edge> edges)
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
                tree.AddChild(parentView.targetNode, childView.targetNode);
            }
        }


        public static void RemoveEdgeAndDisconnection(BehaviourTree tree, Edge edge)
        {
            NodeView parentView = edge.output.node as NodeView;
            NodeView childView = edge.input.node as NodeView;

            if (parentView is null || childView is null)
            {
                return;
            }

            tree.RemoveChild(parentView.targetNode, childView.targetNode);
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
                BehaviourTree tree = BehaviourSystemEditor.Instance.Tree.graph as BehaviourTree;
                tree?.RemoveChild(view.targetNode, childNodeView.targetNode);
                view.outputPort.Disconnect(childNodeView.parentConnectionEdge);
                
                List<GraphElement> edges = ListPool<GraphElement>.Get();
                edges.Add(childNodeView.parentConnectionEdge);
                BehaviourSystemEditor.Instance.View.DeleteElements(edges);
                ListPool<GraphElement>.Release(edges);
            }
        }


        /// <summary> 부모 노드에서 자식 노드와의 연결을 끊는다. </summary>
        public static void TryDisconnectParentToChild(NodeView parentNodeView)
        {
            if (parentNodeView.outputPort is null || parentNodeView.outputPort.connected == false)
            {
                return;
            }

            bool isSingleChildNode = parentNodeView.targetNode.nodeType is BehaviourNodeBase.ENodeType.Decorator or BehaviourNodeBase.ENodeType.Root;
            
            //부모 노드에서 Edge 연결을 시작할 경우로, 부모 노드가 하나의 자식만 가질 수 있으며, 이미 자식으로 연결된 노드가 있다면 그 노드와의 연결을 해제한다.
            if (isSingleChildNode && parentNodeView.outputPort.connections.First()?.input.node is NodeView existingChildView)
            {
                BehaviourTree tree = BehaviourSystemEditor.Instance.Tree.graph as BehaviourTree;
                tree?.RemoveChild(parentNodeView.targetNode, existingChildView.targetNode);
                
                parentNodeView.outputPort.Disconnect(existingChildView.parentConnectionEdge);
                
                List<GraphElement> edges = ListPool<GraphElement>.Get();
                edges.Add(existingChildView.parentConnectionEdge);
                BehaviourSystemEditor.Instance.View.DeleteElements(edges);
                ListPool<GraphElement>.Release(edges);
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
            BehaviourSystemEditor.Instance.View.AddElement(linkedEdge);
            return true;
        }
    }
}