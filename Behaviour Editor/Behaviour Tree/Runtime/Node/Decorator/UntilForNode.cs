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


        protected override EStatus OnUpdate()
        {
            switch (child.UpdateNode())
            {
                case EStatus.Failure:
                {
                    if (targetResult == EUntilCondition.Failure)
                    {
                        return EStatus.Failure;
                    }

                    break;
                }

                case EStatus.Success:
                {
                    if (targetResult == EUntilCondition.Success)
                    {
                        return EStatus.Success;
                    }

                    break;
                }
                
                default: return EStatus.Running;
            }
            
            return EStatus.Running;
        }
    }
}