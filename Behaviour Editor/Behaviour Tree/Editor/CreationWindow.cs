using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    public class CreationWindow : ScriptableObject, ISearchWindowProvider
    {
        private readonly Vector2 _nodeOffset = new Vector2(-75, -20);
        private event Action<NodeView> _nodeCreateCallback;
        
        private BehaviourTreeView _treeView;


        public void Initialize(BehaviourTreeView editorWindowView)
        {
            _treeView = editorWindowView;
        }
        

        public void RegisterNodeCreationCallbackOnce(Action<NodeView> callback)
        {
            _nodeCreateCallback = null;
            _nodeCreateCallback = callback;
        }


        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTree = new List<SearchTreeEntry>();
            searchTree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node"), 0));

            searchTree.AddRange(this.NodeCreationSearchTreeEntry<ActionNode>("Action", t => this.CreateNode(t, context)));
            searchTree.AddRange(this.NodeCreationSearchTreeEntry<SubsetNode>("Subset", t => this.CreateNode(t, context)));
            searchTree.AddRange(this.NodeCreationSearchTreeEntry<CompositeNode>("Composite", t => this.CreateNode(t, context)));
            searchTree.AddRange(this.NodeCreationSearchTreeEntry<DecoratorNode>("Decorator", t => this.CreateNode(t, context)));

            searchTree.Add(new SearchTreeGroupEntry(new GUIContent("Utility"), 1));
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

            Debug.LogError($"{nameof(CreationWindow)} Error : Entry is empty");
            return false;
        }
        
        
        private Vector2 CalculateMousePosition(SearchWindowContext context)
        {
            BehaviourTreeEditor editor = BehaviourTreeEditor.Instance;
            Vector2 targetVector = context.screenMousePosition - editor.position.position;
            Vector2 mousePosition = editor.rootVisualElement.ChangeCoordinatesTo(editor.rootVisualElement.parent, targetVector);
            return editor.View.contentViewContainer.WorldToLocal(mousePosition);
        }


        private SearchTreeEntry[] NodeCreationSearchTreeEntry<T>(string title, Action<Type> invoke, int layerLevel = 1) where T : NodeBase
        {
            Type[] typeList = TypeCache.GetTypesDerivedFrom<T>().OrderByNameAndFilterAbstracts();
            SearchTreeEntry[] entries = new SearchTreeEntry[typeList.Length + 1];
            entries[0] = new SearchTreeGroupEntry(new GUIContent(title));
            entries[0].level = layerLevel;

            int count = entries.Length;
            
            for (int i = 1; i < count; ++i)
            {
                int index = i - 1;
                string typeName = Regex.Replace(typeList[index].Name.Replace("Node", ""), "(?<!^)([A-Z])", " $1");
                
                entries[i] = new SearchTreeEntry(new GUIContent(typeName));
                entries[i].content.text = typeName;
                entries[i].userData = (Action)(() => invoke.Invoke(typeList[index]));
                entries[i].level = layerLevel + 1;
            }

            return entries;
        }


        private NodeView CreateNode(Type type, SearchWindowContext context)
        {
            Vector2 nodePosition = _nodeOffset + this.CalculateMousePosition(context);
            NodeView nodeView = _treeView.CreateNewNodeAndView(type, nodePosition);
            
            _nodeCreateCallback?.Invoke(nodeView);
            _nodeCreateCallback = null;
            _treeView.SelectNode(nodeView);
            return nodeView;
        }


        private SearchTreeEntry CreateNodeViewGroupSearchTreeEntry(SearchWindowContext context, int layerLevel = 2)
        {
            SearchTreeEntry nodeViewGroupEntry = new SearchTreeEntry(new GUIContent("Node Group"));
            Vector2 graphMousePosition = this.CalculateMousePosition(context);

            nodeViewGroupEntry.content.text = "Group";
            nodeViewGroupEntry.userData = (Action)(() => _treeView.CreateNewNodeGroupView("Node Group", graphMousePosition));
            nodeViewGroupEntry.level = layerLevel;

            return nodeViewGroupEntry;
        }
    }
}