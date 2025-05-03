namespace BehaviourSystem.BT
{
    public abstract class SubsetNode : NodeBase
    {
        public override ENodeType nodeType
        {
            get { return ENodeType.Subset; }
        }
    }
}