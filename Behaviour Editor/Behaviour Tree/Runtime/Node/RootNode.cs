using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public sealed class RootNode : NodeBase, IBehaviourIterable
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
        
        public List<NodeBase> GetChildren()
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