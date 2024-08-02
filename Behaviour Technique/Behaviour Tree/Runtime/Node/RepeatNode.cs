public class RepeatNode : DecoratorNode
{
    protected override void OnEnter(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        
    }

    protected override void OnExit(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        
    }

    protected override EState OnUpdate(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        child.UpdateNode(behaviourTree, new PreviusBehaviourInfo(tag, GetType(), baseType));
        return EState.Running;
    }
}
