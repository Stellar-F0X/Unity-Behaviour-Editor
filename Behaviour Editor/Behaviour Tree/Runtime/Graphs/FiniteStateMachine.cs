using BehaviourSystem.BT.State;
using UnityEditor;

namespace BehaviourSystem.BT
{
    public class FiniteStateMachine : Graph
    {
        public StateNodeBase currentState;


        public override Graph CloneGraph(Blackboard clonedBlackboard)
        {
            FiniteStateMachine clonedSet = CreateInstance<FiniteStateMachine>();

            for (int i = 0; i < nodes.Count; ++i)
            {
                clonedSet.nodes[i] = Instantiate(this.nodes[i]);
                NodePropertyFieldBinder.BindNodeProperties(clonedSet.nodes[i], clonedBlackboard);
            }
            
            clonedSet.currentState = clonedSet.nodes[0] as StateNodeBase; //EnterState
            return clonedSet;
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
                currentState?.ExitNode();

                if (base.TryGetNodeByGuid(nextStateUGUID, out NodeBase node))
                {
                    StateNodeBase stateNode = node as StateNodeBase;
                    currentState = stateNode;
                    currentState.EnterNode();
                }
            }
            
            currentState!.UpdateNode();
            return EStatus.Running;
        }


        public override void ResetGraph()
        {
            
        }


        public override void StopGraph()
        {
            if (currentState.callState == ENodeCallState.Updating)
            {
                currentState.ExitNode();
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
            Undo.RecordObject(this, "Finite State Machine (Disconnect)");

            for (int i = from.transitions.Count - 1; i >= 0; --i)
            {
                if (from.transitions[i].nextStateNodeGuid == to.guid)
                {
                    from.transitions.RemoveAt(i);
                    break;
                }
            }

            EditorUtility.SetDirty(this);
        }
        
        
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