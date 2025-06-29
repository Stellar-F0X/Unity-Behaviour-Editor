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
            this.conditions.Add(new BlackboardBasedCondition());
        }

        public bool isUnconditional;
        
        [HideInInspector]
        public UGUID nextStateNodeGuid;
        public List<BlackboardBasedCondition> conditions;


        public bool CheckConditions()
        {
            if (isUnconditional)
            {
                return true;
            }
            
            for (int i = 0; i < conditions.Count; ++i)
            {
                if (conditions[i].Execute())
                {
                    return true;
                }
            }

            return false;
        }
    }
}