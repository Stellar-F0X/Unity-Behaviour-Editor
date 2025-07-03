using System;

namespace BehaviourSystem.BT
{
    public struct FieldTraversalInfo
    {
        public FieldTraversalInfo(object value, Type type, int depth)
        {
            this.value = value;
            this.type = type;
            this.depth = depth;
        }
        
        public object value;
        public Type type;
        public int depth;
    }
}