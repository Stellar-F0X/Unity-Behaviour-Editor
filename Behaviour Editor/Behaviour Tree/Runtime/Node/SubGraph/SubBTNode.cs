namespace BehaviourSystem.BT.SubGraph
{
    public class SubBTNode : SubGraphNode
    {
        public BehaviourTree subBehaviourTree;
        
        protected override EStatus OnUpdate()
        {
            return subBehaviourTree.UpdateGraph();
        }
    }
}