namespace BehaviourSystem.BT
{
    public class UntilForNode : DecoratorNode
    {
        public enum EUntilCondition
        {
            Failure = 1,
            Success = 2
        };

        public EUntilCondition targetResult = EUntilCondition.Success;


        public override string tooltip
        {
            get { return "Executes the child node repeatedly until it returns the specified result."; }
        }


        protected override EBehaviourResult OnUpdate()
        {
            switch (child.UpdateNode())
            {
                case EBehaviourResult.Failure:
                {
                    if (targetResult == EUntilCondition.Failure)
                    {
                        return EBehaviourResult.Failure;
                    }

                    break;
                }

                case EBehaviourResult.Success:
                {
                    if (targetResult == EUntilCondition.Success)
                    {
                        return EBehaviourResult.Success;
                    }

                    break;
                }
                
                default: return EBehaviourResult.Running;
            }
            
            return EBehaviourResult.Running;
        }
    }
}