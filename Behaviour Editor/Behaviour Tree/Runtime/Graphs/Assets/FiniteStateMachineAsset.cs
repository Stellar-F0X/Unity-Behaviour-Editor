using BehaviourSystem.BT.State;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [CreateAssetMenu(fileName = "New Finite State Machine Asset", menuName = "Behavior System/Finite State Machine Asset", order = 2)]
    public sealed class FiniteStateMachineAsset : GraphAsset
    {
        public override EGraphType graphType
        {
            get { return EGraphType.FSM; }
        }
        
        
#if UNITY_EDITOR
        public static FiniteStateMachine CreateFiniteStateMachineGraph(GraphAsset graphAsset)
        {
            FiniteStateMachine graph = CreateInstance<FiniteStateMachine>();
            graph.hideFlags = HideFlags.HideInHierarchy;
            UnityEditor.AssetDatabase.AddObjectToAsset(graph, graphAsset);

            if (graph.entry == null)
            {
                graph.CreateNode(typeof(EnterState), new Vector2Int(0, 0));
                graph.CreateNode(typeof(ExitState), new Vector2Int(200, 0));
                graph.CreateNode(typeof(AnyState), new Vector2Int(-200, 0));
                graph.currentState = graph.nodes[0] as StateNodeBase;
                graph.anyState = graph.nodes[2] as StateNodeBase;
                graph.entry = graph.nodes[0];
            }

            return graph;
#endif
        }
    }
}