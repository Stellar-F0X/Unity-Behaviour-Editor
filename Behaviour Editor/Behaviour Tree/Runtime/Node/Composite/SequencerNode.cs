namespace BehaviourSystem.BT
{
    public sealed class SequencerNode : CompositeNode
    {
        private int _childrenCount;
        private bool _childrenIsInvalid;

        public override void PostTreeCreation()
        {
            _childrenIsInvalid = children is null || children.Count == 0;
        }


        protected override EBehaviourResult OnUpdate(in float deltaTime)
        {
            if (_childrenIsInvalid)
            {
                return EBehaviourResult.Failure;
            }

            switch (children[_currentChildIndex].UpdateNode())
            {
                case EBehaviourResult.Running: return EBehaviourResult.Running;

                case EBehaviourResult.Failure: return EBehaviourResult.Failure;

                case EBehaviourResult.Success: _currentChildIndex++; break;
            }

            if (_currentChildIndex == children.Count)
            {
                return EBehaviourResult.Success;
            }
            else
            {
                return EBehaviourResult.Running;
            }
        }
    }
}