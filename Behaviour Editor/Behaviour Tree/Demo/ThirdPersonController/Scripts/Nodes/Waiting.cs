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
        private bool _isWaiting = false; 


        protected override void OnEnter()
        {
            _startTime = 0f;
            _isWaiting = false;
        }


        protected override EStatus OnUpdate()
        {
            if (_isWaiting == false && distance.value <= navigator.value.stoppingDistance + distanceOffset)
            {
                _startTime = Time.time;
                _isWaiting = true;
            }
            
            if (_isWaiting)
            {
                if (Time.time - _startTime >= waitTime)
                {
                    return EStatus.Success;
                }
                
                return EStatus.Running;
            }
            
            return EStatus.Running;
        }
    }
}