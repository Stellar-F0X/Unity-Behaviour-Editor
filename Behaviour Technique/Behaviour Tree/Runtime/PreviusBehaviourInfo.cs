using UnityEngine;
using System;

[Serializable]
public struct PreviusBehaviourInfo
{
    public PreviusBehaviourInfo(string tag, Type nodeType, Node.ENodeType basedType)
    {
        this.tag = tag;
        this.nodeType = nodeType;
        this.basedType = basedType;
    }

    public string tag;
    public Type nodeType;
    public Node.ENodeType basedType;
}
