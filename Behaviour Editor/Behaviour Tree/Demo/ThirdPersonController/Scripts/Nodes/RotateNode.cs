using System;
using UnityEngine;
using BehaviourSystem.BT;

namespace BehaviourSystem.BT.Demo
{
    [System.Serializable]
    public class RotateNode : ActionNode
    {
        [Range(0.0f, 0.3f)]
        public float rotationSmoothTime = 0.12f;
        
        [SerializeReference]
        public BlackboardProperty<float> cameraAxisY;
        
        [SerializeReference]
        public BlackboardProperty<Vector2> inputValue;
        
        
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        
        
        protected override EBehaviourResult OnUpdate()
        {
            Vector3 inputDirection = new Vector3(inputValue.value.x, 0.0f, inputValue.value.y).normalized;
            
            if (inputValue.value.sqrMagnitude > 0)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cameraAxisY.value;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }
            
            return EBehaviourResult.Success;
        }
    }
}