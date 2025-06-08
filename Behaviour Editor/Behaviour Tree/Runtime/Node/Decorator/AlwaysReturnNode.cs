namespace BehaviourSystem.BT
{
    public class AlwaysReturnNode : DecoratorNode
    {
        public bool forceResultOnRunning;
        public EBehaviourResult result;

        protected override EBehaviourResult OnUpdate()
        {
            EBehaviourResult currentResult = child.UpdateNode();

            if (forceResultOnRunning)
            {
                return result;
            }

            if (currentResult != EBehaviourResult.Running)
            {
                return result;
            }
            else
            {
                return EBehaviourResult.Running;
            }
        }
    }
}