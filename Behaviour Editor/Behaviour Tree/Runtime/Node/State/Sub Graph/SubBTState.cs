namespace BehaviourSystem.BT.State
{
    public class SubBTState : SubGraphState
    {
        public override EGraphType subGraphType
        {
            get { return EGraphType.BT; }
        }
    }
}