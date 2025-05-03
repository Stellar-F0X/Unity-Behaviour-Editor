using UnityEngine;
using System;
using System.Collections.Generic;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class DecoratorNode : NodeBase, IBehaviourIterable
    {
        [SerializeField, HideInInspector]
        private List<NodeBase> _child = new List<NodeBase>(1);

        
        public NodeBase child
        {
            get
            {
                if (_child.Count == 0)
                {
                    return null;
                }

                return _child[0];
            }
            
            set
            {
                if (_child.Count == 1)
                {
                    _child[0] = value;
                }
                else
                {
                    _child.Add(value);
                }
            }
        }


        public override sealed ENodeType nodeType
        {
            get { return ENodeType.Decorator; }
        }

        public int childCount
        {
            get { return _child.Count; }
        }

        
        public List<NodeBase> GetChildren()
        {
            return _child;
        }
    }
}