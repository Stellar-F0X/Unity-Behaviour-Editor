using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourSystem.BT.Demo
{
    public class Patrol : ActionNode
    {
        [SerializeReference]
        public BlackboardProperty<NavMeshAgent> navigator;

        [SerializeReference]
        public BlackboardProperty<List<Transform>> waypoints;
        
        [SerializeReference]
        public BlackboardProperty<int> currentWaypointIndex;


        protected override void OnEnter()
        {
            navigator.value.SetDestination(waypoints.value[currentWaypointIndex.value].position);
        }


        protected override EStatus OnUpdate()
        {
            if (waypoints.value.Count == 0)
            {
                return EStatus.Failure;
            }
            
            if (navigator.value.pathPending)
            {
                return EStatus.Running;
            }
            
            if (navigator.value.remainingDistance <= navigator.value.stoppingDistance)
            {
                navigator.value.velocity = Vector3.zero;
                
                currentWaypointIndex.value = (currentWaypointIndex.value + 1) % waypoints.value.Count;
                return EStatus.Success;
            }
            else
            {
                return EStatus.Running;
            }
        }
    }
}