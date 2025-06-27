using BehaviourSystem.BT.State;

namespace BehaviourSystem.BT.SubGraph
{
    public class SubGraphAdaptorState : StateNodeBase
    {
        public GraphAsset subGraph;

        public override EStateNodeType stateNodeType
        {
            get { return EStateNodeType.User; }
        }
        
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