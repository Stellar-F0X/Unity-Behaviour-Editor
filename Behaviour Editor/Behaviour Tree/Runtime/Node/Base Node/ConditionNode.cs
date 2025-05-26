using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public abstract class ConditionNode : DecoratorNode
    {
        public enum EConditionType
        {
            Any,
            All
        };
        
        [Tooltip("How to evaluate conditions \n(Any: OR logic, All: AND logic)")]
        public EConditionType conditionType;
        
        [Space(2)]
        public List<BlackboardBasedCondition> conditions;
        
        
        protected virtual bool CheckCondition()
        {
            int count = conditions.Count;

            switch (conditionType)
            {
                case EConditionType.Any:
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

                case EConditionType.All:
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