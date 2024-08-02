public class FSMNode : SubsetNode
{
    protected override void OnEnter(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        
    }

    protected override EState OnUpdate(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        return EState.Running;
    }

    protected override void OnExit(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        
    }
}
