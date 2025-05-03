namespace BehaviourSystem.BT
{
    public class AlwaysReturnNode : DecoratorNode
    {
        public bool forceResultOnRunning;
        public EStatus result;

        protected override EStatus OnUpdate()
        {
            EStatus currentResult = child.UpdateNode();

            if (forceResultOnRunning)
            {
                return result;
            }

            if (currentResult != EStatus.Running)
            {
                return result;
            }
            else
            {
                return EStatus.Running;
            }
        }
    }
}