namespace BehaviourSystem.BT
{
    public sealed class SelectorNode : CompositeNode
    {
        private int _childrenCount;
        private bool _isChildrenInvalid;



        public override void PostTreeCreation()
        {
            _isChildrenInvalid = children is null || children.Count == 0;
        }

        
        protected override EBehaviourResult OnUpdate(in float deltaTime)
        {
            if (_isChildrenInvalid)
            {
                return EBehaviourResult.Failure;
            }

            switch (children[_currentChildIndex].UpdateNode())
            {
                case EBehaviourResult.Success: return EBehaviourResult.Success;

                case EBehaviourResult.Running: return EBehaviourResult.Running;

                case EBehaviourResult.Failure: _currentChildIndex++; break;
            }
            
            if (_currentChildIndex == children.Count)
            {
                return EBehaviourResult.Failure;
            }
            else
            {
                return EBehaviourResult.Running;
            }
        }
    }
}