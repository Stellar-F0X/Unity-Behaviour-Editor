using System;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class ActionNode : BehaviourNodeBase
    {
        public override sealed ENodeType nodeType
        {
            get { return ENodeType.Action; }
        }
    }
}