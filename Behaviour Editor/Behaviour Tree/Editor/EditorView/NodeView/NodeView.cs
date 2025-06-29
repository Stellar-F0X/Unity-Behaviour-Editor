using System;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    public abstract class NodeView : Node
    {
        public NodeView(NodeBase targetNode, VisualTreeAsset nodeUxml) : base(AssetDatabase.GetAssetPath(nodeUxml))
        {
            this.targetNode = targetNode;
            this.title = targetNode.name;
            this.tooltip = targetNode.tooltip;
            this.viewDataKey = targetNode.guid.ToString();
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

        protected readonly VisualElement _elementGroup;
        protected readonly VisualElement _nodeBorder;
        protected readonly TextElement _nodeTypeLabel;

        public Port inputPort;
        public Port outputPort;
        public Edge parentConnectionEdge;

        private bool _isHighlighted;
        private float _highlightDuration;
        private float _highlightRemainingTime;
        private ulong _lastProcessedCallCount;


        protected virtual void Initialize()
        {
            _nodeTypeLabel.text = NodeFactory.ApplySpacing(targetNode.GetType().Name);

            if (Application.isPlaying)
            {
                if (_lastProcessedCallCount > 0)
                {
                    this.SetBorderColorByStatus();
                }
                else
                {
                    this.SetBorderColor(style, Color.gray * 0.3f);
                }
            }
            else
            {
                //NodeBase CustomEditor에서 그려지는 NodeBase의 Name Field를 수정시, 에디터에서 값 변경을 확인 후, 알림이 전달.
                //등록된 TrackPropertyValue에 등록된 람다가 호출되고 변경된 이름이 property.stringValue로 전돨되며 NodeView의 Title도 변경됨.
                SerializedProperty nameProp = new SerializedObject(targetNode).FindProperty("m_Name");
                
                this.TrackPropertyValue(nameProp, delegate(SerializedProperty p)
                {
                    if (targetNode is ISubGraphNode subGraphNode)
                    {
                        subGraphNode.subGraphAsset.name = p.stringValue;
                    }

                    this.title = p.stringValue;
                });
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


        public void SetBorderColor(IStyle elementStyle, Color color)
        {
            elementStyle.borderTopColor = color;
            elementStyle.borderBottomColor = color;
            elementStyle.borderLeftColor = color;
            elementStyle.borderRightColor = color;
        }


        public void SetEdgeColor(EdgeControl control, Color color)
        {
            control.inputColor = color;
            control.outputColor = color;
        }


        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            Undo.RecordObject(targetNode, "Behaviour System (Set Position)");

            targetNode.position.x = Mathf.RoundToInt(newPos.xMin);
            targetNode.position.y = Mathf.RoundToInt(newPos.yMin);

            EditorUtility.SetDirty(targetNode);
        }


        protected void SetupPort(Port port, string portName, FlexDirection direction, VisualElement container)
        {
            if (port is null)
            {
                return;
            }

            port.pickingMode = BehaviorEditor.canEditGraph ? PickingMode.Position : PickingMode.Ignore;
            port.style.flexDirection = direction;
            port.portName = portName;
            container.Add(port);
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

            BehaviourTreeEditorSettings settings = BehaviorEditor.settings;

            if (this.UpdateNodeHighlightState(deltaTime, settings.nodeViewHighlightingDuration))
            {
                float progress = _highlightDuration / settings.nodeViewHighlightingDuration;

                this.SetBorderColor(style, settings.nodeStatusLinearColor.Evaluate(progress));

                this.SetEdgeColor(this.parentConnectionEdge.edgeControl, settings.edgeStatusLinearColor.Evaluate(progress));
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


        protected virtual void SetBorderColorByStatus() { }

#endregion

        //NodeView에 포트를 생성합니다.
        protected abstract void CreatePorts();

        //상속받은 상위 클래스에서 Disconnect All이라는 ContextualMenu 생성을 방지하기 위해서 오버라이드
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }
    }
}