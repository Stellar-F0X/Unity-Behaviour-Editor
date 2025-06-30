using System;
using BehaviourSystem.BT.State;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public class FiniteStateMachine : Graph
    {
        public StateNodeBase currentState;
        public StateNodeBase anyState;
        //TODO: ExitStateNode 구현해야 됨.
        

        public override EStatus UpdateGraph()
        {
            if (currentState is null)
            {
                return EStatus.Failure;
            }

            if (currentState.callState == ENodeCallState.BeforeEnter)
            {
                currentState.EnterNode();
                anyState.EnterNode();
            }

            UGUID nextStateGuid;

            //현재 상태에서 전이가 발생하면 || 기준 왼쪽 함수에서 얻어온 guid를 토대로 전이할 것이고 anyState에서 발생하면 그 반대.
            if (currentState.CheckTransition(out nextStateGuid) || anyState.CheckTransition(out nextStateGuid))
            {
                currentState?.ExitNode();

                if (base.TryGetNodeByGuid(nextStateGuid, out NodeBase node))
                {
                    currentState = (StateNodeBase)node;
                    currentState.EnterNode();
                }
            }

            currentState.UpdateNode();
            return EStatus.Running;
        }


        public override void ResetGraph()
        {
            if (nodes[0] is EnterState enterState)
            {
                currentState = enterState;
            }
            else
            {
                Debug.LogError("그래프 생성할 때, 문제가 발생함.");
            }
        }


        public override void StopGraph()
        {
            if (currentState.callState == ENodeCallState.Updating)
            {
                currentState.ExitNode();
            }

            if (anyState.callState == ENodeCallState.Updating)
            {
                anyState.ExitNode();
            }
        }


#if UNITY_EDITOR
        public void ConnectStates(StateNodeBase from, StateNodeBase to)
        {
            //Check already contained
            for (int i = 0; i < from.transitions.Count; ++i)
            {
                if (from.transitions[i].nextStateNodeGuid == to.guid)
                {
                    return;
                }
            }

            Undo.RecordObject(this, "Finite State Machine (Connect)");

            from.transitions.Add(new Transition(to.guid));

            EditorUtility.SetDirty(this);
        }


        public void DisconnectStates(StateNodeBase from, StateNodeBase to)
        {
            int targetIndex = -1;

            for (int i = 0; i < from.transitions.Count; ++i)
            {
                if (from.transitions[i].nextStateNodeGuid == to.guid)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex != -1)
            {
                Undo.RecordObject(this, "Finite State Machine (Disconnect)");
                
                from.transitions.RemoveAt(targetIndex);
                
                EditorUtility.SetDirty(this);
            }
        }


        [Obsolete("아직 사용하지 않음")]
        public void AddState(StateNodeBase state)
        {
            Undo.RecordObject(this, "Finite State Machine (AddState)");

            nodes.Add(state);

            EditorUtility.SetDirty(this);
        }


        [Obsolete("아직 사용하지 않음")]
        public void RemoveState(StateNodeBase state)
        {
            Undo.RecordObject(this, "Finite State Machine (RemoveState)");

            nodes.Remove(state);

            EditorUtility.SetDirty(this);
        }
#endif
    }
}