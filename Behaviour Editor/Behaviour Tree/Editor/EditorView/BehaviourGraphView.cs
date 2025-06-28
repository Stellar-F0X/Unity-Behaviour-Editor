using System;
using System.Collections.Generic;
using System.Linq;
using BehaviourSystem.BT;
using BehaviourSystem.BT.State;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    [UxmlElement]
    public partial class BehaviourGraphView : GraphView
    {
        public BehaviourGraphView()
        {
            base.Insert(0, new GridBackground());

            this.AddManipulator(new DoubleClick(0.3f));
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new GraphZoomer(2f, 0.2f));

            styleSheets.Add(BehaviourSystemEditor.Settings.behaviourGraphStyle);
        }

        /// <summary>노드가 선택될 때 호출되는 이벤트입니다.</summary>
        public Action<NodeView> onNodeSelected;

        private float _nextUpdateTime;
        private float _lastUpdateTime;

        private GraphAsset _graphAsset;
        private GraphViewProcessor _graphViewProcessor;


        public GraphViewProcessor graphViewProcessor
        {
            get { return _graphViewProcessor; }
        }


        /// <summary>에디터 뷰를 초기화하고 모든 그래프 요소를 제거합니다.</summary>
        public void ClearEditorView()
        {
            base.graphViewChanged -= this.OnGraphViewChanged;
            this.deleteSelection -= this.OnDeleteSelectionElements;

            base.DeleteElements(base.graphElements);
        }


        /// <summary>
        /// 주어진 Behaviour Tree를 그래프 에디터 뷰에 표시합니다.
        /// 노드들과 연결을 생성하고 그룹 데이터를 복원합니다.
        /// </summary>
        public void OnGraphEditorView(GraphAsset changedGraphAsset)
        {
            if (changedGraphAsset is not null)
            {
                this._graphAsset = changedGraphAsset;

                switch (_graphAsset.graphType)
                {
                    case EGraphType.BehaviourTree: _graphViewProcessor = new BehaviourTreeViewProcessor(); break;

                    case EGraphType.StateMachine: _graphViewProcessor = new FiniteStateMachineViewProcessor(); break;
                }

                this.ClearEditorView();

                base.graphViewChanged += this.OnGraphViewChanged;
                this.deleteSelection += this.OnDeleteSelectionElements;
                
                this._graphViewProcessor.CreateAndConnectNodes(this._graphAsset, this);
                this._graphAsset.graphGroup.dataList.ForEach(this.RecreateNodeGroupViewOnLoad);
            }
        }


        /// <summary>입력 포트와 연결 가능한 호환되는 포트들의 목록을 반환합니다.</summary>
        public override List<Port> GetCompatiblePorts(Port input, NodeAdapter nodeAdapter)
        {
            if (input is null)
            {
                Debug.LogWarning($"{typeof(BehaviourGraphView)}: Input is null");
                return null;
            }

            //direction은 input과 output이므로, 다른 노드라도 같은 포트에 못 꽂게 방지
            return ports.Where(output => input.direction != output.direction && input.node != output.node).ToList();
        }


        /// <summary>주어진 노드에 해당하는 NodeView를 찾아 반환합니다.</summary>
        public NodeView FindNodeView(NodeBase node)
        {
            if (node is null || node.guid.IsEmpty())
            {
                return null;
            }

            return this.GetNodeByGuid(node.guid.ToString()) as NodeView;
        }


        public NodeView FindNodeView(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            return this.GetNodeByGuid(guid) as NodeView;
        }
        

        /// <summary>런타임 중 노드 뷰들을 업데이트합니다.</summary>
        public void UpdateNodeView()
        {
            if (Time.time < _nextUpdateTime)
            {
                return;
            }

            float currentTime = Time.time;
            float updateInterval = BehaviourSystemEditor.Settings.nodeViewUpdateInterval;

            foreach (Node view in nodes)
            {
                ((NodeView)view).UpdateView(currentTime - _lastUpdateTime);
            }

            _lastUpdateTime = currentTime;
            _nextUpdateTime = currentTime + updateInterval;
        }

        
#region Mouse Related Events
        /// <summary> 마우스 위치에서 컨텍스트 메뉴(노드 생성) 창을 엽니다. </summary>
        public void OpenContextualMenuWindow(Vector2 mousePosition, Action<NodeView> onNewNodeCreatedOnce = null)
        {
            if (BehaviourSystemEditor.CanEditGraph == false)
            {
                return;
            }

            CreationWindowBase creationWindow = _graphViewProcessor.GetGraphNodeCreationWindow();

            creationWindow.RegisterNodeCreationCallbackOnce(onNewNodeCreatedOnce);

            Vector2 screenPoint = GUIUtility.GUIToScreenPoint(mousePosition);
            SearchWindowContext context = new SearchWindowContext(screenPoint, 200, 240);

            SearchWindow.Open(context, creationWindow);
        }


        /// <summary> 우클릭 시 컨텍스트 메뉴를 구성합니다. </summary>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            this.OpenContextualMenuWindow(evt.mousePosition);
        }


        /// <summary>지정된 노드를 선택합니다.</summary>
        public void SelectNode(NodeView nodeView)
        {
            if (nodeView is null || nodeView.targetNode == null)
            {
                return;
            }

            base.ClearSelection();
            base.AddToSelection(nodeView);
        }
#endregion
        

#region Delete Of Modify Graph Elements
        /// <summary>그래프 뷰가 변경될 때 호출되는 콜백 메서드입니다.</summary>
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove is not null)
            {
                //에디터 뷰에서 삭제된 그래프 뷰 요소를 순회하며 대응되는 노드나 간선, 그룹 등의 데이터를 제거한다.
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    switch (element)
                    {
                        case Edge edge: this.graphViewProcessor.DisconnectNodesByEdge(_graphAsset, edge); break;

                        case NodeView nodeView: this._graphAsset.graph.DeleteNode(nodeView.targetNode); break;

                        case NodeGroupView groupView: this._graphAsset.graphGroup.DeleteGroupData(groupView.data); break;
                    }
                }
            }

            //노드가 생성되거나 이동된 경우, 노드의 위치를 업데이트하고 새롭게 생성된 간선을 연결한다.
            if (graphViewChange.edgesToCreate is not null)
            {
                _graphViewProcessor.ConnectNodesByEdges(_graphAsset, graphViewChange.edgesToCreate);
            }

            //노드의 위치를 업데이트된 경우, BT는 앞의 자식을 먼저 순회하기 때문에 X좌표에 따른 순서를 정렬하여 갱신해준다. 
            if (graphViewChange.movedElements is not null)
            {
                _graphViewProcessor.NotifyNodePositionChanged(graphViewChange.movedElements, this);
            }

            return graphViewChange;
        }


        /// <summary>선택된 요소들을 삭제할 때 호출되는 콜백 메서드입니다.</summary>
        private void OnDeleteSelectionElements(string operationName, AskUser user)
        {
            if (BehaviourSystemEditor.CanEditGraph == false)
            {
                return;
            }

            _graphViewProcessor.OnDeleteSelectionElements(this.selection);

            //DeleteSelection는 내부적으로 Selection 배열을 이용해서 VisualElement들을 제거함.
            //따라서 삭제되면 안되는 요소들만 Selection 배열에서 제거한 뒤, 현재 선택된 요소들(Selection 배열)을 제거하면 됨.
            this.DeleteSelection();
        }
#endregion
        

#region Create Graph Elements
        /// <summary>로딩 시 그룹 데이터로부터 NodeGroupView를 재생성합니다.</summary>
        private void RecreateNodeGroupViewOnLoad(GroupData data)
        {
            NodeGroupView nodeGroupView = new NodeGroupView(_graphAsset.graphGroup, data);

            nodeGroupView.AddElements(nodes.Where(n => n is NodeView v && data.Contains(v.targetNode.guid)));

            nodeGroupView.SetPosition(new Rect(data.position, Vector2.zero));

            base.AddElement(nodeGroupView);
        }
        
        
        /// <summary>새로운 노드를 생성하고 해당하는 NodeView를 반환합니다.</summary>
        public NodeView CreateNewNodeAndView(Type type, Vector2 mousePosition)
        {
            NodeBase node = _graphAsset.graph.CreateNode(type);
            node.position = Vector2Int.CeilToInt(mousePosition);
            
            NodeView nodeView = this._graphViewProcessor.RecreateNodeViewOnLoad(node);
            this.AddNewNodeView(nodeView);
            return nodeView;
        }


        public void AddNewNodeView(NodeView nodeView)
        {
            if (nodeView.targetNode is null)
            {
                Debug.LogError("targetNode of nodeView is null");
                return;
            }
            
            nodeView.OnNodeSelected -= this.onNodeSelected;
            nodeView.OnNodeSelected += this.onNodeSelected;
            
            this.AddElement(nodeView);
        }


        /// <summary>새로운 노드 그룹 뷰를 생성하고 반환합니다.</summary>
        public NodeGroupView CreateNewNodeGroupView(string title, Vector2 position)
        {
            GroupData nodeGroupData = _graphAsset.graphGroup.CreateGroupData(title, position);
            NodeGroupView groupView = new NodeGroupView(_graphAsset.graphGroup, nodeGroupData);

            groupView.SetPosition(new Rect(position, Vector2.zero));
            groupView.style.backgroundColor = BehaviourSystemEditor.Settings.nodeGroupColor;
            groupView.title = title;

            base.AddElement(groupView);
            return groupView;
        }
#endregion
    }
}
