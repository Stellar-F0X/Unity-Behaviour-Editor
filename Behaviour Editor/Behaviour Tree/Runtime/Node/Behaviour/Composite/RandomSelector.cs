using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace BehaviourSystem.BT
{
    public class RandomSelector : CompositeNode
    {
        private bool _isChildrenInvalid;
        private int _currentRandomIndex;

        private readonly List<int> _randomIndices = new List<int>();

        
        public override void PostCreation()
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

                case EStatus.Failure: _currentChildIndex = _randomIndices[++_currentRandomIndex]; break;
            }

            if (_currentRandomIndex == children.Count)
            {
                return EStatus.Failure;
            }
            else
            {
                return EStatus.Running;
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