using System;
using System.Collections.Generic;
using BehaviourSystem.BT.State;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviourSystem.BT
{
    public abstract class GraphAsset : ScriptableObject, IEquatable<GraphAsset>, IDisposable
    {
        [HideInInspector]
        public Graph graph;

        [HideInInspector]
        public GraphAsset parentGraphAsset;

        [HideInInspector]
        public GraphAsset rootGraphAsset;

        [HideInInspector]
        public List<GraphAsset> subGraphAssets = new List<GraphAsset>();

        [SerializeField, ReadOnly]
        private Blackboard _blackboardAsset;

#if UNITY_EDITOR
        [HideInInspector]
        public GraphGroup graphGroup;
#endif

        public abstract EGraphType graphType
        {
            get;
        }
        
        [field: SerializeField, ReadOnly]
        public UGUID guid
        {
            get;
            internal set;
        }
        
        [field: SerializeField, HideInInspector]
        public bool isRootGraph
        {
            get;
            internal set;
        }
        
        public Blackboard blackboard
        {
            get { return isRootGraph ? this._blackboardAsset : this.rootGraphAsset._blackboardAsset; }

            set { (isRootGraph ? this : rootGraphAsset)._blackboardAsset = value; }
        }


        public static GraphAsset MakeRuntimeGraph(BehaviourSystemRunner systemRunner, GraphAsset targetGraph)
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


#if UNITY_EDITOR
        public void AddSubGraphAsset(GraphAsset subGraphAsset)
        {
            if (subGraphAsset == null)
            {
                Debug.LogError("subGraphAsset is null");
                return;
            }

            subGraphAsset.parentGraphAsset = this;
            subGraphAsset.rootGraphAsset = this.rootGraphAsset;
            subGraphAssets.Add(subGraphAsset);
            AssetDatabase.AddObjectToAsset(subGraphAsset, this);
        }

        
        public void RemoveSubGraphAsset()
        {
            for (int i = this.subGraphAssets.Count - 1; i >= 0; --i)
            {
                this.subGraphAssets[i].RemoveSubGraphAsset();
            }

            if (this.parentGraphAsset != null)
            {
                this.parentGraphAsset.subGraphAssets.Remove(this);
            }
            
            this.Dispose();
        }


        public void Dispose()
        {
            if (this.graph != null && this.graph.nodes != null)
            {
                this.graph.nodes.ForEach(GraphFactory.RemoveObjectFromAssetAndDestroyImmediate);
                GraphFactory.RemoveObjectFromAssetAndDestroyImmediate(this.graph);
                this.graph = null;
            }

            if (this.graphGroup != null && this.graphGroup.dataList != null)
            {
                this.graphGroup.dataList.ForEach(GraphFactory.RemoveObjectFromAssetAndDestroyImmediate);
                GraphFactory.RemoveObjectFromAssetAndDestroyImmediate(this.graphGroup);
                this.graphGroup = null;
            }
            
            this.subGraphAssets = null;
            this.parentGraphAsset = null;
            this.rootGraphAsset = null;
            
            GraphFactory.RemoveObjectFromAssetAndDestroyImmediate(this);
        }
#endif


        public bool Equals(GraphAsset other)
        {
            if (other is null || ReferenceEquals(this, other) == false)
            {
                return false;
            }

            if (this.guid != other.guid)
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