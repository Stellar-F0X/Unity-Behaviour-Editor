namespace BehaviourSystem.BT
{
    public abstract class SubGraphNode : BehaviourNodeBase
    {
        public GraphAsset subGraph;
        
        public override EBehaviourNodeType nodeType
        {
            get { return EBehaviourNodeType.SubGraph; }
        }
    }
}