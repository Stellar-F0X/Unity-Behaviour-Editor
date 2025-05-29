using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public sealed class RootNode : NodeBase, IBehaviourIterable
    {
        [HideInInspector]
        public NodeBase child;

        public override ENodeType nodeType
        {
            get { return ENodeType.Root; }
        }
        
        public int childCount
        {
            get { return 1; }
        }
        
        public IEnumerable<NodeBase> GetChildren()
        {
            yield return child;
        }

        
        protected override EBehaviourResult OnUpdate()
        {
            if (child is null)
            {
                return EBehaviourResult.Failure;
            }
            else
            {
                return child.UpdateNode();
            }
        }


        public override void GizmosUpdateNode()
        {
            child?.GizmosUpdateNode();
        }
    }
}