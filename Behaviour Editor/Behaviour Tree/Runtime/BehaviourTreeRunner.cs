using System;
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
        
        public bool useUpdateRate = false;
        public bool useFixedUpdate = true;
        public bool useGizmos = true;
        
        [SerializeField]
        private uint _updateRate = 60;
        private float _frameInterval;
        private float _timeSinceLastUpdate;
        
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

        public float updateInterval
        {
            get { return _frameInterval; }
        }
        

        /// <summary> Controls the update frequency for the behaviour tree runner. </summary>
        /// <value> An integer representing the update rate in frames per second, or -1 if <see cref="useUpdateRate"/> is disabled. </value>
        public int updateRate
        {
            get { return this.useUpdateRate ? (int)this._updateRate : -1; }
            
            set { this.SetUpdateRate((uint)value); }
        }


        private void Awake()
        {
            if (_runtimeTree is null)
            {
                Debug.LogError("BehaviourTree is not assigned.");
                this.enabled = false;
                return;
            }

            if (this.useUpdateRate)
            {
                this.SetUpdateRate(_updateRate);
            }
            
            this._runtimeTree = BehaviourTree.MakeRuntimeTree(this, _runtimeTree);
            this._nodeHandler = new BehaviourNodeHandler(this._runtimeTree.nodeSet);
            this._rootNode = _runtimeTree.nodeSet.rootNode;
        }


        private void Update()
        {
            if (_runtimeTree is null || pause)
            {
                return;
            }

            if (useUpdateRate == false || _frameInterval + _timeSinceLastUpdate < Time.time)
            {
                _rootNode.UpdateNode();
            }
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



        private void SetUpdateRate(uint targetUpdateRate)
        {
            if (this.useUpdateRate)
            {
                this._updateRate = (uint)Mathf.Max(Application.targetFrameRate, 0);
                this._updateRate = _updateRate == 0 ? targetUpdateRate : _updateRate;
                this._frameInterval = 1f / _updateRate;
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogWarning("Cannot set the update rate because useUpdateRate is disabled.");
            }
#endif
        }


        public void SetProperty<TValue>(in string key, TValue property)
        {
            if (this._runtimeTree.blackboard is null)
            {
                Debug.Log("Blackboard is not assigned.");
                return;
            }
            
            if (enabled == false)
            {
                return;
            }
            
            if (_properties.TryGetValue(key, out var existingProperty))
            {
                if (existingProperty is BlackboardProperty<TValue> prop)
                {
                    prop.value = property;
                    return;
                }
            }
            else
            {
                IBlackboardProperty newProperty = _runtimeTree.blackboard.FindProperty(key);

                if (newProperty is BlackboardProperty<TValue> prop)
                {
                    prop.value = property;
                    _properties.Add(key, prop);
                    return;
                }
            }

#if UNITY_EDITOR
            Debug.LogWarning($"Blackboard property with key '{key}' was not found.");
#endif
        }


        public TValue GetProperty<TValue>(in string key)
        {
            if (this._runtimeTree.blackboard is null)
            {
                Debug.Log("Blackboard is not assigned.");
                return default;
            }
            
            if (enabled == false)
            {
                Debug.Log("Blackboard property was not found.");
                return default;
            }
            
            if (_properties.TryGetValue(key, out var existingProperty))
            {
                if (existingProperty is BlackboardProperty<TValue> castedProperty)
                {
                    return castedProperty.value;
                }
            }
            else
            {
                IBlackboardProperty newProperty = _runtimeTree.blackboard.FindProperty(key);

                if (newProperty is BlackboardProperty<TValue> castedProperty)
                {
                    _properties.Add(key, newProperty);
                    return castedProperty.value;
                }
            }

#if UNITY_EDITOR
            Debug.LogWarning($"Blackboard property with key '{key}' was not found.");
#endif
            return default;
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