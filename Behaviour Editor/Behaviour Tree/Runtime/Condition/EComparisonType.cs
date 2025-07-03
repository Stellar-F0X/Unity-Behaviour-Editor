using System;

namespace BehaviourSystem.BT
{
    [Flags]
    public enum EComparisonType : byte
    {
        None                   = 0,
        Equal                  = 1 << 0,
        NotEqual               = 1 << 1,
        GreaterThan            = 1 << 2,
        GreaterThanOrEqual     = 1 << 3,
        LessThan               = 1 << 4,
        LessThanOrEqual        = 1 << 5,
        ComparisonOperators = Equal | NotEqual | GreaterThan | GreaterThanOrEqual | LessThan | LessThanOrEqual
    };
}