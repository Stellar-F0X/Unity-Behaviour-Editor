using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using BehaviourTechnique.BehaviourTreeEditor.Setting;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using BehaviourTechnique.UIElements;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public class BehaviourTreeView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<BehaviourTreeView, UxmlTraits>
        {
        }

        public BehaviourTreeView()
        {
            Insert(0, new GridBackground());

            ContentZoomer zoomer = new ContentZoomer() {
                maxScale = BehaviourTreeEditorWindow.Settings.enlargementScale,
                minScale = BehaviourTreeEditorWindow.Settings.reductionScale
            };

            this.AddManipulator(zoomer);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            styleSheets.Add(BehaviourTreeEditorWindow.BehaviourTreeStyle);

            _nodeEdgeHandler = new NodeEdgeHandler();
            _deleteEventDetector = new DeleteEventDetector();

            Undo.undoRedoPerformed = () => {
                this.OnGraphEditorView(_tree);
                AssetDatabase.SaveAssets();
            };
        }


        public Action onGraphViewChange;
        public Action<NodeView> onNodeSelected;

        private BehaviourTree _tree;
        private NodeEdgeHandler _nodeEdgeHandler;
        private NodeCreationWindow _nodeCreationWindow;
        private DeleteEventDetector _deleteEventDetector;


        public void OnGraphEditorView(BehaviourTree tree)
        {
            if (tree == null)
            {
                return;
            }

            this._tree = tree;
            this.Intialize(tree);
            this.IntegrityCheckNodeList(tree);
        }


        public NodeView FindNodeView(Node node)
        {
            if (node == null || node.guid == null)
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
            if (!BehaviourTreeEditorWindow.Instance.Editable)
            {
                return;
            }

            if (_nodeCreationWindow == null)
            {
                _nodeCreationWindow = ScriptableObject.CreateInstance<NodeCreationWindow>();
                _nodeCreationWindow.Initialize(this);
            }

            Vector2 screenPoint = GUIUtility.GUIToScreenPoint(evt.mousePosition);
            SearchWindowContext context = new SearchWindowContext(screenPoint, 200, 240);

            SearchWindow.Open(context, _nodeCreationWindow);
        }


        public void SelectNode(NodeView nodeView)
        {
            base.ClearSelection();

            if (nodeView != null)
            {
                base.AddToSelection(nodeView);
            }
        }


        public NodeView CreateNodeAndView(Type type, Vector2 mousePosition)
        {
            Node node = _tree.CreateNode(type);
            node.position = mousePosition;
            return this.CreateNodeView(node);
        }


        public void ClearEditorViwer()
        {
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            
            nodes.ForEach(n => n.RemoveFromHierarchy());
            edges.ForEach(e => e.RemoveFromHierarchy());
        }


        public void UpdateNodeView()
        {
            foreach (var node in nodes)
            {
                if (node is NodeView nodeView)
                {
                    nodeView.UpdateState();
                } 
            }
        }

        #region Private API
        
        private void Intialize(BehaviourTree tree)
        {
            onGraphViewChange?.Invoke();

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            if (_tree.rootNode == null)
            {
                tree.rootNode = tree.CreateNode(typeof(RootNode)) as RootNode;
                UnityEditor.EditorUtility.SetDirty(tree);
                AssetDatabase.SaveAssets();
            }
        }


        private void IntegrityCheckNodeList(BehaviourTree tree)
        {
            for (int i = 0; i < tree.nodeList.Count; ++i)
            {
                //Undo로 생성이 취소된 노드를 여기서 처리.
                if (tree.nodeList[i] == null)
                {
                    tree.nodeList.RemoveAt(i);
                }
            }

            //트리 구조라서 미리 모두 생성해둬야 자식과 부모를 연결 할 수 있음.
            tree.nodeList.ForEach(n => this.CreateNodeView(n));
            tree.nodeList.ForEach(n => _nodeEdgeHandler.ConnectEdges(this, n, tree.GetChildren(n)));
        }
        
        
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    if (element is NodeView view)
                    {
                        this._tree.DeleteNode(view.node);
                    }

                    if (element is Edge edge)
                    {
                        this._nodeEdgeHandler.DeleteEdges(_tree, edge);
                    }
                }
            }

            if (graphViewChange.edgesToCreate != null)
            {
                this._nodeEdgeHandler.ConnectEdges(_tree, graphViewChange.edgesToCreate);
            }

            if (graphViewChange.movedElements != null)
            {
                base.nodes.ForEach(n => (n as NodeView)?.SortChildren());
            }

            return graphViewChange;
        }


        private NodeView CreateNodeView(Node node)
        {
            NodeView nodeView = new NodeView(node, BehaviourTreeEditorWindow.NodeViewXml);
            
            nodeView.OnNodeSelected += this.onNodeSelected;
            nodeView.OnNodeSelected += this._deleteEventDetector.RegisterCallback;
            nodeView.OnNodeUnselected += this._deleteEventDetector.UnregisterCallback;
            
            this.onGraphViewChange = () => _deleteEventDetector.UnregisterCallback(nodeView);
            
            base.AddElement(nodeView); //nodes라는 GraphElement 컨테이너에 추가.
            return nodeView;
        }
        
        #endregion
    }
}
