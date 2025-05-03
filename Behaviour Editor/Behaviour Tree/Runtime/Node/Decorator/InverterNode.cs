using System;

namespace BehaviourSystem.BT
{
    public class InverterNode : DecoratorNode
    {
        public override string tooltip
        {
            get { return "Inverts the result of the child node (Success to Failure, Failure to Success)"; }
        }

        protected override EStatus OnUpdate()
        {
            switch (child.UpdateNode())
            {
                case EStatus.Failure: return EStatus.Success;

                case EStatus.Success: return EStatus.Failure;
                
                default: return EStatus.Running;
            }
        }
    }
}