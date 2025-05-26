using UnityEngine;
using UnityEngine.Serialization;

namespace BehaviourSystem.BT.Demo
{
    public class MoveNode : ActionNode
    {
        public float speedOffset = 0.1f;

        public float speedChangeRate = 10.0f;

        [SerializeReference]
        public BlackboardProperty<float> targetSpeed;
        
        [SerializeReference]
        public BlackboardProperty<float> currentSpeed;
        
        [SerializeReference]
        public BlackboardProperty<float> verticalVelocity;
        
        [SerializeReference]
        public BlackboardProperty<CharacterController> controller;
        

        protected override EBehaviourResult OnUpdate()
        {
            float currentHorizontalSpeed = new Vector3(controller.value.velocity.x, 0.0f, controller.value.velocity.z).magnitude;

            if (currentHorizontalSpeed < targetSpeed.value - speedOffset || currentHorizontalSpeed > targetSpeed.value + speedOffset)
            {
                currentSpeed.value = Mathf.Lerp(currentHorizontalSpeed, targetSpeed.value, Time.deltaTime * speedChangeRate);
                currentSpeed.value = Mathf.Round(currentSpeed.value * 1000f) * 0.001f;
            }
            else
            {
                currentSpeed.value = targetSpeed.value;
            }
            
            Vector3 horizontalMovement = transform.eulerAngles.normalized * currentSpeed.value;
            Vector3 verticalMovement = new Vector3(0.0f, verticalVelocity.value, 0.0f);

            controller.value.Move((horizontalMovement + verticalMovement) * Time.deltaTime);
            return EBehaviourResult.Success;
        }
    }
}