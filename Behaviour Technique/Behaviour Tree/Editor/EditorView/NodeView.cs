using System;
using System.Reflection;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node, INodeViewDeletable
    {
        public NodeView(Node node, VisualTreeAsset nodeUxml) : base(AssetDatabase.GetAssetPath(nodeUxml))
        {
            this.node = node;
            this.title = node.name;
            this.viewDataKey = node.guid;

            this.style.left = node.position.x;
            this.style.top = node.position.y;

            _nodeBorder = this.Q<VisualElement>("node-border");

            string nodeType = node.baseType.ToString();
            _nodeType = node.GetType();

            this.AddToClassList(nodeType.ToLower());
            this.CreatePorts();
        }


        public event Action<NodeView> OnNodeSelected;
        public event Action<NodeView> OnNodeUnselected;
        public event Action<NodeView> OnNodeDeleted;

        public Node node;
        public Port input;
        public Port output;

        private readonly Type _nodeType;
        private readonly VisualElement _nodeBorder;

        private readonly Color _runningColor = new Color32(54, 154, 204, 255);
        private readonly Color _doneColor = new Color32(24, 93, 125, 255);



        public override void OnSelected() => OnNodeSelected?.Invoke(this);

        public override void OnUnselected() => OnNodeUnselected?.Invoke(this);


        private void CreatePorts()
        {
            switch (node.baseType)
            {
                case Node.ENodeType.Root:
                    output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    break;

                case Node.ENodeType.Action:
                    input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    break;

                case Node.ENodeType.Composite:
                    input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    break;

                case Node.ENodeType.Decorator:
                    input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    break;
            }

            this.SetupPort(input, string.Empty, FlexDirection.Column, base.inputContainer);
            this.SetupPort(output, string.Empty, FlexDirection.ColumnReverse, base.outputContainer);
        }


        private void SetupPort(Port port, string portName, FlexDirection direction, VisualElement container)
        {
            if (port != null)
            {
                port.style.flexDirection = direction;
                port.portName = portName;
                container.Add(port);
            }
        }



        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            Undo.RecordObject(node, "Behaviour Tree (Set Position)");

            node.position.x = newPos.xMin;
            node.position.y = newPos.yMin;

            UnityEditor.EditorUtility.SetDirty(node);
        }


        public void UpdateState()
        {
            if (Application.isPlaying)
            {
                if (node.started)
                {
                    _nodeBorder.style.borderBottomColor = _runningColor;
                    _nodeBorder.style.borderLeftColor = _runningColor;
                    _nodeBorder.style.borderRightColor = _runningColor;
                    _nodeBorder.style.borderTopColor = _runningColor;
                }
                else
                {
                    _nodeBorder.style.borderBottomColor = _doneColor;
                    _nodeBorder.style.borderLeftColor = _doneColor;
                    _nodeBorder.style.borderRightColor = _doneColor;
                    _nodeBorder.style.borderTopColor = _doneColor;
                }
            }
        }


        public void SortChildren()
        {
            if (this.node.baseType != Node.ENodeType.Composite)
            {
                return;
            }

            if (node is CompositeNode compositeNode)
            {
                compositeNode.children.Sort((l, r) => l.position.x < r.position.x ? -1 : 1);
            }
        }


        /// <summary>
        /// BehaviourTreeView의 DeleteEventDetector에서 선택된 노드가 제거될때 호출된다.
        /// </summary>
        /// <param name="actor"> 현재 Editor에 그려진 BehaviourTree가 등록된 Actor </param>
        public void OnNodeDeletedEvent(BehaviourActor actor)
        {
            foreach (FieldInfo fieldInfo in _nodeType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (fieldInfo.FieldType == typeof(BehaviourTreeEvent) && fieldInfo.GetValue(node) is BehaviourTreeEvent eventValue)
                {
                    actor?.RemoveBehaviourEvent(eventValue.key);
                }
            }

            OnNodeDeleted?.Invoke(this);
        }
    }
}
