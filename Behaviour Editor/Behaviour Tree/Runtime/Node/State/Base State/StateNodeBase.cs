using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT.State
{
    public abstract class StateNodeBase : NodeBase
    {
        // 상태 노드의 유형을 정의하는 열거형
        public enum EStateNodeType
        {
            Enter, // 진입 상태 (직접 생성 불가)
            Any,   // Any 상태 (직접 생성 불가)
            User,  // 유저가 직접 생성 가능한 일반 상태
            SubGraph,
            Exit   // 종료 상태 (직접 생성 불가)
        };

        
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
        
        public abstract EStateNodeType stateNodeType
        {
            get;
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
                    nextStateNodeUGUID = transitions[i].nextStateNodeGuid;
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