using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("BehaviourSystemEditor-BT")]

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class NodeBase : ScriptableObject, IEquatable<NodeBase>
    {
        public enum ENodeCallState : byte
        {
            BeforeEnter,
            Updating,
            BeforeExit,
        };

        public enum EStatus : byte
        {
            Running,
            Failure,
            Success
        };

        public enum ENodeType : byte
        {
            Root,
            Action,
            Composite,
            Decorator,
            Subset
        };

        public event Action onNodeEnter;

        public event Action onNodeExit;


        public UGUID guid;

        public string tag;

        public string description;

        [NonSerialized]
        public int depth;

        [NonSerialized]
        public ulong callCount;

        public EStatus status;

        [NonSerialized]
        internal int callStackID;

        public NodeBase parent;

        [NonSerialized]
        public BehaviourTreeRunner runner;

#if UNITY_EDITOR
        [SerializeField]
        internal Vector2 position;
#endif

        public ENodeCallState callState
        {
            get;
            private set;
        }

        public Transform transform
        {
            get { return runner.transform; }
        }

        public abstract ENodeType nodeType
        {
            get;
        }

        public virtual string tooltip
        {
            get;
        }
        

        public EStatus UpdateNode()
        {
            if (this.runner is null)
            {
                return EStatus.Failure;
            }

            this.callCount++;

            if (callState == ENodeCallState.BeforeEnter)
            {
                this.EnterNode();
                this.onNodeEnter?.Invoke();
            }

            if (this.callState == ENodeCallState.Updating)
            {
                this.status = this.OnUpdate();

                if (this.status == EStatus.Running)
                {
                    return EStatus.Running;
                }

                if (this.runner.handler.GetCurrentNode(callStackID) != this)
                {
                    this.runner.handler.AbortSubtreeFrom(callStackID, this);
                }

                this.callState = ENodeCallState.BeforeExit;
            }

            if (this.callState == ENodeCallState.BeforeExit)
            {
                this.onNodeExit?.Invoke();
                this.ExitNode();
            }

            return this.status;
        }


        internal virtual void EnterNode()
        {
            this.runner.handler.PushInCallStack(callStackID, this);
            this.OnEnter();
            this.callState = ENodeCallState.Updating;
        }


        internal virtual void ExitNode()
        {
            this.runner.handler.PopInCallStack(callStackID);
            this.OnExit();
            this.callState = ENodeCallState.BeforeEnter;

            // If a parent node fails during execution, this node's result is set to Failure.
            if (this.status == EStatus.Running)
            {
                this.status = EStatus.Failure;
            }
        }


        public bool Equals(NodeBase other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.guid == other.guid;
        }
        
        
        /// <summary>Registers a callback to be executed during FixedUpdate. Used when the node needs to perform physics-based or time-consistent operations.</summary>
        protected void RegisterFixedUpdateCallback(Action callback)
        {
            if (callback is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] FixedUpdate callback is null.");
                return;
            }

            if (runner is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] BehaviourTreeRunner is not set.");
                return;
            }

            runner.onNodeFixedUpdate += callback;
        }

        
        /// <summary>Unregisters a previously registered FixedUpdate callback.</summary>
        protected void UnregisterFixedUpdateCallback(Action callback)
        {
            if (callback is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] FixedUpdate callback to unregister is null.");
                return;
            }

            if (runner is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] BehaviourTreeRunner is not set.");
                return;
            }

            runner.onNodeFixedUpdate -= callback;
        }

        
        /// <summary>Registers a callback for Gizmos rendering. Used when the node needs to draw debug visualization elements.</summary>
        protected void RegisterGizmosUpdateCallback(Action callback)
        {
            if (callback is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] Gizmos callback is null.");
                return;
            }

            if (runner is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] BehaviourTreeRunner is not set.");
                return;
            }

            runner.onNodeGizmosUpdate += callback;
        }

        
        /// <summary>Unregisters a previously registered Gizmos rendering callback.</summary>
        protected void UnregisterGizmosUpdateCallback(Action callback)
        {
            if (callback is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] Gizmos callback to unregister is null.");
                return;
            }

            if (runner is null)
            {
                Debug.LogError($"[{nameof(NodeBase)}] BehaviourTreeRunner is not set.");
                return;
            }

            runner.onNodeGizmosUpdate -= callback;
        }


        ///Function called after all nodes in the tree asset are created.
        /// This function is invoked using a breadth-first search (BFS) traversal pattern,
        /// processing nodes level by level starting from the root node.
        public virtual void PostTreeCreation() { }


        /// Called when the node execution begins.
        /// Used for initialization when the node is first executed.
        protected virtual void OnEnter() { }


        /// Called when the node execution ends.
        /// Used for cleanup and state reset when the node is no longer active.
        protected virtual void OnExit() { }


        /// Core behavior update function that must be implemented by derived classes.
        /// Returns the execution result of the node's behavior.
        protected abstract EStatus OnUpdate();
    }
}