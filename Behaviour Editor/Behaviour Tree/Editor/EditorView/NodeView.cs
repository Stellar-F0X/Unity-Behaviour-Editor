using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEditor.UIElements;

namespace BehaviourSystemEditor.BT
{
    public class NodeView : Node
    {
        private enum EIndicatorColor : byte
        {
            White,
            Red,
            Yellow,
            Green
        };

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
            this._nodeTypeLabel = this.Q<TextElement>("node-type-label");
            this._nodeResultIndicator = this.Q<VisualElement>("executed-sign");

            this.Initialize();
            this.CreatePorts();
        }

        private readonly static EIndicatorColor[] _CurrentIndicatorColor =
        {
            EIndicatorColor.Yellow, 
            EIndicatorColor.Red,
            EIndicatorColor.Green
        };

        private readonly static StyleColor[] _CurrentColor =
        {
            new StyleColor(Color.yellow),
            new StyleColor(Color.red),
            new StyleColor(Color.green)
        };

        public event Action<NodeView> OnNodeSelected;
        public event Action<NodeView> OnNodeUnselected;

        private readonly VisualElement _nodeResultIndicator;
        private readonly VisualElement _nodeBorder;
        private readonly TextElement _nodeTypeLabel;
        
        public readonly NodeBase targetNode;

        private bool _isHighlighted;

        private float _highlightDuration;
        private float _highlightRemainingTime;
        private ulong _lastProcessedCallCount;

        private EIndicatorColor _previousColor = EIndicatorColor.White;

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
            _nodeTypeLabel.text = NodeFactory.ApplySpacing(targetNode.GetType().Name);

            if (Application.isPlaying)
            {
                _nodeBorder.style.SetBorderColor(BehaviourTreeEditor.Settings.nodeDisappearingColor);
            }
            else
            {
                //NodeView의 커스텀 타이틀 변경을 위해 SerializedProperty 등록.
                //CustomeEditor에서 그려지는 NodeBase의 Name Field를 수정시, 에디터에서 값 변경을 확인 후, 알림이 전달됨.
                //그러면 등록된 TrackPropertyValue에 등록된 람다가 호출되고 변경된 이름이 property.stringValue로 전돨됨.
                SerializedObject targetObject = new SerializedObject(targetNode);
                SerializedProperty nameProperty = targetObject.FindProperty("m_Name");
                this.TrackPropertyValue(nameProperty, p => this.title = p.stringValue);
            }
        }



        public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
        {
            return new PortView(direction, capacity);
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
            int index = (int)targetNode.status;
            
            if (_previousColor != _CurrentIndicatorColor[index])
            {
                _previousColor = _CurrentIndicatorColor[index];
                _nodeResultIndicator.style.backgroundColor = _CurrentColor[index];
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