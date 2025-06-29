using UnityEngine;

namespace BehaviourSystem.BT.State
{
    public abstract class SubGraphState : StateNodeBase, ISubGraphNode
    {
        [HideInInspector]
        public GraphAsset subGraph;

        public override EStateNodeType stateNodeType
        {
            get { return EStateNodeType.SubGraph; }
        }

        GraphAsset ISubGraphNode.subGraphAsset
        {
            get => subGraph;

            set => subGraph = value;
        }

        public abstract EGraphType subGraphType
        {
            get;
        }
        
        
        protected override void OnEnter()
        {
            subGraph.graph.ResetGraph();
        }

        protected override void OnUpdate()
        {
            subGraph.graph.UpdateGraph();
        }

        protected override void OnExit()
        {
            subGraph.graph.StopGraph();
        }
    }
}