using System;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public sealed class BlackboardBasedCondition
    {
        [SerializeReference]
        public IBlackboardProperty property;

        [SerializeReference]
        public IBlackboardProperty comparableValue;

        public EConditionType conditionType;


        private const int _EQUAL = 0;

        private const int _GREATER = 1;
        
        private const int _LESS = -1;
        
        

        public bool Execute()
        {
            if ((property.comparableConditions & conditionType) == conditionType)
            {
                return this.Compare((IComparable<IBlackboardProperty>)property, comparableValue);
            }

            return false;
        }
        

        private bool Compare(IComparable<IBlackboardProperty> a, IBlackboardProperty b)
        {
            switch (conditionType)
            {
                case EConditionType.Trigger: return a.CompareTo(b) == _EQUAL;
                
                case EConditionType.Equal: return a.CompareTo(b) == _EQUAL;

                case EConditionType.NotEqual: return a.CompareTo(b) != _EQUAL;

                case EConditionType.GreaterThan: return a.CompareTo(b) == _GREATER;

                case EConditionType.GreaterThanOrEqual: return a.CompareTo(b) is _GREATER or _EQUAL;

                case EConditionType.LessThan: return a.CompareTo(b) == _LESS;

                case EConditionType.LessThanOrEqual: return a.CompareTo(b) is _LESS or _EQUAL;
            }

            return false;
        }
    }
}