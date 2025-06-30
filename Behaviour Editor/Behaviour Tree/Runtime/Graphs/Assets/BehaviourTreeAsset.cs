using UnityEngine;

namespace BehaviourSystem.BT
{
    [CreateAssetMenu(fileName = "New Behavior Tree Asset", menuName = "Behavior System/Behavior Tree Asset", order = 1)]
    public sealed class BehaviourTreeAsset : GraphAsset
    {
        public override EGraphType graphType
        {
            get { return EGraphType.BT; }
        }
        
        
#if UNITY_EDITOR
        public static BehaviourTree CreateBehaviorTreeGraph(GraphAsset graphAsset)
        {
            BehaviourTree graph = CreateInstance<BehaviourTree>();
            graph.hideFlags = HideFlags.HideInHierarchy;
            UnityEditor.AssetDatabase.AddObjectToAsset(graph, graphAsset);

            if (graph.entry == null)
            {
                graph.CreateNode(typeof(RootNode));
                graph.entry = graph.nodes[0];
            }

            return graph;
        }
#endif
    }
}