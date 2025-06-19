using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Profiling;

[assembly: InternalsVisibleTo("BehaviourSystemEditor-BT")]

namespace BehaviourSystem.BT
{
    [DefaultExecutionOrder(-1), AddComponentMenu("Behaviour System/Behaviour Tree Runner")]
    public class BehaviourTreeRunner : MonoBehaviour
    {
        private readonly Dictionary<string, IBlackboardProperty> _properties = new Dictionary<string, IBlackboardProperty>();

        public ETickUpdateMode tickUpdateMode = ETickUpdateMode.NormalUpdate;

        public bool useFixedUpdate = true;
        public bool useGizmos = true;


        [SerializeField]
        private BehaviourTree _runtimeTree;

        private BehaviourCallStack _callStackHandler;
        private NodeBase _rootNode;


        internal event Action onNodeFixedUpdate;
        internal event Action onNodeGizmosUpdate;

        internal BehaviourTree runtimeTree
        {
            get { return _runtimeTree; }
        }

        internal BehaviourCallStack handler
        {
            get { return _callStackHandler; }
        }

        public bool pause
        {
            get;
            set;
        }


        private void Awake()
        {
            if (_runtimeTree is null)
            {
                Debug.LogError("BehaviourTree is not assigned.");
                this.enabled = false;
                return;
            }

            this._runtimeTree = BehaviourTree.MakeRuntimeTree(this, _runtimeTree);
            this._callStackHandler = new BehaviourCallStack(this._runtimeTree.nodeSet);
            this._rootNode = _runtimeTree.nodeSet.rootNode;
        }


        private void OnDestroy()
        {
            onNodeFixedUpdate = null;
            onNodeGizmosUpdate = null;
        }


        private void Update()
        {
            if (_runtimeTree is null || pause)
            {
                return;
            }

            if (tickUpdateMode == ETickUpdateMode.NormalUpdate)
            {
#if UNITY_EDITOR
                Profiler.BeginSample("BehaviourTreeRunner.Update");
#endif
                _rootNode.UpdateNode();
#if UNITY_EDITOR
                Profiler.EndSample();
#endif
            }
        }


        private void FixedUpdate()
        {
            if (useFixedUpdate == false || _runtimeTree is null || pause)
            {
                return;
            }

            if (tickUpdateMode == ETickUpdateMode.FixedUpdate)
            {
#if UNITY_EDITOR
                Profiler.BeginSample("BehaviourTreeRunner.FixedUpdate");
#endif
                _rootNode.UpdateNode();
#if UNITY_EDITOR
                Profiler.EndSample();
#endif
            }

            this.onNodeFixedUpdate?.Invoke();
        }


        private void LateUpdate()
        {
            if (_runtimeTree is null || pause)
            {
                return;
            }

            if (tickUpdateMode == ETickUpdateMode.LateUpdate)
            {
#if UNITY_EDITOR
                Profiler.BeginSample("BehaviourTreeRunner.LateUpdate");
#endif
                _rootNode.UpdateNode();
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

            if (useGizmos == false || _runtimeTree is null)
            {
                return;
            }

            this.onNodeGizmosUpdate?.Invoke();
        }


        public void ExternalUpdate()
        {
            if (_runtimeTree is null || pause)
            {
                return;
            }

            if (tickUpdateMode == ETickUpdateMode.ExternalUpdate)
            {
#if UNITY_EDITOR
                Profiler.BeginSample("BehaviourTreeRunner.ExternalUpdate");
#endif
                _rootNode.UpdateNode();
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
            if (this._runtimeTree?.blackboard is null || enabled == false)
            {
                throw new InvalidOperationException("BehaviourTree 또는 Blackboard가 활성화되어 있지 않습니다.");
            }

            if (_properties.TryGetValue(key, out var existingProperty))
            {
                if (existingProperty is BlackboardProperty<TValue> prop)
                {
                    prop.value = value;
                    return;
                }

                throw new InvalidOperationException($"키 '{key}'에 대한 타입이 일치하지 않습니다.");
            }
            else
            {
                IBlackboardProperty newProperty = _runtimeTree.blackboard.FindProperty(key);

                if (newProperty is BlackboardProperty<TValue> prop)
                {
                    prop.value = value;
                    _properties.Add(key, prop);
                    return;
                }

                throw new KeyNotFoundException($"키 '{key}'에 해당하는 프로퍼티를 찾을 수 없습니다.");
            }
        }


        public TValue GetProperty<TValue>(in string key)
        {
            if (this._runtimeTree?.blackboard is null || enabled == false)
            {
                throw new InvalidOperationException("BehaviourTree 또는 Blackboard가 활성화되어 있지 않습니다.");
            }

            if (_properties.TryGetValue(key, out var existingProperty))
            {
                if (existingProperty is BlackboardProperty<TValue> castedProperty)
                {
                    return castedProperty.value;
                }

                throw new InvalidOperationException($"키 '{key}'에 대한 타입이 일치하지 않습니다.");
            }
            else
            {
                IBlackboardProperty newProperty = _runtimeTree.blackboard.FindProperty(key);

                if (newProperty is BlackboardProperty<TValue> castedProperty)
                {
                    _properties.Add(key, newProperty);
                    return castedProperty.value;
                }

                throw new KeyNotFoundException($"키 '{key}'에 해당하는 프로퍼티를 찾을 수 없습니다.");
            }
        }


        public bool TryGetNodeByTag(string nodeTag, out NodeAccessor[] accessors)
        {
            int count = _runtimeTree.nodeSet.nodeList.Count;

            List<NodeBase> nodeListPool = ListPool<NodeBase>.Get();
            IReadOnlyList<NodeBase> nodes = _runtimeTree.nodeSet.nodeList;

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
                accessors = null;
                return false;
            }

            accessors = new NodeAccessor[nodeListPool.Count];
            
            for (int i= 0; i < nodeListPool.Count; ++i)
            {
                accessors[i] = new NodeAccessor(nodeListPool[i]);
            }

            ListPool<NodeBase>.Release(nodeListPool);
            return true;
        }
    }
}