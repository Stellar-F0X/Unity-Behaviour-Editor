using System;
using System.Collections.Generic;

namespace BehaviourSystem.BT
{
    public readonly struct NodeAccessor
    {
        public NodeAccessor(NodeBase targetNode)
        {
            this._targetNode = targetNode;
        }

        private readonly NodeBase _targetNode;

        public string name
        {
            get { return _targetNode.name; }
        }

        public string tag
        {
            get { return _targetNode.tag; }
            set { _targetNode.tag = value; }
        }

        public NodeBase.ENodeCallState state
        {
            get { return _targetNode.callState; }
        }

        public bool hasChildren
        {
            get { return _targetNode is IBehaviourIterable iterable && iterable.childCount > 0; }
        }


        public bool TryGetChildren(out NodeAccessor[] accessors)
        {
            if (_targetNode is IBehaviourIterable iterable)
            {
                accessors = new NodeAccessor[iterable.childCount];
                List<NodeBase> children = iterable.GetChildren();
                int count = children.Count;

                for (int i = 0; i < count; ++i)
                {
                    accessors[i] = new NodeAccessor(children[i]);
                }

                return true;
            }

            accessors = null;
            return false;
        }


        public bool TryGetParent(out NodeAccessor accessor)
        {
            if (_targetNode.parent is null)
            {
                accessor = default;
                return false;
            }

            accessor = new NodeAccessor(_targetNode.parent);
            return true;
        }


        public void RegisterNodeEnterCallback(Action callback)
        {
            _targetNode.onNodeEnter += callback;
        }


        public void UnregisterNodeEnterCallback(Action callback)
        {
            _targetNode.onNodeEnter -= callback;
        }


        public void RegisterNodeExitCallback(Action callback)
        {
            _targetNode.onNodeExit += callback;
        }


        public void UnregisterNodeExitCallback(Action callback)
        {
            _targetNode.onNodeExit -= callback;
        }
    }
}