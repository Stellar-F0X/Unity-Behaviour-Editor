using System;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class ActionNode : NodeBase
    {
        public override sealed ENodeType nodeType
        {
            get { return ENodeType.Action; }
        }
    }
}