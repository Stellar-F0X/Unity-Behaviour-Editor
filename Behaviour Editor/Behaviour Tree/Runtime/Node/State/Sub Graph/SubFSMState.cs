namespace BehaviourSystem.BT.State
{
    public class SubFSMState : SubGraphState
    {
        public override EGraphType subGraphType
        {
            get { return EGraphType.FSM; }
        }
    }
}