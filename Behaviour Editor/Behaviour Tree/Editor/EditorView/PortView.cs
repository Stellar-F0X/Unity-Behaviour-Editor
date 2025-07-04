using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    public class PortView : Port
    {
        public PortView(Direction direction, Capacity capacity) : base(Orientation.Vertical, direction, capacity, typeof(bool))
        {
            EdgeConnectorListener listener = new EdgeConnectorListener();
            base.m_EdgeConnector = new EdgeConnector<Edge>(listener);
            this.AddManipulator(base.m_EdgeConnector);
        }


        public override bool ContainsPoint(Vector2 localPoint)
        {
            Rect rect = new Rect(0, 0, layout.width, layout.height);
            return rect.Contains(localPoint);
        }
    }


    public class EdgeConnectorListener : IEdgeConnectorListener
    {
        public EdgeConnectorListener()
        {
            _pendingEdgesToCreate = new List<Edge>();
            _graphViewChange.edgesToCreate = this._pendingEdgesToCreate;
        }

        private GraphViewChange _graphViewChange;
        private List<Edge> _pendingEdgesToCreate;


        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            if (BehaviourTreeEditor.Instance is null || BehaviourTreeEditor.Instance.View is null || BehaviourTreeEditor.CanEditTree == false)
            {
                return;
            }

            if (edge.input is not null && edge.input.node is NodeView connectionDestination) //Create and link new parent node
            {
                BehaviourTreeEditor.Instance.View.OpenContextualMenuWindow(position, newParentNodeView =>
                {
                    NodeLinkHelper.TryDisconnectChildToParent(connectionDestination);
                    
                    if (NodeLinkHelper.TryConnectNodesByEdge(newParentNodeView, connectionDestination, out _))
                    {
                        BehaviourTreeEditor.Instance.Tree.nodeSet.AddChild(newParentNodeView.targetNode, connectionDestination.targetNode);
                    }
                });
            }
            else if (edge.output is not null && edge.output.node is NodeView connectionSource) //Create and link new child node
            {
                BehaviourTreeEditor.Instance.View.OpenContextualMenuWindow(position, newChildNodeView =>
                {
                    NodeLinkHelper.TryDisconnectParentToChild(connectionSource);
                    
                    if (NodeLinkHelper.TryConnectNodesByEdge(connectionSource, newChildNodeView, out _))
                    {
                        BehaviourTreeEditor.Instance.Tree.nodeSet.AddChild(connectionSource.targetNode, newChildNodeView.targetNode);
                    }
                });
            }
        }


        //Referenced: https://github.com/thekiwicoder0/UnityBehaviourTreeEditor/blob/main/Editor/NodePort.cs
        public void OnDrop(GraphView graphView, Edge edge)
        {
            List<GraphElement> edgesToDelete = ListPool<GraphElement>.Get();

            _pendingEdgesToCreate.Clear();
            _pendingEdgesToCreate.Add(edge);

            if (edge.input.capacity == Port.Capacity.Single)
            {
                this.AddToDeleteList(edgesToDelete, edge.input.connections, edge);
            }

            if (edge.output.capacity == Port.Capacity.Single)
            {
                this.AddToDeleteList(edgesToDelete, edge.output.connections, edge);
            }

            if (edgesToDelete.Count > 0)
            {
                graphView.DeleteElements(edgesToDelete);
            }

            ListPool<GraphElement>.Release(edgesToDelete);

            if (graphView.graphViewChanged is not null)
            {
                _pendingEdgesToCreate = graphView.graphViewChanged.Invoke(_graphViewChange).edgesToCreate;
            }

            foreach (var e in _pendingEdgesToCreate)
            {
                graphView.AddElement(e);
                edge.input.Connect(e);
                edge.output.Connect(e);
            }
        }


        private void AddToDeleteList(List<GraphElement> edgeToDeleteList, IEnumerable<Edge> edges, Edge targetEdge)
        {
            foreach (var edge in edges)
            {
                if (edge != targetEdge)
                {
                    edgeToDeleteList.Add(edge);
                }
            }
        }
    }
}