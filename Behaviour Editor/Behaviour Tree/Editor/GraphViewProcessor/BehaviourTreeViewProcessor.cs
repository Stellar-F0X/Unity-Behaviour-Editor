using System.Collections.Generic;
using System.Linq;
using BehaviourSystem.BT;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    public class BehaviourTreeViewProcessor : GraphViewProcessor
    {
        public override bool TryConnectNodesByEdge(NodeView connectionSource, NodeView connectionTarget, out Edge linkedEdge)
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

        
        public override void CreateAndConnectNodes(GraphAsset graphAsset)
        {
            for (int i = 0; i < graphAsset.graph.nodes.Count; ++i)
            {
                this.RecreateNodeViewOnLoad(graphAsset.graph.nodes[i]);
            }

            for (int i = 0; i < graphAsset.graph.nodes.Count; ++i)
            {
                NodeBase parentNodeBase = graphAsset.graph.nodes[i];
                IBehaviourIterable iterable = parentNodeBase as IBehaviourIterable;

                if (iterable is null || iterable.childCount == 0)
                {
                    break;
                }

                foreach (NodeBase child in iterable.GetChildren())
                {
                    NodeView parentView = graphView.FindNodeView(parentNodeBase);
                    NodeView childView = graphView.FindNodeView(child);

                    if (this.TryConnectNodesByEdge(parentView, childView, out Edge newEdge))
                    {
                        newEdge.pickingMode = Application.isPlaying ? PickingMode.Ignore : PickingMode.Position;
                    }
                }
            }
        }


        public override void OnDeleteSelectionElements(List<ISelectable> selection)
        {
            if (selection is null || selection.Count == 0)
            {
                return;
            }

            for (int i = 0; i < selection.Count; ++i)
            {
                if (selection[i] is not NodeView view)
                {
                    continue;
                }

                BehaviourNodeBase node = (BehaviourNodeBase)view.targetNode;

                if (node.nodeType == BehaviourNodeBase.EBehaviourNodeType.Root)
                {
                    view.selected = false;
                    selection.RemoveAt(i);
                    break;
                }
            }
        }


        public override void NotifyNodePositionChanged(List<GraphElement> elements)
        {
            if (elements is null || elements.Count == 0)
            {
                return;
            }

            foreach (var nodeElement in graphView.nodes)
            {
                if (nodeElement is BehaviourNodeView view)
                {
                    view.SortChildren();
                }
            }
        }


        /// <summary>로딩 시 노드 데이터로부터 NodeView를 재생성합니다.</summary>
        public override NodeView RecreateNodeViewOnLoad(NodeBase node)
        {
            if (node is null)
            {
                return null;
            }

            NodeView nodeView = new BehaviourNodeView(node, BehaviourSystemEditor.Settings.behaviourNodeViewXml);

            Debug.Assert(nodeView is not null, $"{nameof(BehaviourGraphView)}: NodeView is null");

            nodeView.OnNodeSelected += this.graphView.onNodeSelected;

            this.graphView.AddElement(nodeView); //nodes라는 GraphElement 컨테이너에 추가.
            return nodeView;
        }


        /// <summary> 자식 노드에서 부모 노드와의 연결을 끊는다. </summary>
        public override void TryDisconnectChildToParent(NodeView childNodeView)
        {
            if (childNodeView.inputPort is null || childNodeView.inputPort.connected == false)
            {
                return;
            }

            if (childNodeView.parentConnectionEdge?.output.node is NodeView view)
            {
                BehaviourTree tree = BehaviourSystemEditor.Instance.Tree.graph as BehaviourTree;
                tree?.RemoveChild((BehaviourNodeBase)view.targetNode, (BehaviourNodeBase)childNodeView.targetNode);
                view.outputPort.Disconnect(childNodeView.parentConnectionEdge);

                List<GraphElement> edges = ListPool<GraphElement>.Get();
                edges.Add(childNodeView.parentConnectionEdge);
                BehaviourSystemEditor.Instance.View.DeleteElements(edges);
                ListPool<GraphElement>.Release(edges);
            }
        }

        public override void DisconnectNodesByEdge(GraphAsset graphAsset, Edge edge)
        {
            BehaviourTree tree = graphAsset.graph as BehaviourTree;

            if (tree is null)
            {
                return;
            }

            NodeView parentView = edge.output.node as NodeView;
            NodeView childView = edge.input.node as NodeView;

            if (parentView is null || childView is null)
            {
                return;
            }

            tree.RemoveChild((BehaviourNodeBase)parentView.targetNode, (BehaviourNodeBase)childView.targetNode);
            edge.RemoveFromHierarchy();
        }


        public override void ConnectNodesByEdges(GraphAsset graphAsset, List<Edge> edges)
        {
            BehaviourTree tree = graphAsset.graph as BehaviourTree;

            if (tree is null || edges.Count == 0)
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

                tree.AddChild((BehaviourNodeBase)parentView.targetNode, (BehaviourNodeBase)childView.targetNode);
            }
        }


        /// <summary> 부모 노드에서 자식 노드와의 연결을 끊는다. </summary>
        public override void TryDisconnectParentToChild(NodeView parentNodeView)
        {
            if (parentNodeView.outputPort is null || parentNodeView.outputPort.connected == false)
            {
                return;
            }

            BehaviourNodeBase node = (BehaviourNodeBase)parentNodeView.targetNode;

            bool isSingleChildNode = false;
            
            isSingleChildNode |= node.nodeType is BehaviourNodeBase.EBehaviourNodeType.Decorator;
            isSingleChildNode |= node.nodeType is BehaviourNodeBase.EBehaviourNodeType.Root;

            //부모 노드에서 Edge 연결을 시작할 경우로, 부모 노드가 하나의 자식만 가질 수 있으며, 이미 자식으로 연결된 노드가 있다면 그 노드와의 연결을 해제한다.
            if (isSingleChildNode && parentNodeView.outputPort.connections.First()?.input.node is NodeView existingChildView)
            {
                BehaviourTree tree = BehaviourSystemEditor.Instance.Tree.graph as BehaviourTree;
                tree?.RemoveChild((BehaviourNodeBase)parentNodeView.targetNode, (BehaviourNodeBase)existingChildView.targetNode);

                parentNodeView.outputPort.Disconnect(existingChildView.parentConnectionEdge);

                List<GraphElement> edges = ListPool<GraphElement>.Get();
                edges.Add(existingChildView.parentConnectionEdge);
                BehaviourSystemEditor.Instance.View.DeleteElements(edges);
                ListPool<GraphElement>.Release(edges);
            }
        }
    }
}