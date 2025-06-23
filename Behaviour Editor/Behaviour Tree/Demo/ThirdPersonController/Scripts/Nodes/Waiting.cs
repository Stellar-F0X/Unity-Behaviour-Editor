using System;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourSystem.BT.Demo
{
    [Serializable]
    public class Waiting : ActionNode
    {
        public float distanceOffset = 0.1f;
        public float waitTime = 2.0f;

        [SerializeReference]
        public BlackboardProperty<float> distance;

        [SerializeReference]
        public BlackboardProperty<NavMeshAgent> navigator;

        private float _startTime = 0f;


        protected override void OnEnter()
        {
            _startTime = 0f;
        }


        protected override EStatus OnUpdate()
        {
            if (distance.value <= navigator.value.stoppingDistance + distanceOffset)
            {
                _startTime = Time.time;
            }
            
            if (Time.time > _startTime + waitTime)
            {
                return EStatus.Success;
            }
            else
            {
                return EStatus.Running;
            }
        }
    }
}