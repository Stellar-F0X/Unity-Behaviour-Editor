using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace BehaviourSystem.BT.Demo
{
    public class EnemyController : MonoBehaviour
    {
        public Transform player;
        public Animator animator;
        public NavMeshAgent navigator;
        public BehaviourTreeRunner runner;
        
        public List<Transform> waypoints;
        

        private void Awake()
        {
            navigator = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();

            runner.SetProperty("Animator", animator);
            runner.SetProperty("Way Point", waypoints);
            runner.SetProperty("Navigator", navigator);
            runner.SetProperty("Target", player);
            runner.SetProperty("Distance", Vector3.Distance(transform.position, player.position));
        }
        
        
        private void Update()
        {
            runner.SetProperty("Distance", Vector3.Distance(transform.position, player.position));
        }
        

        public void OnFootstep()
        {
            
        }
    }
}