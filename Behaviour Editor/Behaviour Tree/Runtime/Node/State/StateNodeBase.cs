using System.Collections.Generic;

namespace BehaviourSystem.BT.State
{
    public abstract class StateNodeBase : NodeBase
    {
        public List<Transition> transitions = new List<Transition>();


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
            this.OnEnter();
            this.callState = ENodeCallState.Updating;
        }


        public override void ExitNode()
        {
            this.OnExit();
            this.callState = ENodeCallState.BeforeEnter;
        }


        protected abstract void OnUpdate();
    }
}