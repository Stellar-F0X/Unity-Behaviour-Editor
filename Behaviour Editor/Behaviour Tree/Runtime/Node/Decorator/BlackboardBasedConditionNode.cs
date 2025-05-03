using System;

namespace BehaviourSystem.BT
{
    [Serializable]
    public sealed class BlackboardBasedConditionNode : ConditionNodeBase
    {
        public override string tooltip
        {
            get 
            {
                return "You can set conditions based on blackboard data." +
                         "\nSuccessful: it executes its child." +
                         "\nFailed: it stops all children that were running under this node with Failure."; 
            }
        }



        protected override EStatus OnUpdate()
        {
            if (conditions != null && this.CheckCondition())
            {
                return child.UpdateNode();
            }
            else
            {
                return EStatus.Failure;
            }
        }
    }
}