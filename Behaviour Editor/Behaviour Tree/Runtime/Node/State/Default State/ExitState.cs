namespace BehaviourSystem.BT.State
{
    public class ExitState : StateNodeBase
    {
        public override EStateNodeType stateNodeType
        {
            get { return EStateNodeType.Exit; }
        }

        protected override void OnUpdate()
        {
            
        }
    }
}