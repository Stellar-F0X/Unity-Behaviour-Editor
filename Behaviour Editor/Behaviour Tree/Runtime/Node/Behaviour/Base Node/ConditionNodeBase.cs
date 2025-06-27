using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public abstract class ConditionNodeBase : DecoratorNode
    {
        public enum ESuccessType
        {
            Any,
            All
        };
        
        [Tooltip("How to evaluate conditions \n(Any: OR logic, All: AND logic)")]
        public ESuccessType successType;
        
        [Space(2)]
        public List<BlackboardBasedCondition> conditions;
        
        
        protected virtual bool CheckCondition()
        {
            int count = conditions.Count;

            switch (successType)
            {
                case ESuccessType.Any:
                {
                    for (int i = 0; i < count; ++i)
                    {
                        if (conditions[i].Execute())
                        {
                            return true;
                        }
                    }

                    return false;
                }

                case ESuccessType.All:
                {
                    for (int i = 0; i < count; ++i)
                    {
                        if (conditions[i].Execute() == false)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }
    }
}