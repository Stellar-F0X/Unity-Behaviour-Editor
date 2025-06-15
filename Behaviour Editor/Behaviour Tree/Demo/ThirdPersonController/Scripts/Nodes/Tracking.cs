using UnityEngine;
using UnityEngine.AI;

namespace BehaviourSystem.BT.Demo
{
    public class Tracking : ActionNode
    {
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

            if (navigator.value.remainingDistance <= navigator.value.stoppingDistance)
            {
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