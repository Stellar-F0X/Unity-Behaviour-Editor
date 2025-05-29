using UnityEngine;
using System;
using System.Collections.Generic;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class DecoratorNode : NodeBase, IBehaviourIterable
    {
        [HideInInspector]
        public NodeBase child;


        public override sealed ENodeType nodeType
        {
            get { return ENodeType.Decorator; }
        }

        public int childCount
        {
            get { return 1; }
        }

        
        public IEnumerable<NodeBase> GetChildren()
        {
            yield return child;
        }

        public override sealed void FixedUpdateNode()
        {
            child?.FixedUpdateNode();
        }

        public override sealed void GizmosUpdateNode()
        {
            child?.GizmosUpdateNode();
        }
    }
}