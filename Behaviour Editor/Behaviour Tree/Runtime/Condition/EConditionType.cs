using System;

namespace BehaviourSystem.BT
{
    [Flags]
    public enum EConditionType : byte
    {
        None                   = 0,
        Trigger                = 1 << 0,
        Equal                  = 1 << 1,
        NotEqual               = 1 << 2,
        GreaterThan            = 1 << 3,
        GreaterThanOrEqual     = 1 << 4,
        LessThan               = 1 << 5,
        LessThanOrEqual        = 1 << 6,
        ComparisonOperators = Equal | NotEqual | GreaterThan | GreaterThanOrEqual | LessThan | LessThanOrEqual
    };
}