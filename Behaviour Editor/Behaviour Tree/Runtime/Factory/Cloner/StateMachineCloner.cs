using BehaviourSystem.BT.State;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public class StateMachineCloner : GraphCloner
    {
        public override EGraphType cloneGraphType
        {
            get { return EGraphType.FSM; }
        }

        public override void ClearAllNodesOfGraph(GraphAsset graph)
        {
            
        }
        
        public override Graph CloneGraph(BehaviorSystemRunner systemRunner, Graph targetGraph, BlackboardAsset blackboardAsset)
        {
            FiniteStateMachine originalFSM = targetGraph as FiniteStateMachine;
            FiniteStateMachine clonedFSM = ScriptableObject.CreateInstance<FiniteStateMachine>();

            for (int i = 0; i < originalFSM.nodes.Count; ++i)
            {
                clonedFSM.nodes[i] = Object.Instantiate(originalFSM.nodes[i]);
                //TODO: 트랜지션도 복사해야됨.
            }
            
            clonedFSM.currentState = clonedFSM.nodes[0] as StateNodeBase; //EnterState
            //TODO: ExitStateNode 자리.
            clonedFSM.anyState = clonedFSM.nodes[2] as StateNodeBase; //AnyState
            return clonedFSM;
        }
    }
}