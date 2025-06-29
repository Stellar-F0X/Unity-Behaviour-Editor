using System.Collections.Generic;
using BehaviourSystem.BT;
using BehaviourSystem.BT.State;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    public class FiniteStateMachineViewProcessor : GraphViewProcessor
    {
        public override bool TryConnectNodesByEdge(NodeView nodeA, NodeView nodeB, out Edge linkedEdge)
        {
            if (nodeA is null || nodeB is null)
            {
                linkedEdge = null;
                return false;
            }
    
            Port outputPort = null;
            Port inputPort = null;
    
            // 포트 방향에 따라 올바른 연결 결정
            if (nodeA.outputPort is not null && nodeB.inputPort is not null)
            {
                outputPort = nodeA.outputPort;
                inputPort = nodeB.inputPort;
            }
            else if (nodeB.outputPort is not null && nodeA.inputPort is not null)
            {
                outputPort = nodeB.outputPort;
                inputPort = nodeA.inputPort;
            }
            else
            {
                linkedEdge = null;
                return false;
            }
            
            TransitionEdge transitionEdge = new TransitionEdge()
            {
                output = outputPort,
                input = inputPort
            };
            
            outputPort.Connect(transitionEdge);
            inputPort.Connect(transitionEdge);
    
            linkedEdge = transitionEdge;
    
            // input 노드에 연결 정보 저장
            if (inputPort.node is NodeView inputNodeView)
            {
                inputNodeView.parentConnectionEdge = linkedEdge;
            }
    
            BehaviorEditor.Instance.view.AddElement(linkedEdge);
            return true;
        }
        

        public override void CreateAndConnectNodes(GraphAsset graphAsset, BehaviourGraphView graphView)
        {
            for (int i = 0; i < graphAsset.graph.nodes.Count; ++i)
            {
                graphView.AddNewNodeView(this.RecreateNodeViewOnLoad(graphAsset.graph.nodes[i]));
            }

            for (int i = 0; i < graphAsset.graph.nodes.Count; ++i)
            {
                if (graphAsset.graph.nodes[i] is not StateNodeBase parentNodeBase)
                {
                    continue;
                }

                foreach (Transition child in parentNodeBase.transitions)
                {
                    NodeView sourceView = graphView.FindNodeView(parentNodeBase);
                    NodeView targetView = graphView.FindNodeView(child.nextStateNodeUguid.ToString());

                    if (TryConnectNodesByEdge(sourceView, targetView, out Edge newEdge))
                    {
                        newEdge.pickingMode = Application.isPlaying ? PickingMode.Ignore : PickingMode.Position;
                    }
                }
            }
        }


        protected override CreationWindowBase CreateGraphNodeCreationWindow()
        {
            return ScriptableObject.CreateInstance<StateCreationWindow>();
        }


        public override void OnDeleteSelectionElements(List<ISelectable> selection)
        {
            for (int i = 0; i < selection.Count; ++i)
            {
                if (selection[i] is NodeView view && view.targetNode is StateNodeBase targetNode)
                {
                    StateNodeBase.EStateNodeType type = targetNode.stateNodeType;

                    bool exclude = false;
                    
                    exclude |= type == StateNodeBase.EStateNodeType.Any;
                    exclude |= type == StateNodeBase.EStateNodeType.Enter;
                    exclude |= type == StateNodeBase.EStateNodeType.Exit;
                    
                    if (exclude)
                    {
                        view.selected = false;
                        selection.RemoveAt(i--);
                    }
                }
            }
        }


        public override void DisconnectNodesByEdge(GraphAsset graphAsset, Edge edge)
        {
            FiniteStateMachine fsm = graphAsset.graph as FiniteStateMachine;

            if (fsm is null)
            {
                return;
            }

            NodeView sourceStateView = edge.output.node as NodeView;
            NodeView targetStateView = edge.input.node as NodeView;

            if (sourceStateView is null || targetStateView is null)
            {
                return;
            }

            fsm.DisconnectStates((StateNodeBase)sourceStateView.targetNode, (StateNodeBase)targetStateView.targetNode);
            edge.RemoveFromHierarchy();
        }

        
        public override void ConnectNodesByEdges(GraphAsset graphAsset, List<Edge> edges)
        {
            FiniteStateMachine fsm = graphAsset.graph as FiniteStateMachine;

            if (fsm is null || edges.Count == 0)
            {
                return;
            }

            foreach (Edge edge in edges)
            {
                NodeView sourceStateView = edge.output.node as NodeView;
                NodeView targetStateView = edge.input.node as NodeView;

                if (sourceStateView is null || targetStateView is null)
                {
                    continue;
                }

                targetStateView.parentConnectionEdge = edge;

                fsm.ConnectStates((StateNodeBase)sourceStateView.targetNode, (StateNodeBase)targetStateView.targetNode);
            }
        }


        public override NodeView RecreateNodeViewOnLoad(NodeBase node)
        {
            if (node is null)
            {
                return null;
            }

            NodeView nodeView = new StateNodeView(node, BehaviorEditor.settings.stateNodeViewXml);

            Debug.Assert(nodeView is not null, $"{nameof(BehaviourGraphView)}: NodeView is null");
            return nodeView;
        }


        //TODO: OnExit나 OnEnter 대상으로만 
        public override void TryDisconnectParentToChild(NodeView parentNodeView) { }


        //TODO: 마찬가지로 OnExit나 OnEnter 대상으로만 
        public override void TryDisconnectChildToParent(NodeView childNodeView) { }
    }
}