namespace BehaviourSystem.BT
{
    public class BlackboardBasedUntilForNode : ConditionNodeBase
    {
        public override string tooltip
        {
            get { return "Keeps executing the child node until all blackboard conditions are satisfied."; }
        }


        protected override EStatus OnUpdate()
        {
            if (conditions != null && this.CheckCondition())
            {
                return EStatus.Success;
            }
            else
            {
                child.UpdateNode();
                return EStatus.Running;
            }
        }
    }
}