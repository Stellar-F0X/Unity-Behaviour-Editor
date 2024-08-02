using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class EventNode : ActionNode
{
    public BehaviourTreeEvent onEnterEvent;
    public BehaviourTreeEvent onUpdateEvent;
    public BehaviourTreeEvent onExitEvent;

    protected override void OnEnter(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        onEnterEvent?.Invoke();
    }

    protected override EState OnUpdate(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        onUpdateEvent?.Invoke();
        return EState.Success;
    }

    protected override void OnExit(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        onExitEvent?.Invoke();
    }
}