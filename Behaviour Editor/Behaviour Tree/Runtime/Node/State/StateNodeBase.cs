using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT.State
{
    public abstract class StateNodeBase : NodeBase
    {
        public List<Transition> transitions = new List<Transition>();

        public float enterTime
        {
            get;
            private set;
        }

        public float elapsedTime
        {
            get { return Time.time - enterTime; }
        }
        
        

        public virtual void UpdateNode()
        {
            this.OnUpdate();
        }


        public bool CheckTransition(out UGUID nextStateNodeUGUID)
        {
            for (int i = 0; i < transitions.Count; ++i)
            {
                if (transitions[i].CheckConditions())
                {
                    nextStateNodeUGUID = transitions[i].nextStateNodeUGUID;
                    return true;
                }
            }

            nextStateNodeUGUID = default;
            return false;
        }


        public override void EnterNode()
        {
            enterTime = Time.time;
            this.OnEnter();
            this.onNodeEnter?.Invoke();
            this.callState = ENodeCallState.Updating;
        }


        public override void ExitNode()
        {
            this.OnExit();
            this.onNodeExit?.Invoke();
            this.callState = ENodeCallState.BeforeEnter;
            enterTime = 0;
        }


        protected abstract void OnUpdate();
    }
}