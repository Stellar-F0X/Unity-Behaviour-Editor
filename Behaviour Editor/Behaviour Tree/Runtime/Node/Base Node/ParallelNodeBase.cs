using System.Collections.Generic;

namespace BehaviourSystem.BT
{
    public abstract class ParallelNodeBase : CompositeNode
    {
        protected List<bool> _isChildStopped;

        
        public override void PostTreeCreation()
        {
            _isChildStopped = new List<bool>(children.Count);

            for (int i = 0; i < children.Count; ++i)
            {
                _isChildStopped.Add(false);
            }
        }

        
        protected override void OnEnter()
        {
            int count = children?.Count ?? 0;

            for (int i = 0; i < count; ++i)
            {
                _isChildStopped[i] = false;
            }
        }
        
        
        protected override void OnExit()
        {
            int count = children?.Count ?? 0;
            
            for (int i = 0; i < count; ++i)
            {
                if (_isChildStopped[i] == false)
                {
                    runner.handler.AbortSubtree(children[i].callStackID);
                    _isChildStopped[i] = true;
                }
            }
        }
        
        
        public virtual void Stop()
        {
            for (int i = 0; i < children.Count; ++i)
            {
                _isChildStopped[i] = false;
            }
        }


        protected abstract EBehaviourResult EvaluatePolicy();
    }
}