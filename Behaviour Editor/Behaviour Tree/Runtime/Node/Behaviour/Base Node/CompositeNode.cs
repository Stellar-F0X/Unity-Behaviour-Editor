using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class CompositeNode : BehaviorNodeBase, IBehaviourIterable
    {
        [HideInInspector]
        public List<BehaviorNodeBase> children = new List<BehaviorNodeBase>();

        protected int _currentChildIndex = 0;


        public override sealed EBehaviourNodeType nodeType
        {
            get { return EBehaviourNodeType.Composite; }
        }

        public int childCount
        {
            get { return children.Count; }
        }

        public int currentChildIndex
        {
            get { return _currentChildIndex; }
        }
        
        
        public IEnumerable<NodeBase> GetChildren()
        {
            return children;
        }


        public override sealed void ExitNode()
        {
            base.ExitNode();
            _currentChildIndex = 0;
        }
    }
}
