using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using BehaviourSystem.BT;
using UnityEditor;

namespace BehaviourSystemEditor.BT
{
    public class NodeView : Node
    {
        public NodeView(NodeBase targetNode, VisualTreeAsset nodeUxml) : base(AssetDatabase.GetAssetPath(nodeUxml))
        {
            this.targetNode = targetNode;
            this.title = targetNode.name;
            this.tooltip = targetNode.tooltip;
            this.viewDataKey = targetNode.guid;
            this.style.left = targetNode.position.x;
            this.style.top = targetNode.position.y;
            this._lastProcessedCallCount = targetNode.callCount;
            
            this._nodeBorder = this.Q<VisualElement>("node-border");
            this._nodeResultIndicator = this.Q<VisualElement>("executed-sign");

            this.Initialize();
            this.CreatePorts();
        }

        public event Action<NodeView> OnNodeSelected;
        public event Action<NodeView> OnNodeUnselected;

        private readonly VisualElement _nodeResultIndicator;
        private readonly VisualElement _nodeBorder;

        public readonly NodeBase targetNode;

        private bool _isInitializedColor;
        private bool _isHighlighted;

        private float _highlightDuration;
        private float _highlightRemainingTime;
        private ulong _lastProcessedCallCount;

        private NodeBase.EBehaviourResult _previousFrameResult;

        public Edge parentConnectionEdge;
        public Port inputPort;
        public Port outputPort;


        public override void OnSelected()
        {
            OnNodeSelected?.Invoke(this); 
        }
        
        
        
        public override void OnUnselected()
        {
            OnNodeUnselected?.Invoke(this);
        }


        
        private void Initialize()
        {
            _nodeBorder.AddToClassList($"behaviour-node-{targetNode.nodeType}");

            if (Application.isPlaying)
            {
                _nodeBorder.style.SetBorderColor(BehaviourTreeEditor.Settings.nodeDisappearingColor);
            }
        }



        private void CreatePorts()
        {
            switch (targetNode.nodeType)
            {
                case NodeBase.ENodeType.Root:
                {
                    outputPort = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    break;
                }

                case NodeBase.ENodeType.Action:
                {
                    inputPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    break;
                }

                case NodeBase.ENodeType.Composite:
                {
                    inputPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    outputPort = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    break;
                }

                case NodeBase.ENodeType.Decorator:
                {
                    inputPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    outputPort = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    break;
                }
            }

            this.SetupPort(inputPort, string.Empty, FlexDirection.Column, base.inputContainer);
            this.SetupPort(outputPort, string.Empty, FlexDirection.ColumnReverse, base.outputContainer);
        }


        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            Undo.RecordObject(targetNode, "Behaviour Tree (Set Position)");

            targetNode.position.x = newPos.xMin;
            targetNode.position.y = newPos.yMin;

            EditorUtility.SetDirty(targetNode);
        }


        public void SortChildren()
        {
            if (this.targetNode.nodeType != NodeBase.ENodeType.Composite)
            {
                return;
            }

            if (targetNode is CompositeNode compositeNode)
            {
                compositeNode.children.Sort((l, r) => l.position.x < r.position.x ? -1 : 1);
            }
        }


        //상속받은 상위 클래스에서 Disconnect All이라는 ContextualMenu 생성을 방지하기 위해서 오버라이드
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }


#region Highlighting Logic
        public void UpdateView(float deltaTime)
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            BehaviourTreeEditorSettings settings = BehaviourTreeEditor.Settings;

            if (this.UpdateNodeHighlightState(deltaTime, settings.highlightingDuration))
            {
                this.UpdateNodeVisuals(settings, _highlightDuration / settings.highlightingDuration);
            }
        }


        
        private bool UpdateNodeHighlightState(float deltaTime, float highlightingDuration)
        {
            if (_isHighlighted == false && targetNode.callCount == _lastProcessedCallCount)
            {
                return false;
            }

            if (targetNode.callCount > _lastProcessedCallCount)
            {
                if (_isHighlighted == false)
                {
                    _isHighlighted = true;
                    parentConnectionEdge?.BringToFront();
                }

                _lastProcessedCallCount = targetNode.callCount;
                _highlightRemainingTime = highlightingDuration;
            }

            if (_highlightRemainingTime > 0f)
            {
                _highlightDuration = Mathf.Min(_highlightDuration + deltaTime, highlightingDuration);
                _highlightRemainingTime = Mathf.Max(_highlightRemainingTime - deltaTime, 0f);
                return true;
            }

            _highlightDuration = Mathf.Max(_highlightDuration - deltaTime, 0f);

            if (_highlightDuration < 0.005f)
            {
                _isHighlighted = false;
                parentConnectionEdge?.SendToBack();
                return true;
            }

            return _isHighlighted;
        }
        


        private void UpdateNodeVisuals(BehaviourTreeEditorSettings settings, float progress)
        {
            if (targetNode.behaviourResult != _previousFrameResult || _isInitializedColor == false)
            {
                _isInitializedColor = true;
                
                switch (targetNode.behaviourResult)
                {
                    case NodeBase.EBehaviourResult.Running:
                    {
                        _nodeResultIndicator.style.backgroundColor = new StyleColor(Color.yellow);
                        _previousFrameResult = NodeBase.EBehaviourResult.Running;
                        break;
                    }
                    case NodeBase.EBehaviourResult.Failure:
                    {
                        _nodeResultIndicator.style.backgroundColor = new StyleColor(Color.red);
                        _previousFrameResult = NodeBase.EBehaviourResult.Failure;
                        break;
                    }
                    case NodeBase.EBehaviourResult.Success:
                    {
                        _nodeResultIndicator.style.backgroundColor = new StyleColor(Color.green);
                        _previousFrameResult = NodeBase.EBehaviourResult.Success;
                        break;
                    }
                }
            }
            
            _nodeBorder?.style.SetBorderColor(Color.Lerp(settings.nodeDisappearingColor, settings.nodeAppearingColor, progress));
            
            parentConnectionEdge?.edgeControl.SetEdgeColor(Color.Lerp(settings.edgeDisappearingColor, settings.edgeAppearingColor, progress));
        }
#endregion
        

        private void SetupPort(Port port, string portName, FlexDirection direction, VisualElement container)
        {
            if (port is null)
            {
                return;
            }

            port.pickingMode = BehaviourTreeEditor.CanEditTree ? PickingMode.Position : PickingMode.Ignore;
            port.style.flexDirection = direction;
            port.portName = portName;
            container.Add(port);
        }
    }
}