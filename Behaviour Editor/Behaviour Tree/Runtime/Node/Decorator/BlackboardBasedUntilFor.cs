using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public class BlackboardBasedUntilFor : ConditionNode
    {
        public override string tooltip
        {
            get { return "Keeps executing the child node until all blackboard conditions are satisfied."; }
        }


        protected override EBehaviourResult OnUpdate()
        {
            if (conditions != null && this.CheckCondition())
            {
                return EBehaviourResult.Success;
            }
            else
            {
                child.UpdateNode();
                return EBehaviourResult.Running;
            }
        }
    }
}