using BehaviourSystem.BT.State;

namespace BehaviourSystem.BT.SubGraph
{
    public class SubBTState : StateNodeBase
    {
        public BehaviourTree subBehaviourTree;
        
        protected override void OnUpdate()
        {
            subBehaviourTree.UpdateGraph();
        }
    }
}