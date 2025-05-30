using System;
using System.Collections.Generic;
using BehaviourSystem.BT;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;

namespace BehaviourSystemEditor.BT
{
    public class NodeView : Node
    {
        public NodeView(NodeBase node, VisualTreeAsset nodeUxml) : base(AssetDatabase.GetAssetPath(nodeUxml))
        {
            this.node = node;
            this.title = node.name;
            this.tooltip = node.tooltip;
            this.viewDataKey = node.guid;
            this.style.left = node.position.x;
            this.style.top = node.position.y;

            _lastRenderedNodeCount = node.callCount;
            
            _nodeBorder = this.Q<VisualElement>("node-border");
            _executedResultSign = this.Q<VisualElement>("executed-sign");
            
            if (Application.isPlaying)
            {
                _nodeBorder.style.borderTopColor = BehaviourTreeEditor.Settings.nodeDisappearingColor;
                _nodeBorder.style.borderBottomColor = BehaviourTreeEditor.Settings.nodeDisappearingColor;
                _nodeBorder.style.borderLeftColor = BehaviourTreeEditor.Settings.nodeDisappearingColor;
                _nodeBorder.style.borderRightColor = BehaviourTreeEditor.Settings.nodeDisappearingColor;
            }

            this.AddToClassList(node.nodeType.ToString().ToLower());
            this.CreatePorts();
        }

        public event Action<NodeView> OnNodeSelected;
        public event Action<NodeView> OnNodeUnselected;


        private float _elapsedTime;
        private float _minimumStayTime;
        private bool _isInHighlighting;
        private ulong _lastRenderedNodeCount = 0;

        
        public NodeBase node;
        public Edge toParentEdge;

        public Port input;
        public Port output;

        private VisualElement _executedResultSign;
        private VisualElement _nodeBorder;
        private VisualElement _inputPortView;
        private VisualElement _outputPortView;


        public override void OnSelected() => OnNodeSelected?.Invoke(this);

        public override void OnUnselected() => OnNodeUnselected?.Invoke(this);


        private void CreatePorts()
        {
            switch (node.nodeType)
            {
                case NodeBase.ENodeType.Root:
                    output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    _outputPortView = output.Q<VisualElement>("cap");
                    break;

                case NodeBase.ENodeType.Action:
                    input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    _inputPortView = input.Q<VisualElement>("cap");
                    break;

                case NodeBase.ENodeType.Composite:
                    input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    _inputPortView = input.Q<VisualElement>("cap");
                    _outputPortView = output.Q<VisualElement>("cap");
                    break;

                case NodeBase.ENodeType.Decorator:
                    input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    _inputPortView = input.Q<VisualElement>("cap");
                    _outputPortView = output.Q<VisualElement>("cap");
                    break;
            }

            this.SetupPort(input, string.Empty, FlexDirection.Column, base.inputContainer);
            this.SetupPort(output, string.Empty, FlexDirection.ColumnReverse, base.outputContainer);
        }


        private void SetupPort(Port port, string portName, FlexDirection direction, VisualElement container)
        {
            if (port is not null)
            {
                port.pickingMode = BehaviourTreeEditor.CanEditTree ? PickingMode.Position : PickingMode.Ignore;
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

            EditorUtility.SetDirty(node);
        }


        public void SortChildren()
        {
            if (this.node.nodeType != NodeBase.ENodeType.Composite)
            {
                return;
            }

            if (node is CompositeNode compositeNode)
            {
                compositeNode.children.Sort((l, r) => l.position.x < r.position.x ? -1 : 1);
            }
        }


        //상속받은 상위 클래스에서 Disconnect All이라는 ContextualMenu 생성을 방지하기 위해서 오버라이드
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }



        public void UpdateView(float deltaTime)
        {
            if (Application.isPlaying)
            {
                if (node.callCount > _lastRenderedNodeCount)
                {
                    if (_isInHighlighting == false)
                    {
                        _isInHighlighting = true;
                        toParentEdge?.BringToFront();
                    }
                    
                    _lastRenderedNodeCount = node.callCount;
                    _minimumStayTime = BehaviourTreeEditor.Settings.minimumFocusingDuration;
                }

                if (_minimumStayTime > 0f)
                {
                    _elapsedTime = Mathf.Min(_elapsedTime + deltaTime, BehaviourTreeEditor.Settings.minimumFocusingDuration);
                    _minimumStayTime = Mathf.Max(_minimumStayTime - deltaTime, 0f);
                }
                else if (Mathf.Approximately(this._elapsedTime, 0f))
                {
                    if (_isInHighlighting)
                    {
                        _isInHighlighting = false;
                        toParentEdge?.SendToBack();
                    }
                    
                    return;
                }
                else
                {
                    _elapsedTime = Mathf.Max(_elapsedTime - deltaTime, 0f);
                }

                switch (node.behaviourResult)
                {
                    case NodeBase.EBehaviourResult.Running: _executedResultSign.style.backgroundColor = new StyleColor(Color.yellow); break;

                    case NodeBase.EBehaviourResult.Failure: _executedResultSign.style.backgroundColor = new StyleColor(Color.red); break;

                    case NodeBase.EBehaviourResult.Success: _executedResultSign.style.backgroundColor = new StyleColor(Color.green); break;
                }

                float progress = this._elapsedTime / BehaviourTreeEditor.Settings.minimumFocusingDuration;

                Color borderColor = Color.Lerp(BehaviourTreeEditor.Settings.nodeDisappearingColor, BehaviourTreeEditor.Settings.nodeAppearingColor, progress);
                Color portColor = Color.Lerp(BehaviourTreeEditor.Settings.portDisappearingColor, BehaviourTreeEditor.Settings.portAppearingColor, progress);
                Color edgeColor = Color.Lerp(BehaviourTreeEditor.Settings.edgeDisappearingColor, BehaviourTreeEditor.Settings.edgeAppearingColor, progress);

                if (_nodeBorder != null)
                {
                    _nodeBorder.style.borderTopColor = borderColor;
                    _nodeBorder.style.borderBottomColor = borderColor;
                    _nodeBorder.style.borderLeftColor = borderColor;
                    _nodeBorder.style.borderRightColor = borderColor;
                }

                if (toParentEdge != null)
                {
                    toParentEdge.edgeControl.inputColor = edgeColor;
                    toParentEdge.edgeControl.outputColor = edgeColor;
                }

                if (_inputPortView != null)
                {
                    _inputPortView.style.backgroundColor = portColor;
                }

                if (_outputPortView != null)
                {
                    _outputPortView.style.backgroundColor = portColor;
                }
            }
        }
    }
}