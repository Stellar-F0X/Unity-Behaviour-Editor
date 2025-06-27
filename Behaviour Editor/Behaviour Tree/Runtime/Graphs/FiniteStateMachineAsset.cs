using UnityEngine;

namespace BehaviourSystem.BT
{
    [CreateAssetMenu(fileName = "New FSM Asset", menuName = "Behaviour System/FSM Asset")]
    public sealed class FiniteStateMachineAsset : GraphAsset
    {
        public override EGraphType graphType
        {
            get { return EGraphType.StateMachine; }
        }
    }
}