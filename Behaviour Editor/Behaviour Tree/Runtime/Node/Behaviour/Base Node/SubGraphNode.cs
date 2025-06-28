namespace BehaviourSystem.BT
{
    public class SubGraphNode : ActionNode
    {
        public GraphAsset subGraph;
        
        protected override void OnEnter()
        {
            subGraph?.graph.ResetGraph();
        }

        protected override EStatus OnUpdate()
        {
            if (subGraph is null)
            {
                return EStatus.Failure;
            }
            
            return subGraph.graph.UpdateGraph();
        }
        
        protected override void OnExit()
        {
            subGraph?.graph.StopGraph();
        }
    }
}