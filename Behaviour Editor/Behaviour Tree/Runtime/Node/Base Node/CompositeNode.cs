using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class CompositeNode : NodeBase, IBehaviourIterable
    {
        [HideInInspector]
        public List<NodeBase> children = new List<NodeBase>();

        protected int _currentChildIndex = 0;


        public override sealed ENodeType nodeType
        {
            get { return ENodeType.Composite; }
        }

        public int childCount
        {
            get { return children.Count; }
        }

        public int currentChildIndex
        {
            get { return _currentChildIndex; }
        }
        
        
        public List<NodeBase> GetChildren()
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
