using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public sealed class RootNode : BehaviourNodeBase, IBehaviourIterable
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
        

        public override ENodeType nodeType
        {
            get { return ENodeType.Root; }
        }
        
        public int childCount
        {
            get { return _child.Count; }
        }
        
        public IEnumerable<NodeBase> GetChildren()
        {
            return _child;
        }

        
        protected override EStatus OnUpdate()
        {
            if (child is null)
            {
                return EStatus.Failure;
            }
            else
            {
                return child.UpdateNode();
            }
        }
    }
}