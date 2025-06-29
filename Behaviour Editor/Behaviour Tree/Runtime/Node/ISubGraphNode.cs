namespace BehaviourSystem.BT
{
    public interface ISubGraphNode
    {
        public GraphAsset subGraphAsset
        {
            get;
            internal set;
        }

        public EGraphType subGraphType { get; }
    }
}