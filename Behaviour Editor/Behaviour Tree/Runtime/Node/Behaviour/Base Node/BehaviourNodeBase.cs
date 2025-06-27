using UnityEngine;

namespace BehaviourSystem.BT
{
    public abstract class BehaviourNodeBase : NodeBase
    {
        public enum ENodeType : byte
        {
            Root,
            Action,
            Composite,
            Decorator,
            SubGraph
        };
        
        [SerializeField]
        private NodeBase _parent;
        
        
        public abstract ENodeType nodeType
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
        
        
        public EStatus UpdateNode()
        {
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


        public override void EnterNode()
        {
            this.runner.handler.PushInCallStack(callStackID, this);
            this.OnEnter();
            this.callState = ENodeCallState.Updating;
        }


        public override void ExitNode()
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
        
        
        /// Core behavior update function that must be implemented by derived classes.
        /// Returns the execution result of the node's behavior.
        protected abstract EStatus OnUpdate();
    }
}