using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Profiling;

[assembly: InternalsVisibleTo("BehaviourSystemEditor-BT")]

namespace BehaviourSystem.BT
{
    [DefaultExecutionOrder(-1), AddComponentMenu("Behavior System/Behavior System Runner")]
    public class BehaviorSystemRunner : MonoBehaviour
    {
        public ETickUpdateMode tickUpdateMode = ETickUpdateMode.NormalUpdate;

        public bool useFixedUpdate = true;
        public bool useGizmos = true;


        [SerializeField]
        private GraphAsset _runtimeGraph;

        internal event Action onNodeFixedUpdate;
        internal event Action onNodeGizmosUpdate;

        internal GraphAsset runtimeGraph
        {
            get { return _runtimeGraph; }
        }

        public bool pause
        {
            get;
            set;
        }


        private void Awake()
        {
            if (_runtimeGraph is null)
            {
                Debug.LogError("BehaviourTree is not assigned.");
                this.enabled = false;
                return;
            }

            this._runtimeGraph = GraphFactory.CloneGraph(this, _runtimeGraph);
        }


        private void OnDestroy()
        {
            onNodeFixedUpdate = null;
            onNodeGizmosUpdate = null;
        }


        private void Update()
        {
            if (_runtimeGraph is null || pause)
            {
                return;
            }

            if (tickUpdateMode == ETickUpdateMode.NormalUpdate)
            {
#if UNITY_EDITOR
                Profiler.BeginSample("BehaviourTreeRunner.RuntimeUpdate");
#endif
                _runtimeGraph.graph.UpdateGraph();
#if UNITY_EDITOR
                Profiler.EndSample();
#endif
            }
        }


        private void FixedUpdate()
        {
            if (useFixedUpdate == false || _runtimeGraph is null || pause)
            {
                return;
            }

            if (tickUpdateMode == ETickUpdateMode.FixedUpdate)
            {
#if UNITY_EDITOR
                Profiler.BeginSample("BehaviourTreeRunner.RuntimeFixedUpdate");
#endif
                _runtimeGraph.graph.UpdateGraph();
#if UNITY_EDITOR
                Profiler.EndSample();
#endif
            }

            this.onNodeFixedUpdate?.Invoke();
        }


        private void LateUpdate()
        {
            if (_runtimeGraph is null || pause)
            {
                return;
            }

            if (tickUpdateMode == ETickUpdateMode.LateUpdate)
            {
#if UNITY_EDITOR
                Profiler.BeginSample("BehaviourTreeRunner.RuntimeLateUpdate");
#endif
                _runtimeGraph.graph.UpdateGraph();
#if UNITY_EDITOR
                Profiler.EndSample();
#endif
            }
        }


        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                return;
            }
#endif

            if (useGizmos == false || _runtimeGraph is null)
            {
                return;
            }

            this.onNodeGizmosUpdate?.Invoke();
        }


        public void ExternalUpdate()
        {
            if (_runtimeGraph is null || pause)
            {
                return;
            }

            if (tickUpdateMode == ETickUpdateMode.ExternalUpdate)
            {
#if UNITY_EDITOR
                Profiler.BeginSample("BehaviourTreeRunner.RuntimeExternalUpdate");
#endif
                _runtimeGraph.graph.UpdateGraph();
#if UNITY_EDITOR
                Profiler.EndSample();
#endif
            }
            else
            {
                Debug.LogWarning("ExternalUpdate는 tickUpdateMode가 ExternalUpdate로 설정되어 있을 때만 호출해야 합니다.");
            }
        }


        public void SetProperty<TValue>(in string key, TValue value)
        {
            if (this._runtimeGraph?.blackboardAsset is null || enabled == false)
            {
                throw new InvalidOperationException("BehaviourTree 또는 Blackboard가 활성화되어 있지 않습니다.");
            }

            BlackboardVariable foundVariable = _runtimeGraph.blackboardAsset.FindVariable(key);

            if (foundVariable.variable is Variable<TValue> prop)
            {
                prop.value = value;
                return;
            }

            throw new KeyNotFoundException($"키 '{key}'에 해당하는 프로퍼티를 찾을 수 없습니다.");
        }


        public TValue GetProperty<TValue>(in string key)
        {
            if (this._runtimeGraph?.blackboardAsset is null || enabled == false)
            {
                throw new InvalidOperationException("BehaviourTree 또는 Blackboard가 활성화되어 있지 않습니다.");
            }
            
            BlackboardVariable foundVariable = _runtimeGraph.blackboardAsset.FindVariable(key);

            if (foundVariable.variable is Variable<TValue> castedProperty)
            {
                return castedProperty.value;
            }

            throw new KeyNotFoundException($"키 '{key}'에 해당하는 프로퍼티를 찾을 수 없습니다.");
        }


        public bool TryGetNodeByTag(string nodeTag, out NodeBase[] resultNodes)
        {
            int count = _runtimeGraph.graph.nodes.Count;

            List<NodeBase> nodeListPool = ListPool<NodeBase>.Get();
            IReadOnlyList<NodeBase> nodes = _runtimeGraph.graph.nodes;

            for (int i = 0; i < count; ++i)
            {
                if (string.Compare(nodes[i].tag, nodeTag) == 0)
                {
                    nodeListPool.Add(nodes[i]);
                }
            }

            if (nodeListPool.Count == 0)
            {
                ListPool<NodeBase>.Release(nodeListPool);
                resultNodes = null;
                return false;
            }

            resultNodes = nodeListPool.ToArray();
            ListPool<NodeBase>.Release(nodeListPool);
            return true;
        }
    }
}