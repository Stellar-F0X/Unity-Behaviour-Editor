using UnityEngine;

namespace BehaviourSystem.BT
{
    public class RepeaterNode : DecoratorNode
    {
        public uint repeatCount;
        
        [SerializeField, ReadOnly]
        private int _currentCount;


        public override string tooltip
        {
            get { return "Repeats the child node a specified number of times."; }
        }


        protected override void OnEnter()
        {
            _currentCount = 0;
        }


        protected override EStatus OnUpdate()
        {
            if (_currentCount < repeatCount)
            {
                if (child.UpdateNode() != EStatus.Running)
                {
                    _currentCount++;
                }
                
                return EStatus.Running;
            }

            return EStatus.Success;
        }
    }
}