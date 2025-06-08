using System;

namespace BehaviourSystem.BT
{
    public class InverterNode : DecoratorNode
    {
        public override string tooltip
        {
            get { return "Inverts the result of the child node (Success to Failure, Failure to Success)"; }
        }

        protected override EBehaviourResult OnUpdate()
        {
            switch (child.UpdateNode())
            {
                case EBehaviourResult.Failure: return EBehaviourResult.Success;

                case EBehaviourResult.Success: return EBehaviourResult.Failure;
                
                default: return EBehaviourResult.Running;
            }
        }
    }
}