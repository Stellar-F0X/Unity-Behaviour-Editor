using System;
using System.Collections.Generic;
using BehaviourSystem.BT.State;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviourSystem.BT
{
    //TODO : 리팩토링할때, 실행 로직이랑 데이터랑 분리
    public abstract class GraphAsset : ScriptableObject, IEquatable<GraphAsset>
    {
        [SerializeField, HideInInspector]
        private Graph _graph;

        [SerializeField, HideInInspector]
        private GraphAsset _rootGraphAsset;

        [SerializeField, HideInInspector]
        private GraphAsset _parentGraphAsset;

        [SerializeField, HideInInspector]
        private List<GraphAsset> _subGraphAssets = new List<GraphAsset>();

        [SerializeField, ReadOnly]
        private Blackboard _blackboardAsset;

#if UNITY_EDITOR
        [SerializeField, HideInInspector]
        private GraphGroup _graphGroup;
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
            get { return isRootGraph ? this._blackboardAsset : this._rootGraphAsset._blackboardAsset; }

            internal set { (isRootGraph ? this : _rootGraphAsset)._blackboardAsset = value; }
        }

        public GraphAsset rootGraphAsset
        {
            get { return _rootGraphAsset; }

            internal set { _rootGraphAsset = value; }
        }

        public Graph graph
        {
            get { return _graph; }
            
            internal set { _graph = value; }
        }

        public List<GraphAsset> subGraphAssets
        {
            get { return _subGraphAssets; }
        }

#if UNITY_EDITOR
        public GraphGroup graphGroup
        {
            get { return _graphGroup; }
            
            internal set { _graphGroup = value; }
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

            if (this.graph.nodes != other.graph.nodes)
            {
                return false;
            }

            return true;
        }


#if UNITY_EDITOR
        public void AddSubGraphAsset(GraphAsset subGraphAsset)
        {
            if (subGraphAsset == null)
            {
                Debug.LogError("subGraphAsset is null");
                return;
            }

            subGraphAsset._parentGraphAsset = this;
            subGraphAsset._rootGraphAsset = this._rootGraphAsset;
            
            _subGraphAssets.Add(subGraphAsset);
            AssetDatabase.AddObjectToAsset(subGraphAsset, this);
        }


        public void RemoveSubGraphAsset()
        {
            for (int i = this._subGraphAssets.Count - 1; i >= 0; --i)
            {
                this._subGraphAssets[i].RemoveSubGraphAsset();
            }

            if (this._parentGraphAsset != null)
            {
                this._parentGraphAsset._subGraphAssets.Remove(this);
            }

            if (this._graph != null && this._graph.nodes != null)
            {
                this._graph.nodes.ForEach(GraphFactory.RemoveObjectFromAssetAndDestroyImmediate);
                GraphFactory.RemoveObjectFromAssetAndDestroyImmediate(this._graph);
            }

            if (this._graphGroup != null && this._graphGroup.dataList != null)
            {
                this._graphGroup.dataList.ForEach(GraphFactory.RemoveObjectFromAssetAndDestroyImmediate);
                GraphFactory.RemoveObjectFromAssetAndDestroyImmediate(this._graphGroup);
            }

            this._graph = null;
            this._graphGroup = null;
            this._subGraphAssets = null;
            this._parentGraphAsset = null;
            this._rootGraphAsset = null;

            GraphFactory.RemoveObjectFromAssetAndDestroyImmediate(this);
        }
#endif
    }
}