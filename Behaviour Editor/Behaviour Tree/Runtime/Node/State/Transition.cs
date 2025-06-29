using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public struct Transition
    {
        public Transition(UGUID targetState, bool isUnconditional = false)
        {
            this.isUnconditional = isUnconditional; 
            this.nextStateNodeGuid = targetState;
            
            this.conditions = new List<BlackboardBasedCondition>();
        }

        public bool isUnconditional;
        
        public UGUID nextStateNodeGuid;
        public List<BlackboardBasedCondition> conditions;


        public bool CheckConditions()
        {
            if (this.isUnconditional)
            {
                return true;
            }
            
            for (int i = 0; i < this.conditions.Count; ++i)
            {
                if (this.conditions[i].Execute())
                {
                    return true;
                }
            }

            return false;
        }
    }
}