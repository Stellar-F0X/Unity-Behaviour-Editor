using System;
using UnityEngine;
using BehaviourSystem.BT;
using UnityEngine.Serialization;

namespace BehaviourSystem.BT.Demo
{
    [System.Serializable]
    public class JumpNode : ActionNode
    {
        public float jumpHeight = 1.2f;
        public float jumpTimeout = 0.50f;

        [SerializeReference]
        public BlackboardProperty<float> verticalVelocity;
        
        private float _lastJumpTime;
        


        protected override void OnEnter()
        {
            if (_lastJumpTime + jumpTimeout > Time.time)
            {
                return;
            }

            // the square root of H * -2 * G = how much velocity needed to reach desired height
            verticalVelocity.value = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            _lastJumpTime = Time.time;
        }


        protected override EBehaviourResult OnUpdate()
        {
            if (_lastJumpTime + jumpTimeout > Time.time)
            {
                return EBehaviourResult.Failure;
            }
            else
            {
                return EBehaviourResult.Success;
            }
        }
    }
}