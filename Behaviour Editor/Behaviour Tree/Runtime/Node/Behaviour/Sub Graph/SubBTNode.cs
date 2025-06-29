namespace BehaviourSystem.BT
{
    public class SubBTNode : SubGraphNode
    {
        public override EGraphType subGraphType
        {
            get { return EGraphType.BT; }
        }
    }
}