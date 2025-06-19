using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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



        //private IEnumerator Start()
        //{
        //    bool found = runner.TryGetNodeByTag("patrol", out NodeAccessor[] patrolNode);
        //    
        //    if (found)
        //    {
        //        Debug.Log("Registering patrol callbacks");
        //        
        //        patrolNode[0].RegisterNodeEnterCallback(OnPatrolStart);
        //        patrolNode[0].RegisterNodeExitCallback(OnPatrolEnd);
        //    }
        //
        //    yield return new WaitForSeconds(10f);
        //
        //    if (found)
        //    {
        //        Debug.Log("Unregistering patrol callbacks");
        //        
        //        patrolNode[0].UnregisterNodeEnterCallback(OnPatrolStart);
        //        patrolNode[0].UnregisterNodeExitCallback(OnPatrolEnd);
        //    }
        //}
        
        
        void OnPatrolStart() => Debug.Log("Patrol Start");
        
        void OnPatrolEnd() => Debug.Log("Patrol End");
        


        private void Update()
        {
            runner.SetProperty("Distance", Vector3.Distance(transform.position, player.position));
        }


        public void OnFootstep() { }
    }
}