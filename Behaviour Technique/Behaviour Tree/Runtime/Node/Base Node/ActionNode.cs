using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public abstract class ActionNode : Node
{
    public override ENodeType baseType
    {
        get { return ENodeType.Action; }
    }
}
