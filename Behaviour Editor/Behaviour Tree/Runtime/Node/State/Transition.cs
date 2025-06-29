using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public class Transition
    {
        public Transition(UGUID targetState)
        {
            nextStateNodeUguid = targetState;
            conditions.Add(new BlackboardBasedCondition());
        }
        
        
        [HideInInspector]
        public UGUID nextStateNodeUguid;
        
        public List<BlackboardBasedCondition> conditions = new List<BlackboardBasedCondition>();


        public bool CheckConditions()
        {
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