using UnityEngine;
using UnityEngine.AI;

namespace BehaviourSystem.BT.Demo
{
    public class Tracking : ActionNode
    {
        [Header("Tracking Settings")]
        public float distanceOffset = 0.1f;
        
        [Space]
        [SerializeReference]
        public BlackboardProperty<Transform> target;

        [SerializeReference]
        public BlackboardProperty<NavMeshAgent> navigator;


        protected override void OnEnter()
        {
            navigator.value.SetDestination(target.value.position);
        }


        protected override EStatus OnUpdate()
        {
            if (navigator.value.pathPending)
            {
                return EStatus.Running;
            }
            
            if (navigator.value.remainingDistance <= navigator.value.stoppingDistance + distanceOffset)
            {
                navigator.value.velocity = Vector3.zero;
                return EStatus.Success;
            }
            else
            {
                navigator.value.SetDestination(target.value.position);
                return EStatus.Running;
            }
        }
    }
}