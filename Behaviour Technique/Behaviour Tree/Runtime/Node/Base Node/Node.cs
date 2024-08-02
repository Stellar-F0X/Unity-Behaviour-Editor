using System;
using UnityEngine;
using BehaviourTechnique;
using Object = UnityEngine.Object;


[Serializable]
public abstract class Node : ScriptableObject
{
    public enum ENodeCallState
    {
        OnEnter,
        OnUpdate,
        OnExit
    };

    public enum EState
    {
        Running,
        Failure,
        Success
    };

    public enum ENodeType
    {
        Root,
        Action,
        Composite,
        Decorator,
        Subset
    };

    [HideInInspector]
    public EState state = EState.Running;

    [HideInInspector]
    public bool started = false;

    [HideInInspector]
    public Vector2 position; //나중에 nodeView로 옮김

    [ReadOnly]
    public string guid;

    public string tag;


    public ENodeCallState updateState
    {
        get;
        private set;
    }

    public abstract ENodeType baseType
    {
        get;
    }



    protected abstract void OnEnter(BehaviourActor behaviourTree, PreviusBehaviourInfo info);

    protected abstract EState OnUpdate(BehaviourActor behaviourTree, PreviusBehaviourInfo info);

    protected abstract void OnExit(BehaviourActor behaviourTree, PreviusBehaviourInfo info);


    public virtual EState UpdateNode(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        switch (updateState)
        {
            case ENodeCallState.OnEnter:             
                started = true;
                this.OnEnter(behaviourTree, info);
                updateState = ENodeCallState.OnUpdate;
                return EState.Running;

            case ENodeCallState.OnUpdate: 
                state = this.OnUpdate(behaviourTree, info);
                EState resultState = state != EState.Running ? EState.Running : state;
                updateState = state != EState.Running ? ENodeCallState.OnExit : updateState;
                return resultState;

            case ENodeCallState.OnExit: 
                this.OnExit(behaviourTree, info);
                updateState = ENodeCallState.OnEnter;
                started = false;
                return state;

            default: throw new ArgumentOutOfRangeException("Not supported CallState Type");
        }
    }


    public virtual Node Clone()
    {
        return Object.Instantiate(this);
    }
}
