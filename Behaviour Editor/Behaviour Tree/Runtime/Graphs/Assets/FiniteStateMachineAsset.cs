using UnityEngine;

namespace BehaviourSystem.BT
{
    [CreateAssetMenu(fileName = "New FSM Asset", menuName = "Behaviour System/FSM Asset", order = 2)]
    public sealed class FiniteStateMachineAsset : GraphAsset
    {
        public override EGraphType graphType
        {
            get { return EGraphType.FSM; }
        }
    }
}