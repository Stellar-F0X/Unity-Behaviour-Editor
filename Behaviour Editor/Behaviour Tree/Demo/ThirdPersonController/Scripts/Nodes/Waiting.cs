using System;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourSystem.BT.Demo
{
    [Serializable]
    public class Waiting : ActionNode
    {
        public float distanceOffset = 0.1f;
        
        [SerializeReference]
        public BlackboardProperty<float> distance;
        
        [SerializeReference]
        public BlackboardProperty<NavMeshAgent> navigator;
        
        
        protected override EStatus OnUpdate()
        {
            if (distance.value <= navigator.value.stoppingDistance + distanceOffset)
            {
                return EStatus.Running;
            }
            
            return EStatus.Success;
        }
    }
}