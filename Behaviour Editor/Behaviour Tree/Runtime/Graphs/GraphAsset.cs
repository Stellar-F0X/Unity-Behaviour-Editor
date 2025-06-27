using System;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public abstract class GraphAsset : ScriptableObject, IEquatable<GraphAsset>
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

        public abstract EGraphType graphType
        {
            get;
        }


        internal static GraphAsset MakeRuntimeGraph(BehaviourSystemRunner systemRunner, GraphAsset targetGraph)
        {
            Debug.Assert(systemRunner is not null && targetGraph is not null, "BehaviourRunner or BehaviourTree is null.");

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
            if (other is null || ReferenceEquals(this, other) == false)
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

            for (int i = 0; i < this.graph.nodes.Count; i++)
            {
                if (this.graph.nodes[i].guid != other.graph.nodes[i].guid)
                {
                    return false;
                }
            }

            return true;
        }
    }
}