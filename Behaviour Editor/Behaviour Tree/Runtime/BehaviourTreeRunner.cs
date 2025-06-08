using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("BehaviourSystemEditor-BT")]

namespace BehaviourSystem.BT
{
    [DefaultExecutionOrder(-1), AddComponentMenu("Behaviour System/Behaviour Tree Runner")]
    public class BehaviourTreeRunner : MonoBehaviour
    {
        private readonly Dictionary<string, IBlackboardProperty> _properties = new Dictionary<string, IBlackboardProperty>();

        public bool useFixedUpdate = true;
        public bool useGizmos = true;


        [SerializeField]
        private BehaviourTree _runtimeTree;

        private BehaviourNodeHandler _nodeHandler;
        private NodeBase _rootNode;

        internal Action onNodeFixedUpdate;
        internal Action onNodeGizmosUpdate;


        internal BehaviourTree runtimeTree
        {
            get { return _runtimeTree; }
        }

        internal BehaviourNodeHandler handler
        {
            get { return _nodeHandler; }
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
            this._nodeHandler = new BehaviourNodeHandler(this._runtimeTree.nodeSet);
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
            
            _rootNode.UpdateNode();
        }


        private void FixedUpdate()
        {
            if (useFixedUpdate == false || _runtimeTree is null || pause)
            {
                return;
            }

            this.onNodeFixedUpdate?.Invoke();
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


        public bool TrySetProperty<TValue>(in string key, TValue value)
        {
            if (this._runtimeTree?.blackboard is null || enabled == false)
            {
                return false;
            }

            if (_properties.TryGetValue(key, out var existingProperty))
            {
                if (existingProperty is BlackboardProperty<TValue> prop)
                {
                    prop.value = value;
                    return true;
                }
            }
            else
            {
                IBlackboardProperty newProperty = _runtimeTree.blackboard.FindProperty(key);

                if (newProperty is BlackboardProperty<TValue> prop)
                {
                    prop.value = value;
                    _properties.Add(key, prop);
                    return true;
                }
            }

            return false;
        }


        public bool TryGetProperty<TValue>(in string key, out TValue value)
        {
            if (this._runtimeTree?.blackboard is null || enabled == false)
            {
                value = default;
                return false;
            }

            if (_properties.TryGetValue(key, out var existingProperty))
            {
                if (existingProperty is BlackboardProperty<TValue> castedProperty)
                {
                    value = castedProperty.value;
                    return true;
                }
            }
            else
            {
                IBlackboardProperty newProperty = _runtimeTree.blackboard.FindProperty(key);

                if (newProperty is BlackboardProperty<TValue> castedProperty)
                {
                    _properties.Add(key, newProperty);
                    value = castedProperty.value;
                    return true;
                }
            }

            value = default;
            return false;
        }


        public bool TryGetNodeByTreePath(string path, out NodeAccessor accessor)
        {
            if (_nodeHandler.TryGetNodeByPath(path, out accessor))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public bool TryGetNodeByTag(string nodeTag, out NodeAccessor[] accessors)
        {
            accessors = _nodeHandler.GetNodeByTag(nodeTag);

            if (accessors is null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}