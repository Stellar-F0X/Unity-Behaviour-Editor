namespace BehaviourSystem.BT
{
    public sealed class SelectorNode : CompositeNode
    {
        private bool _isChildrenInvalid;

        
        public override void PostTreeCreation()
        {
            _isChildrenInvalid = children is null || children.Count == 0;
        }

        
        protected override EStatus OnUpdate()
        {
            if (_isChildrenInvalid)
            {
                return EStatus.Failure;
            }

            switch (children[_currentChildIndex].UpdateNode())
            {
                case EStatus.Success: return EStatus.Success;

                case EStatus.Running: return EStatus.Running;

                case EStatus.Failure: _currentChildIndex++; break;
            }
            
            if (_currentChildIndex == children.Count)
            {
                return EStatus.Failure;
            }
            else
            {
                return EStatus.Running;
            }
        }
    }
}