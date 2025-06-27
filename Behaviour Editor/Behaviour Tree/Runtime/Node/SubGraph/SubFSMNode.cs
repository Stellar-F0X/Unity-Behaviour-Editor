namespace BehaviourSystem.BT.SubGraph
{
    public class SubFSMNode : SubGraphNode
    {
        public FiniteStateMachine subFiniteStateMachine;
        
        protected override EStatus OnUpdate()
        {
            return subFiniteStateMachine.UpdateGraph();
        }
    }
}