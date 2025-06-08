using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using System.Linq;
using BehaviourSystem.BT;

namespace BehaviourSystemEditor.BT
{
    [UxmlElement]
    public partial class BehaviourTreeView : GraphView
    {
        public BehaviourTreeView()
        {
            base.Insert(0, new GridBackground());

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContentZoomer()
            {
                maxScale = BehaviourTreeEditor.Settings.maxZoomScale,
                minScale = BehaviourTreeEditor.Settings.minZoomScale
            });

            styleSheets.Add(BehaviourTreeEditor.Settings.behaviourTreeStyle);
        }

        public Action<NodeView> onNodeSelected;

        private float _nextUpdateTime;
        private float _lastUpdateTime;

        private BehaviourTree _tree;
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
                    //1. Undo로 생성이 취소된 노드를 여기서 처리.
                    //2. Graph에 만들어졌지만 클래스를 삭제당한 노드도 삭제.
                    if (tree.nodeSet.nodeList[i] is null)
                    {
                        tree.nodeSet.nodeList.RemoveAt(i--);
                    }
                }
                
                //트리 구조라서 미리 모두 생성해둬야 자식과 부모를 연결 할 수 있음.
                tree.nodeSet.nodeList.ForEach(n => this.RecreateNodeViewOnLoad(n));
                tree.nodeSet.nodeList.ForEach(n => NodeLinkHelper.CreateVisualEdgesFromNodeData(this, n, n as IBehaviourIterable));
                
                tree.groupDataSet?.dataList.ForEach(groupData => this.RecreateNodeGroupViewOnLoad(groupData));
            }
        }
        
        
        
        public override List<Port> GetCompatiblePorts(Port input, NodeAdapter nodeAdapter)
        {
            if (input is null)
            {
                Debug.LogWarning($"{typeof(BehaviourTreeView)}: Input is null");
                return null;
            }

            //direction은 input과 output이므로, 다른 노드라도 같은 포트에 못 꽂게 방지
            return ports.Where(output => input.direction != output.direction && input.node != output.node).ToList();
        }
        


        public NodeView FindNodeView(NodeBase node)
        {
            if (node is null || node.guid is null)
            {
                return null;
            }

            return this.GetNodeByGuid(node.guid) as NodeView;
        }


        public void OpenContextualMenuWindow(Vector2 mousePosition, Action<NodeView> onNewNodeCreatedOnce = null)
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

            _creationWindow.RegisterNodeCreationCallbackOnce(onNewNodeCreatedOnce);

            Vector2 screenPoint = GUIUtility.GUIToScreenPoint(mousePosition);
            SearchWindowContext context = new SearchWindowContext(screenPoint, 200, 240);

            SearchWindow.Open(context, _creationWindow);
        }


        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            this.OpenContextualMenuWindow(evt.mousePosition);
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
                _nextUpdateTime = currentTime + BehaviourTreeEditor.Settings.nodeViewUpdateInterval;

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
                        case Edge edge: NodeLinkHelper.RemoveEdgeAndNodeConnection(_tree.nodeSet, edge); break;

                        case NodeView nodeView: this._tree.nodeSet.DeleteNode(nodeView.targetNode); break;

                        case NodeGroupView groupView: this._tree.groupDataSet.DeleteGroupData(groupView.data); break;
                    }
                }
            }

            if (graphViewChange.edgesToCreate is not null)
            {
                NodeLinkHelper.UpdateNodeDataFromVisualEdges(_tree.nodeSet, graphViewChange.edgesToCreate);
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
                if (selection[i] is NodeView view && view.targetNode.nodeType == NodeBase.ENodeType.Root)
                {
                    view.selected = false;
                    selection.RemoveAt(i);
                    break;
                }
            }

            //DeleteSelection는 내부적으로 Selection 배열을 이용해서 VisualElement들을 제거함.
            //따라서 삭제되면 안되는 요소들만 Selection 배열에서 제거한 뒤, 현재 선택된 요소들(Selection 배열)을 제거하면 됨.
            this.DeleteSelection();
        }


        private NodeView RecreateNodeViewOnLoad(NodeBase node)
        {
            if (node is null)
            {
                return null;
            }

            NodeView nodeView = new NodeView(node, BehaviourTreeEditor.Settings.nodeViewXml);
            nodeView.OnNodeSelected += this.onNodeSelected;

            base.AddElement(nodeView); //nodes라는 GraphElement 컨테이너에 추가.
            return nodeView;
        }


        private void RecreateNodeGroupViewOnLoad(GroupData data)
        {
            NodeGroupView nodeGroupView = new NodeGroupView(_tree.groupDataSet, data);
            
            nodeGroupView.AddElements(nodes.Where(n => n is NodeView v && data.Contains(v.targetNode.guid)));
            nodeGroupView.SetPosition(new Rect(data.position, Vector2.zero));
            
            base.AddElement(nodeGroupView);
        }
    }
}