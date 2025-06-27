namespace BehaviourSystem.BT.State
{
    public class AnyState : StateNodeBase
    {
        public override EStateNodeType stateNodeType
        {
            get { return EStateNodeType.Any; }
        }

        protected override void OnUpdate()
        {
            
        }
    }
}