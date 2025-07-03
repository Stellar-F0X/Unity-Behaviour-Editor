using System;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public sealed class BlackboardBasedCondition
    {
        [SerializeReference]
        public BlackboardVariable blackboardVariable;

        [SerializeReference]
        public Variable comparableValue;
        public EComparisonType comparableType;
        

        public bool Execute()
        {
            if (blackboardVariable is null)
            {
                Debug.LogWarning("Blackboard property is not set for this condition.");
                return false;
            }
            
            if ((blackboardVariable.comparison & comparableType) == comparableType)
            {
                return this.Compare(blackboardVariable.variable, comparableValue);
            }

            return false;
        }
        

        private bool Compare(IComparable<Variable> a, Variable b)
        {
            switch (comparableType)
            {
                case EComparisonType.Equal: return a.CompareTo(b) == 0;

                case EComparisonType.NotEqual: return a.CompareTo(b) != 0;

                case EComparisonType.GreaterThan: return a.CompareTo(b) == 1;

                case EComparisonType.GreaterThanOrEqual: return a.CompareTo(b) is 1 or 0;

                case EComparisonType.LessThan: return a.CompareTo(b) == -1;

                case EComparisonType.LessThanOrEqual: return a.CompareTo(b) is -1 or 0;
            }

            return false;
        }
    }
}