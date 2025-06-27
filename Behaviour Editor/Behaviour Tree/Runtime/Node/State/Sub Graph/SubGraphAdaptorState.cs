using BehaviourSystem.BT.State;

namespace BehaviourSystem.BT.SubGraph
{
    public class SubGraphAdaptorState : StateNodeBase
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