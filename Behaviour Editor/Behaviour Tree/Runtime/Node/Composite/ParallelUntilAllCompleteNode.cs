namespace BehaviourSystem.BT
{
    public class ParallelUntilAllCompleteNode : ParallelNodeBase
    {
        public override string tooltip
        {
            get
            {
                return "Continuously executes all children until each has completed at least once.\n" +
                       "Already completed nodes will continue to re-run until this condition is met.";
            }
        }
        

        protected override EBehaviourResult OnUpdate()
        {
            return this.EvaluatePolicy();
        }
        

        protected override EBehaviourResult EvaluatePolicy()
        {
            bool allCompletedOnce = true;

            int count = children.Count;

            for (int i = 0; i < count; i++)
            {
                EBehaviourResult result = children[i].UpdateNode();

                if (result != EBehaviourResult.Running)
                {
                    _isChildStopped[i] = true;
                }

                if (_isChildStopped[i] == false)
                {
                    allCompletedOnce = false;
                }
            }

            return allCompletedOnce ? EBehaviourResult.Success : EBehaviourResult.Running;
        }
    }
}