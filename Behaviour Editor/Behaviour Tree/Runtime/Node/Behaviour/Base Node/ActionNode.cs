using System;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class ActionNode : BehaviourNodeBase
    {
        public override sealed EBehaviourNodeType nodeType
        {
            get { return EBehaviourNodeType.Action; }
        }
    }
}