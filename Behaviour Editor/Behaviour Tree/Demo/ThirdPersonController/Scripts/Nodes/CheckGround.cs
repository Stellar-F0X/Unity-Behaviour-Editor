using System;
using UnityEngine;
using BehaviourSystem.BT;
using UnityEngine.Serialization;

namespace BehaviourSystem.BT.Demo
{
    [System.Serializable]
    public class CheckGround : ActionNode
    {
        [Range(10f, 75f)]
        public float acceptableFloorAngle = 45f;
        public float checkStartHeightOffset = 0.5f;
        public float groundCheckDistance = 0.5f;
        
        [SerializeReference]
        public BlackboardProperty<bool> isGrounded;
        public LayerMask groundCheckMask;
        

        protected override EBehaviourResult OnUpdate()
        {
            Vector3 start = runner.transform.position + new Vector3(0, checkStartHeightOffset, 0);
            
            if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, groundCheckDistance, groundCheckMask))
            {
                float angle = Vector3.Angle(hit.normal, Vector3.up);
                
                if (angle < acceptableFloorAngle)
                {
                    isGrounded.value = true;
                    return EBehaviourResult.Success;
                }
                else
                {
                    isGrounded.value = false;
                    return EBehaviourResult.Failure;
                }
            }
            
            isGrounded.value = false;
            return NodeBase.EBehaviourResult.Failure;
        }


        public override void GizmosUpdateNode()
        {
            
        }
    }
}