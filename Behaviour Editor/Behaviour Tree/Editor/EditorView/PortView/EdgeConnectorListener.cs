using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Pool;

namespace BehaviourSystemEditor.BT
{
    public abstract class EdgeConnectorListener : IEdgeConnectorListener
    {
        public EdgeConnectorListener()
        {
            _pendingEdgesToCreate = new List<Edge>();
            _graphViewChange.edgesToCreate = this._pendingEdgesToCreate;
        }

        private GraphViewChange _graphViewChange;
        private List<Edge> _pendingEdgesToCreate;


        public virtual void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            edge.isGhostEdge = false;
            
            if (BehaviourSystemEditor.Instance?.View is null || BehaviourSystemEditor.CanEditGraph == false)
            {
                return;
            }

            if (edge.input is not null && edge.input.node is NodeView connectionDestination) //Create and link new parent node
            {
                BehaviourSystemEditor.Instance.View.OpenContextualMenuWindow(position, newParentNodeView => 
                {
                    this.CreateAndLinkFromNewToOriginalNode(newParentNodeView, connectionDestination, position);
                });
            }
            else if (edge.output is not null && edge.output.node is NodeView connectionSource) //Create and link new child node
            {
                BehaviourSystemEditor.Instance.View.OpenContextualMenuWindow(position, newChildNodeView =>
                {
                    this.CreateAndLinkFromOriginalToNewNode(connectionSource, newChildNodeView, position);
                });
            }
        }


        // Referenced: https://github.com/thekiwicoder0/UnityBehaviourTreeEditor/blob/main/Editor/NodePort.cs
        // Copyright (c) 2021 Original Author
        // Licensed under the MIT License. See LICENSE file in the root for details.
        public virtual void OnDrop(GraphView graphView, Edge edge)
        {
            List<GraphElement> edgesToDelete = ListPool<GraphElement>.Get();

            edge.isGhostEdge = false;
            
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


        protected virtual void AddToDeleteList(List<GraphElement> edgeToDeleteList, IEnumerable<Edge> edges, Edge targetEdge)
        {
            foreach (var edge in edges)
            {
                if (edge != targetEdge)
                {
                    edgeToDeleteList.Add(edge);
                }
            }
        }


        // BT: 기존 노드에서 새 노드로 부모-자식 관계를 생성 및 연결합니다.
        // FSM: 기존 노드에서 새 노드로 상태 전이 관계를 생성 및 연결합니다.
        // BT: Creates and links a parent-child relationship from the original node to the new node.
        // FSM: Creates and links a state transition relationship from the original node to the new node.
        protected abstract void CreateAndLinkFromOriginalToNewNode(NodeView sourceNodeView, NodeView targetNodeView, Vector2 position);

        
        // BT: 새 노드에서 기존 노드로 부모-자식 관계를 생성 및 연결합니다.
        // FSM: 새 노드에서 기존 노드로 상태 전이 관계를 생성 및 연결합니다.
        // BT: Creates and links a parent-child relationship from the new node to the original node.
        // FSM: Creates and links a state transition relationship from the new node to the original node.
        protected abstract void CreateAndLinkFromNewToOriginalNode(NodeView sourceNodeView, NodeView targetNodeView, Vector2 position);
    }
}