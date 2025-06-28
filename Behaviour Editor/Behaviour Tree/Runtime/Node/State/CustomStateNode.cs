namespace BehaviourSystem.BT.State
{
    public abstract class CustomStateNode : StateNodeBase
    {
        public override EStateNodeType stateNodeType
        {
            get { return EStateNodeType.User; }
        }
    }
}