using UnityEngine;

namespace BehaviourSystem.BT
{
    [CreateAssetMenu(fileName = "New BT Asset", menuName = "Behaviour System/BT Asset")]
    public sealed class BehaviourTreeAsset : GraphAsset
    {
        public override EGraphType graphType
        {
            get { return EGraphType.BehaviourTree; }
        }
    }
}