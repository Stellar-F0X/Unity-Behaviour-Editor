using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public abstract class DecoratorNode : Node
{
    [HideInInspector]
    public Node child;

    public override ENodeType baseType
    {
        get { return ENodeType.Decorator; }
    }

    public override Node Clone()
    {
        DecoratorNode node = base.Clone() as DecoratorNode;
        node.child = this.child.Clone();
        return node;
    }
}
