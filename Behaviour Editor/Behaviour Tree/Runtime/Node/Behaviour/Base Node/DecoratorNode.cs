using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class DecoratorNode : BehaviourNodeBase, IBehaviourIterable
    {
        [SerializeField, HideInInspector]
        private List<BehaviourNodeBase> _child = new List<BehaviourNodeBase>(1);

        
        public BehaviourNodeBase child
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
                if (value is null)
                {
                    _child.Clear();
                    return;
                }
                
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


        public override sealed EBehaviourNodeType nodeType
        {
            get { return EBehaviourNodeType.Decorator; }
        }

        public int childCount
        {
            get { return _child.Count; }
        }

        
        public IEnumerable<NodeBase> GetChildren()
        {
            return _child;
        }
    }
}