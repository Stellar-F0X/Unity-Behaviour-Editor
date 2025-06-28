using System;
using BehaviourSystem.BT;
using BehaviourSystem.BT.State;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    public class StateNodeView : NodeView
    {
        public StateNodeView(NodeBase targetNode, VisualTreeAsset nodeUxml) : base(targetNode, nodeUxml) { }


        protected override void Initialize()
        {
            _elementGroup.AddToClassList($"state-node-{((StateNodeBase)targetNode).stateNodeType}");
            base.Initialize();
        }

        
        public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
        {
            return new PortView(EGraphType.StateMachine, direction, capacity);
        }
        

        protected override void CreatePorts()
        {
            switch (((StateNodeBase)targetNode).stateNodeType)
            {
                case StateNodeBase.EStateNodeType.Enter:
                {
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    break;
                }
                
                case StateNodeBase.EStateNodeType.Exit:
                {
                    inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
                    break;
                }
                
                case StateNodeBase.EStateNodeType.Any:
                {
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    break;
                }
                
                case StateNodeBase.EStateNodeType.User:
                {
                    inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    break;
                }
            }
            
            this.SetupPort(inputPort, string.Empty, FlexDirection.Column, base.inputContainer);
            this.SetupPort(outputPort, string.Empty, FlexDirection.ColumnReverse, base.outputContainer);
        }
    }
}