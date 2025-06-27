using System;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [CreateAssetMenu(fileName = "New Behaviour Tree", menuName = "Behaviour Tree/Behaviour Tree Asset")]
    public sealed class GraphAsset : ScriptableObject, IEquatable<GraphAsset>
    {
        [HideInInspector]
        public Graph graph;
        
        [ReadOnly]
        public Blackboard blackboard;

#if UNITY_EDITOR
        [HideInInspector]
        public GraphGroup graphGroup;
#endif
        
        [field: SerializeField, ReadOnly]
        public UGUID guid
        {
            get;
            internal set;
        }


        internal static GraphAsset MakeRuntimeGraph(BehaviourSystemRunner systemRunner, GraphAsset targetGraph)
        {
            Debug.Assert(systemRunner is not null && targetGraph is not null, "BehaviourActor or BehaviourTree is null.");

            GraphAsset runtimeGraph = Instantiate(targetGraph);

            Debug.Assert(runtimeGraph is not null, "Failed to create runtime tree.");

#if UNITY_EDITOR
            runtimeGraph.graphGroup = targetGraph.graphGroup.Clone();
#endif
            runtimeGraph.blackboard = targetGraph.blackboard?.Clone();
            runtimeGraph.graph = targetGraph.graph.CloneGraph(runtimeGraph.blackboard);
            
            foreach (NodeBase currentNode in runtimeGraph.graph.nodes)
            {
                currentNode.runner = systemRunner;
                currentNode.PostCreation();
            }

            return runtimeGraph;
        }


        public bool Equals(GraphAsset other)
        {
            if (other is null || this.GetType() != other.GetType())
            {
                return false;
            }

            if (this.guid == other.guid)
            {
                return false;
            }

            if (this.graph.nodes.Count != other.graph.nodes.Count)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < this.graph.nodes.Count; i++)
                {
                    if (this.graph.nodes[i].guid != other.graph.nodes[i].guid)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}