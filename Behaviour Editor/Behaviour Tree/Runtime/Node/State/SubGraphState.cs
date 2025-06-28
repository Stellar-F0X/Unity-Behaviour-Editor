namespace BehaviourSystem.BT.State
{
    public class SubGraphState : CustomStateNode
    {
        public GraphAsset subGraph;
        
        
        protected override void OnEnter()
        {
            subGraph?.graph.ResetGraph();
        }

        protected override void OnUpdate()
        {
            subGraph?.graph.UpdateGraph();
        }

        protected override void OnExit()
        {
            subGraph?.graph.StopGraph();
        }
    }
}