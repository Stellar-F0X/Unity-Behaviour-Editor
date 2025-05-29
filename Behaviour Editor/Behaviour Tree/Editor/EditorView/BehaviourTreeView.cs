using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using System.Linq;
using BehaviourSystem.BT;
using UnityEditor.UIElements;

namespace BehaviourSystemEditor.BT
{
    [UxmlElement]
    public partial class BehaviourTreeView : GraphView
    {
        public BehaviourTreeView()
        {
            base.Insert(0, new GridBackground());

            ContentZoomer zoomer = new ContentZoomer()
            {
                maxScale = BehaviourTreeEditor.Settings.maxZoomScale,
                minScale = BehaviourTreeEditor.Settings.minZoomScale,
            };

            this.AddManipulator(zoomer);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.UnregisterCallback<GeometryChangedEvent>(this.OnGraphViewGeometryChanged);
            this.RegisterCallback<GeometryChangedEvent>(this.OnGraphViewGeometryChanged);

            styleSheets.Add(BehaviourTreeEditor.Settings.behaviourTreeStyle);

            _nodeEdgeHandler = new NodeEdgeHandler();
            _nodeSearchHelper = new NodeSearchHelper();
        }

        public Action<NodeView> onNodeSelected;
        public ToolbarPopupSearchField popupSearchField;

        private float _nextUpdateTime;
        private float _lastUpdateTime;

        private MiniMap _miniMap;
        private BehaviourTree _tree;
        private NodeSearchHelper _nodeSearchHelper;
        private NodeEdgeHandler _nodeEdgeHandler;
        private CreationWindow _creationWindow;


        public void ClearEditorView()
        {
            graphViewChanged -= OnGraphViewChanged;
            base.DeleteElements(graphElements);
        }


        public void OnGraphEditorView(BehaviourTree tree)
        {
            if (tree is not null)
            {
                this._tree = tree;

                graphViewChanged -= this.OnGraphViewChanged;
                this.deleteSelection -= this.OnDeleteSelectionElements;

                base.DeleteElements(base.graphElements);

                graphViewChanged += this.OnGraphViewChanged;
                this.deleteSelection += this.OnDeleteSelectionElements;

                for (int i = 0; i < tree.nodeSet.nodeList.Count; ++i)
                {
                    //Undo로 생성이 취소된 노드를 여기서 처리.
                    if (tree.nodeSet.nodeList[i] is null)
                    {
                        tree.nodeSet.nodeList.RemoveAt(i);
                    }
                }

                //트리 구조라서 미리 모두 생성해둬야 자식과 부모를 연결 할 수 있음.
                tree.nodeSet.nodeList.ForEach(n => this.RecreateNodeViewOnLoad(n));
                tree.groupDataSet?.dataList.ForEach(d => this.RecreateNodeGroupViewOnLoad(d));
                tree.nodeSet.nodeList.ForEach(n => _nodeEdgeHandler.ConnectEdges(this, n, n as IBehaviourIterable));
            }
        }


        public NodeView FindNodeView(NodeBase node)
        {
            if (node is null || node.guid is null)
            {
                return null;
            }

            return this.GetNodeByGuid(node.guid) as NodeView;
        }


        public override List<Port> GetCompatiblePorts(Port input, NodeAdapter nodeAdapter)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            //direction은 input과 output이므로, 다른 노드라도 같은 포트에 못 꽂게 방지
            return ports.Where(output => input.direction != output.direction && input.node != output.node).ToList();
        }


        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (BehaviourTreeEditor.CanEditTree == false)
            {
                return;
            }

            if (_creationWindow is null)
            {
                _creationWindow = ScriptableObject.CreateInstance<CreationWindow>();
                _creationWindow.Initialize(this);
            }

            Vector2 screenPoint = GUIUtility.GUIToScreenPoint(evt.mousePosition);
            SearchWindowContext context = new SearchWindowContext(screenPoint, 200, 240);

            SearchWindow.Open(context, _creationWindow);
        }


        public void SelectNode(NodeView nodeView)
        {
            base.ClearSelection();

            if (nodeView != null)
            {
                base.AddToSelection(nodeView);
            }
        }


        public NodeView CreateNewNodeAndView(Type type, Vector2 mousePosition)
        {
            NodeBase node = _tree.nodeSet.CreateNode(type);
            node.position = mousePosition;
            return this.RecreateNodeViewOnLoad(node);
        }


        public NodeGroupView CreateNewNodeGroupView(string title, Vector2 position)
        {
            GroupData nodeGroupData = _tree.groupDataSet.CreateGroupData(title, position);
            NodeGroupView groupView = new NodeGroupView(_tree.groupDataSet, nodeGroupData);

            groupView.SetPosition(new Rect(position, Vector2.zero));
            groupView.style.backgroundColor = BehaviourTreeEditor.Settings.nodeGroupColor;
            groupView.title = title;

            base.AddElement(groupView);
            return groupView;
        }
        

        public void UpdateNodeView()
        {
            if (Time.time > _nextUpdateTime)
            {
                float currentTime = Time.time;
                float actualDeltaTime = currentTime - _lastUpdateTime;
                
                _lastUpdateTime = currentTime;
                _nextUpdateTime = currentTime + BehaviourTreeEditor.Settings.editorUpdateInterval;
                
                foreach (Node view in nodes)
                {
                    ((NodeView)view)!.UpdateView(actualDeltaTime);
                }
            }
        }


        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove is not null)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    switch (element)
                    {
                        case Edge edge: this._nodeEdgeHandler.DeleteEdges(_tree, edge); break;

                        case NodeView nodeView: this._tree.nodeSet.DeleteNode(nodeView.node); break;

                        case NodeGroupView groupView: this._tree.groupDataSet.DeleteGroupData(groupView.data); break;
                    }
                }
            }

            if (graphViewChange.edgesToCreate is not null)
            {
                this._nodeEdgeHandler.ConnectEdges(_tree, graphViewChange.edgesToCreate);
            }

            if (graphViewChange.movedElements is not null)
            {
                base.nodes.ForEach(n => (n as NodeView)?.SortChildren());
            }

            return graphViewChange;
        }


        private void OnDeleteSelectionElements(string operationName, AskUser user)
        {
            if (BehaviourTreeEditor.CanEditTree == false)
            {
                return;
            }

            for (int i = 0; i < selection.Count; ++i)
            {
                if (selection[i] is NodeView view && view.node.nodeType == NodeBase.ENodeType.Root)
                {
                    view.selected = false;
                    selection.RemoveAt(i);
                    break;
                }
            }

            //DeleteSelection는 내부적으로 Selection 배열을 이용해서 VisualElement들을 제거함.
            this.DeleteSelection();
        }


        private NodeView RecreateNodeViewOnLoad(NodeBase node)
        {
            NodeView nodeView = new NodeView(node, BehaviourTreeEditor.Settings.nodeViewXml);
            nodeView.OnNodeSelected += this.onNodeSelected;

            base.AddElement(nodeView); //nodes라는 GraphElement 컨테이너에 추가.
            return nodeView;
        }


        private void RecreateNodeGroupViewOnLoad(GroupData data)
        {
            NodeGroupView nodeGroupView = new NodeGroupView(_tree.groupDataSet, data);
            base.AddElement(nodeGroupView);

            nodeGroupView.schedule.Execute(() =>
            {
                nodeGroupView.SetPosition(new Rect(data.position, Vector2.zero));
                nodeGroupView.AddElements(nodes.Where(n => n is NodeView v && data.Contains(v.node.guid)));
            });
        }


        private void OnGraphViewGeometryChanged(GeometryChangedEvent evt)
        {
            if (_miniMap is null)
            {
                _miniMap = new MiniMap();
                _miniMap.anchored = true;
                _miniMap.style.backgroundColor = BehaviourTreeEditor.Settings.miniMapBackgroundColor;
                this.Add(_miniMap);
            }

            if (evt.newRect.width >= 280 && evt.newRect.height >= 240)
            {
                _miniMap.visible = true;
                _miniMap.enabledSelf = true;

                _miniMap.SetPosition(new Rect(evt.newRect.width - 240, evt.newRect.height - 200, 220, 180));
            }
            else
            {
                _miniMap.visible = false;
                _miniMap.enabledSelf = false;
            }
        }


        public void SearchNodeByNameOrTag(ChangeEvent<string> changeEvent)
        {
            if (_nodeSearchHelper.HasSyntaxes(changeEvent.newValue, out var syntaxes))
            {
                popupSearchField.menu.ClearItems();

                NodeView[] views = null;

                if (syntaxes.Length == 1)
                {
                    views = _nodeSearchHelper.GetNodeView(syntaxes[0], NodeSearchHelper.ESearchOptions.Both, nodes);
                }
                else if (string.Compare(syntaxes[0], "t:", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    views = _nodeSearchHelper.GetNodeView(syntaxes[1], NodeSearchHelper.ESearchOptions.Tag, nodes);
                }
                else if (string.Compare(syntaxes[0], "n:", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    views = _nodeSearchHelper.GetNodeView(syntaxes[1], NodeSearchHelper.ESearchOptions.Name, nodes);
                }

                if (views is null)
                {
                    return;
                }

                for (int i = 0; i < views.Length; ++i)
                {
                    NodeView view = views[i];

                    string menuName = $"[{i + 1}]   name: [{view.node.name}]   tag: [{view.node.tag}]";

                    popupSearchField.menu.AppendAction(menuName, delegate
                    {
                        this.SelectNode(view);
                        base.FrameSelection();
                    });
                }

                popupSearchField.ShowMenu();
            }
        }
    }
}