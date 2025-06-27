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
            nextStateNodeUGUID = targetState;
            conditions.Add(new BlackboardBasedCondition());
        }
        
        
        [HideInInspector]
        public UGUID nextStateNodeUGUID;
        
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