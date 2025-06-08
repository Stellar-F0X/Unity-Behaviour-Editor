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


        protected override EBehaviourResult OnUpdate()
        {
            if (_currentCount < repeatCount)
            {
                if (child.UpdateNode() != EBehaviourResult.Running)
                {
                    _currentCount++;
                }
                
                return EBehaviourResult.Running;
            }

            return EBehaviourResult.Success;
        }
    }
}