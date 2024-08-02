using System;
using UnityEngine.Events;
using Object = UnityEngine.Object;


[Serializable]
public class BehaviourTreeEvent
{
    public BehaviourTreeEvent(string key, UnityEvent value)
    {
        this.value = value;
        this.key = key;
    }
    
    public string key;
    public UnityEvent value;


    public void Invoke()
    {
        value?.Invoke();
    }

    public bool HasInvocation()
    {
        return value != null;
    }
}