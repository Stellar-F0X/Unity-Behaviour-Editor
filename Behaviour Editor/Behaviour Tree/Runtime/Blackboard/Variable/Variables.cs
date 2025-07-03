using System;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public class IntVariable : Variable<int>
    {
        public override EComparisonType comparison => EComparisonType.ComparisonOperators;
        
        protected override int CompareTo(Variable<int> other) => this.value.CompareTo(other.value);
    }

    
    [Serializable]
    public class AnimatorVariable : Variable<Animator>
    {
        public override EComparisonType comparison => EComparisonType.Equal | EComparisonType.NotEqual;

        protected override int CompareTo(Variable<Animator> other) => this._value == other.value ? 0 : -1;
    }
}