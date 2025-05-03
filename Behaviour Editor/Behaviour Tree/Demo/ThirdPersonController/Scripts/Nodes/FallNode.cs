using System;
using UnityEngine;
using BehaviourSystem.BT;
using UnityEngine.Serialization;

namespace BehaviourSystem.BT.Demo
{
    [System.Serializable]
    public class FallNode : ActionNode
    {
        public float terminalVelocity = 53.0f;
        
        [SerializeReference]
        public BlackboardProperty<float> verticalVelocity;

        protected override void OnEnter() { }

        protected override NodeBase.EBehaviourResult OnUpdate()
        {
            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (verticalVelocity.value < terminalVelocity)
            {
                verticalVelocity.value += Physics.gravity.y * Time.deltaTime;
            }

            return EBehaviourResult.Running;
        }

        protected override void OnExit()
        {
            if (verticalVelocity.value < 0.0f)
            {
                verticalVelocity.value = -2f;
            }
        }
    }
}