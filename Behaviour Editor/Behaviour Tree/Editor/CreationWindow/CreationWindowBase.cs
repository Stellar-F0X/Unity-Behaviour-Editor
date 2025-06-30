using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BehaviourSystem.BT;
using BehaviourSystem.BT.State;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace BehaviourSystemEditor.BT
{
    public abstract class CreationWindowBase : ScriptableObject, ISearchWindowProvider
    {
        private readonly Vector2 _nodeOffset = new Vector2(-75, -20);
        private event Action<NodeView> _createCallback;


        protected BehaviourGraphView graphView
        {
            get { return BehaviorEditor.Instance.view; }
        }


        public void RegisterNodeCreationCallbackOnce(Action<NodeView> callback)
        {
            _createCallback = null;
            _createCallback = callback;
        }


        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTree = new List<SearchTreeEntry>();
            searchTree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node"), 0));

            this.RegisterSubSearchTrees(searchTree, context);

            searchTree.Add(new SearchTreeGroupEntry(new GUIContent("Group"), 1));
            searchTree.Add(this.CreateNodeViewGroupSearchTreeEntry(context));
            return searchTree;
        }


        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (searchTreeEntry.userData is Action entryIAction)
            {
                entryIAction.Invoke();
                return true;
            }

            Debug.LogError($"{nameof(CreationWindowBase)} Error : Entry is empty");
            return false;
        }


        private Vector2 CalculateMousePosition(SearchWindowContext context)
        {
            if (BehaviorEditor.canEditGraph == false)
            {
                Debug.LogError($"{nameof(CreationWindowBase)} Error : CanEditGraph is false");
                return Vector2.zero;
            }

            BehaviorEditor editor = BehaviorEditor.Instance;

            Vector2 targetVector = context.screenMousePosition - editor.position.position;
            Vector2 mousePosition = editor.rootVisualElement.ChangeCoordinatesTo(editor.rootVisualElement.parent, targetVector);
            return editor.view.contentViewContainer.WorldToLocal(mousePosition);
        }


        protected SearchTreeEntry[] CreateSearchTreeEntry<TParent>(string title, Action<Type> invoke, int layerLevel = 1) where TParent : NodeBase
        {
            Type[] typeList = TypeCache.GetTypesDerivedFrom<TParent>().OrderByNameAndFilterAbstracts();
            SearchTreeEntry[] entries = new SearchTreeEntry[typeList.Length + 1];
            entries[0] = new SearchTreeGroupEntry(new GUIContent(title));
            entries[0].level = layerLevel;

            for (int i = 1; i < entries.Length; ++i)
            {
                Type nodeType = typeList[i - 1];
                string typeName = NodeFactory.ApplySpacing(nodeType.Name);

                entries[i] = new SearchTreeEntry(new GUIContent(typeName))
                {
                    userData = (Action)(() => invoke.Invoke(nodeType)),
                    level = layerLevel + 1
                };
            }

            return entries;
        }


        protected virtual NodeView CreateNode(Type type, SearchWindowContext context)
        {
            Vector2 nodePosition = _nodeOffset + this.CalculateMousePosition(context);
            NodeView nodeView = graphView.CreateNewNodeAndView(type, nodePosition);

            _createCallback?.Invoke(nodeView);
            _createCallback = null;
            graphView.SelectNode(nodeView);
            return nodeView;
        }


        private SearchTreeEntry CreateNodeViewGroupSearchTreeEntry(SearchWindowContext context, int layerLevel = 2)
        {
            SearchTreeEntry nodeViewGroupEntry = new SearchTreeEntry(new GUIContent("Node Group"));
            Vector2 graphMousePosition = this.CalculateMousePosition(context);

            nodeViewGroupEntry.content.text = "Group";
            nodeViewGroupEntry.userData = (Action)(() => graphView.CreateNewNodeGroupView("Node Group", graphMousePosition));
            nodeViewGroupEntry.level = layerLevel;

            return nodeViewGroupEntry;
        }


        protected virtual void AllocateSubGraphAssets(NodeView newSubGraphNodeView)
        {
            if (newSubGraphNodeView.targetNode is ISubGraphNode graphNode)
            {
                GraphAsset parentGraphAsset = BehaviorEditor.Instance.graph;
                
                Debug.Assert(parentGraphAsset != null, $"{nameof(CreationWindowBase)}: ParentGraphAsset is null");

                switch (graphNode.subGraphType)
                {
                    case EGraphType.BT: graphNode.subGraphAsset = GraphFactory.CreateGraphAsset<BehaviourTreeAsset>(parentGraphAsset); break;

                    case EGraphType.FSM: graphNode.subGraphAsset = GraphFactory.CreateGraphAsset<FiniteStateMachineAsset>(parentGraphAsset); break;
                }

                graphNode.subGraphAsset.name = newSubGraphNodeView.title;
                EditorUtility.SetDirty(graphNode as NodeBase);
                AssetDatabase.SaveAssets();
            }
        }


        protected abstract void RegisterSubSearchTrees(List<SearchTreeEntry> searchTree, SearchWindowContext context);
    }
}