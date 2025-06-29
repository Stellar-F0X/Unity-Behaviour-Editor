using UnityEditor;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [CreateAssetMenu(fileName = "New BT Asset", menuName = "Behaviour System/BT Asset", order = 1)]
    public sealed class BehaviourTreeAsset : GraphAsset
    {
        public override EGraphType graphType
        {
            get { return EGraphType.BT; }
        }
    }
}