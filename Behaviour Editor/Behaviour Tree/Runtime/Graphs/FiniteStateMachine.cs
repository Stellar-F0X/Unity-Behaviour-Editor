using BehaviourSystem.BT.State;
using UnityEditor;

namespace BehaviourSystem.BT
{
    public class FiniteStateMachine : Graph
    {
        public StateNodeBase currentState;


        public override Graph CloneGraph(Blackboard clonedBlackboard)
        {
            return null;
        }


        public override EStatus UpdateGraph()
        {
            if (currentState is null)
            {
                return EStatus.Failure;
            }

            if (currentState.callState == ENodeCallState.BeforeEnter)
            {
                currentState.EnterNode();
            }

            if (currentState.CheckTransition(out UGUID nextStateUGUID))
            {
                if (currentState is not null)
                {
                    currentState.ExitNode();
                }

                if (base.TryGetNodeByGUID(nextStateUGUID, out NodeBase node))
                {
                    StateNodeBase stateNode = node as StateNodeBase;
                    currentState = stateNode;
                    currentState.EnterNode();
                }
            }
            
            currentState!.UpdateNode();
            return EStatus.Running;
        }


        public override void ResetGraph() { }


        public override void StopGraph() { }


#if UNITY_EDITOR
        public void AddState(StateNodeBase state)
        {
            Undo.RecordObject(this, "Finite State Machine (AddState)");

            nodes.Add(state);

            EditorUtility.SetDirty(this);
        }


        public void RemoveState(StateNodeBase state)
        {
            Undo.RecordObject(this, "Finite State Machine (RemoveState)");

            nodes.Remove(state);

            EditorUtility.SetDirty(this);
        }
#endif
    }
}