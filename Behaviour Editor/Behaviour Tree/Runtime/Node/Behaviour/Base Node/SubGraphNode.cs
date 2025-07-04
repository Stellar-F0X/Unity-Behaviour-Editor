using System;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public abstract class SubGraphNode : BehaviorNodeBase, ISubGraphNode
    {
        [HideInInspector]
        public GraphAsset subGraph;

        public override EBehaviourNodeType nodeType
        {
            get { return EBehaviourNodeType.SubGraph; }
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

        protected override EStatus OnUpdate()
        {
            return subGraph.graph.UpdateGraph();
        }

        protected override void OnExit()
        {
            subGraph.graph.StopGraph();
        }
    }
}