using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace BehaviourSystem.BT
{
    public class RandomSelector : CompositeNode
    {
        private bool _isChildrenInvalid;
        private int _currentRandomIndex;

        private readonly List<int> _randomIndices = new List<int>();

        
        public override void PostTreeCreation()
        {
            _isChildrenInvalid = children is null || children.Count == 0;

            if (_isChildrenInvalid)
            {
                return;
            }
            
            for (int i = 0; i < children!.Count; i++)
            {
                _randomIndices.Add(i);
            }
        }


        protected override void OnEnter()
        {
            this.ShuffleIndices(_randomIndices);
            _currentChildIndex = _randomIndices[_currentRandomIndex];
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

                case EBehaviourResult.Failure: _currentChildIndex = _randomIndices[++_currentRandomIndex]; break;
            }

            if (_currentRandomIndex == children.Count)
            {
                return EBehaviourResult.Failure;
            }
            else
            {
                return EBehaviourResult.Running;
            }
        }


        private void ShuffleIndices(List<int> indices)
        {
            int temp = 0;
            int tempIndex = 0;

            for (int i = indices.Count - 1; i > 0; i--)
            {
                tempIndex = Random.Range(0, i + 1);

                temp = indices[i];
                indices[i] = indices[tempIndex];
                indices[tempIndex] = temp;
            }
        }
    }
}