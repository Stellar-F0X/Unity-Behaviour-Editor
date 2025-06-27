using UnityEngine;

namespace BehaviourSystem.BT
{
    public class TimeLimitNode : DecoratorNode
    {
        public float limitTime;
        
        private float _startTime;


        public override string tooltip
        {
            get { return "Limits execution of child node to a specified duration. \nReturns Failure if time expires."; }
        }


        protected override void OnEnter()
        {
            _startTime = Time.time;
        }


        protected override EStatus OnUpdate()
        {
            if (_startTime + limitTime > Time.time)
            {
                return child.UpdateNode();
            }

            return EStatus.Failure;
        }
    }
}