using UnityEngine;

namespace BehaviourSystem.BT
{
    public abstract class BehaviorNodeBase : NodeBase
    {
        public enum EBehaviourNodeType : byte
        {
            Root,
            Action,
            Composite,
            Decorator,
            SubGraph
        };
        
        [SerializeField]
        private NodeBase _parent;
        private BehaviorTree _tree;
        
        
        public abstract EBehaviourNodeType nodeType
        {
            get;
        }
        
        public EStatus status
        {
            get;
            private set;
        }
        
        public NodeBase parent
        {
            get { return _parent; }
            
            internal set { _parent = value; }
        }
        
        public BehaviorTree tree
        {
            get { return _tree; }

            internal set { _tree = value; }
        }
        
        
        public EStatus UpdateNode()
        {
            this.callCount++;

            if (callState == ENodeCallState.BeforeEnter)
            {
                this.EnterNode();
            }

            if (this.callState == ENodeCallState.Updating)
            {
                this.status = this.OnUpdate();

                if (this.status == EStatus.Running)
                {
                    return EStatus.Running;
                }

                if (this.tree.interrupter.GetCurrentNode(callStackID) != this)
                {
                    this.tree.interrupter.AbortSubtreeFrom(callStackID, this);
                }

                this.callState = ENodeCallState.BeforeExit;
            }

            if (this.callState == ENodeCallState.BeforeExit)
            {
                this.ExitNode();
            }

            return this.status;
        }


        public override void EnterNode()
        {
            this.tree.interrupter.PushInCallStack(callStackID, this);
            this.OnEnter();
            this.onNodeEnter?.Invoke();
            this.callState = ENodeCallState.Updating;
        }


        public override void ExitNode()
        {
            this.tree.interrupter.PopInCallStack(callStackID);
            this.OnExit();
            this.onNodeExit?.Invoke();
            this.callState = ENodeCallState.BeforeEnter;

            // If a parent node fails during execution, this node's result is set to Failure.
            if (this.status == EStatus.Running)
            {
                this.status = EStatus.Failure;
            }
        }
        
        
        /// Core behavior update function that must be implemented by derived classes.
        /// Returns the execution result of the node's behavior.
        protected abstract EStatus OnUpdate();
    }
}