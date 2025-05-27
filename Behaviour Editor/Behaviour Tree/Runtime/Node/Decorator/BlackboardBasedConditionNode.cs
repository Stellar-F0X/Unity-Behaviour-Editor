using System;

namespace BehaviourSystem.BT
{
    [Serializable]
    public sealed class BlackboardBasedConditionNode : ConditionNode
    {
        public override string tooltip
        {
            get { return "Provides access to the blackboard variables and data"; }
        }


        protected override EBehaviourResult OnUpdate()
        {
            if (conditions != null && this.CheckCondition())
            {
                return child.UpdateNode();
            }
            else
            {
                return EBehaviourResult.Failure;
            }
        }
    }
}