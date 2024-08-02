using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SequencerNode : CompositeNode
{
    private int _current;
    private readonly Type _type = typeof(SequencerNode);

    protected  override void OnEnter(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        _current = 0;
    }

    protected override void OnExit(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        
    }
    
    
    protected override EState OnUpdate(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        switch (children[_current].UpdateNode(behaviourTree, new PreviusBehaviourInfo(tag, _type, baseType)))
        {
            case EState.Running: return EState.Running;
            case EState.Failure: return EState.Failure;
            case EState.Success: _current++; break;
        }

        if (_current == children.Count)
        {
            return EState.Success;
        }
        else
        {
            return EState.Running;
        }
    }
}
