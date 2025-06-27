using BehaviourSystem.BT.State;

namespace BehaviourSystem.BT.SubGraph
{
    public class SubFSMState : StateNodeBase
    {
        public FiniteStateMachine subFiniteStateMachine;
        
        protected override void OnUpdate()
        {
            subFiniteStateMachine.UpdateGraph();
        }
    }
}