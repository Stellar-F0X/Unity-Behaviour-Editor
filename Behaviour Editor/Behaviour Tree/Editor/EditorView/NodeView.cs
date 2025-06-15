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
        public NodeView(NodeBase targetNode, VisualTreeAsset nodeUxml) : base(AssetDatabase.GetAssetPath(nodeUxml))
        {
            this.targetNode = targetNode;
            this.title = targetNode.name;
            this.tooltip = targetNode.tooltip;
            this.viewDataKey = targetNode.guid;
            this.style.left = targetNode.position.x;
            this.style.top = targetNode.position.y;
            this._lastProcessedCallCount = targetNode.callCount;

            this._elementGroup = this.Q<VisualElement>("group");
            this._nodeBorder = this.Q<VisualElement>("node-border");
            this._nodeTypeLabel = this.Q<TextElement>("node-type-label");

            this.Initialize();
            this.CreatePorts();
        }

        public event Action<NodeView> OnNodeSelected;
        public event Action<NodeView> OnNodeUnselected;

        public readonly NodeBase targetNode;
        
        private readonly VisualElement _elementGroup;
        private readonly VisualElement _nodeBorder;
        private readonly TextElement _nodeTypeLabel;

        public Port inputPort;
        public Port outputPort;
        public Edge parentConnectionEdge;

        private bool _isHighlighted;
        private float _highlightDuration;
        private float _highlightRemainingTime;
        private ulong _lastProcessedCallCount;


        private void Initialize()
        {
            _elementGroup.AddToClassList($"behaviour-node-{targetNode.nodeType}");
            _nodeTypeLabel.text = NodeFactory.ApplySpacing(targetNode.GetType().Name);

            if (Application.isPlaying)
            {
                if (_lastProcessedCallCount > 0)
                {
                    this.SetBorderColorByStatus();
                }
                else
                {
                    _nodeBorder.style.SetBorderColor(Color.gray * 0.3f);
                }
            }
            else
            {
                //NodeBase CustomEditor에서 그려지는 NodeBase의 Name Field를 수정시, 에디터에서 값 변경을 확인 후, 알림이 전달.
                //등록된 TrackPropertyValue에 등록된 람다가 호출되고 변경된 이름이 property.stringValue로 전돨되며 NodeView의 Title도 변경됨.
                SerializedProperty nameProperty = new SerializedObject(targetNode).FindProperty("m_Name");
                this.TrackPropertyValue(nameProperty, p => this.title = p.stringValue);
            }
        }


        public override void OnSelected()
        {
            OnNodeSelected?.Invoke(this);
        }


        public override void OnUnselected()
        {
            OnNodeUnselected?.Invoke(this);
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


#region Highlighting Logic

        public void UpdateView(float deltaTime)
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            // 하이라이트되지 않은 상태이고 노드 호출 횟수가 변경되지 않았다면 업데이트 불필요
            if (_isHighlighted == false && targetNode.callCount == _lastProcessedCallCount)
            {
                return;
            }

            BehaviourTreeEditorSettings settings = BehaviourTreeEditor.Settings;

            if (this.UpdateNodeHighlightState(deltaTime, settings.highlightingDuration))
            {
                float progress = _highlightDuration / settings.highlightingDuration;

                _nodeBorder?.style.SetBorderColor(Color.Lerp(settings.nodeDisappearingColor, settings.nodeAppearingColor, progress));

                parentConnectionEdge?.edgeControl.SetEdgeColor(Color.Lerp(settings.edgeDisappearingColor, settings.edgeAppearingColor, progress));
            }
        }


        /// <summary> 노드의 하이라이트 상태를 업데이트하고 시각적 효과를 관리합니다. </summary>
        /// <param name="deltaTime">프레임 간 경과 시간</param>
        /// <param name="highlightingDuration">하이라이트 효과 지속 시간</param>
        /// <returns>노드 시각적 업데이트가 필요한 경우 true, 그렇지 않으면 false</returns>
        private bool UpdateNodeHighlightState(float deltaTime, float highlightingDuration)
        {
            // 노드가 새로 호출되었는지 확인
            if (targetNode.callCount > _lastProcessedCallCount)
            {
                if (_isHighlighted == false)
                {
                    _isHighlighted = true;
                    parentConnectionEdge?.BringToFront();
                }

                // 처리된 호출 횟수 업데이트 및 하이라이트 시간 초기화
                _lastProcessedCallCount = targetNode.callCount;
                _highlightRemainingTime = highlightingDuration;
            }

            // 하이라이트 효과가 진행 중인 경우
            if (_highlightRemainingTime > 0f)
            {
                // 하이라이트 지속 시간 증가
                _highlightDuration = Mathf.Min(_highlightDuration + deltaTime, highlightingDuration);
                // 남은 하이라이트 시간 감소
                _highlightRemainingTime = Mathf.Max(_highlightRemainingTime - deltaTime, 0f);
            }
            else
            {
                _highlightDuration = Mathf.Max(_highlightDuration - deltaTime, 0f);

                // 하이라이트 효과가 거의 끝났을 때
                if (_highlightDuration < 0.003f)
                {
                    _isHighlighted = false;
                    parentConnectionEdge?.SendToBack();
                    this.SetBorderColorByStatus();
                }
            }

            return _isHighlighted;
        }


        private void SetBorderColorByStatus()
        {
            switch (targetNode.status)
            {
                case NodeBase.EStatus.Failure: _nodeBorder?.style.SetBorderColor(BehaviourTreeEditor.Settings.nodeFailureColor); break;

                case NodeBase.EStatus.Success: _nodeBorder?.style.SetBorderColor(BehaviourTreeEditor.Settings.nodeSuccessColor); break;
            }
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


        //상속받은 상위 클래스에서 Disconnect All이라는 ContextualMenu 생성을 방지하기 위해서 오버라이드
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }
    }
}