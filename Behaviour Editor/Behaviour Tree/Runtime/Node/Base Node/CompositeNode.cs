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
        
        protected int _currentChildIndex;


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
        
        
        public override void FixedUpdateNode()
        {
            if (currentChildIndex < children.Count)
            {
                children[_currentChildIndex].FixedUpdateNode();
            }
        }

        
        public override void GizmosUpdateNode()
        {
            if (currentChildIndex < children.Count)
            {
                children[_currentChildIndex].GizmosUpdateNode();
            }
        }


        internal override sealed void ExitNode()
        {
            base.ExitNode();
            _currentChildIndex = 0;
        }
    }
}
