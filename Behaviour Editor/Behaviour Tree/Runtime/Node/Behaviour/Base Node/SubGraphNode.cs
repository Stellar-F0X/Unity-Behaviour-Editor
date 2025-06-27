namespace BehaviourSystem.BT
{
    public abstract class SubGraphNode : BehaviourNodeBase
    {
        public override ENodeType nodeType
        {
            get { return ENodeType.SubGraph; }
        }
    }
}