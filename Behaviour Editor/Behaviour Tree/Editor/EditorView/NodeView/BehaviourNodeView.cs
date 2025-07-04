using System;
using BehaviourSystem.BT;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    public class BehaviourNodeView : NodeView
    {
        public BehaviourNodeView(NodeBase targetNode, VisualTreeAsset nodeUxml) : base(targetNode, nodeUxml) { }


        protected override void Initialize()
        {
            _elementGroup.AddToClassList($"behaviour-node-{((BehaviorNodeBase)targetNode).nodeType}");
            base.Initialize();
        }


        public void SortChildren()
        {
            if (((BehaviorNodeBase)targetNode).nodeType != BehaviorNodeBase.EBehaviourNodeType.Composite)
            {
                return;
            }

            if (targetNode is CompositeNode compositeNode)
            {
                compositeNode.children.Sort((l, r) => l.position.x < r.position.x ? -1 : 1);
            }
        }


        public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
        {
            return new PortView(EGraphType.BT, direction, capacity);
        }


        protected override void CreatePorts()
        {
            switch (((BehaviorNodeBase)targetNode).nodeType)
            {
                case BehaviorNodeBase.EBehaviourNodeType.Root:
                {
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    break;
                }

                case BehaviorNodeBase.EBehaviourNodeType.Action:
                {
                    inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    break;
                }

                case BehaviorNodeBase.EBehaviourNodeType.SubGraph:
                {
                    inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    break;
                }

                case BehaviorNodeBase.EBehaviourNodeType.Composite:
                {
                    inputPort =  this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    break;
                }

                case BehaviorNodeBase.EBehaviourNodeType.Decorator:
                {
                    inputPort =  this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    break;
                }
            }

            this.SetupPort(inputPort, string.Empty, FlexDirection.Column, base.inputContainer);
            this.SetupPort(outputPort, string.Empty, FlexDirection.ColumnReverse, base.outputContainer);
        }


        protected override void SetBorderColorByStatus()
        {
            switch (((BehaviorNodeBase)targetNode).status)
            {
                case EStatus.Failure: base.SetBorderColor(_nodeBorder.style, BehaviorEditor.settings.nodeFailureColor); break;

                case EStatus.Success: base.SetBorderColor(_nodeBorder.style, BehaviorEditor.settings.nodeSuccessColor); break;
            }
        }
    }
}