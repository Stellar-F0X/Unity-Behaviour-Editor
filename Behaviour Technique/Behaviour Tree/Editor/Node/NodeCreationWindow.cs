using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public class NodeCreationWindow : ScriptableObject, ISearchWindowProvider
    {
        private readonly Vector2 _nodeOffset = new Vector2(-75, -20);
        private BehaviourTreeView _treeView;


        public void Initialize(BehaviourTreeView editorWindowView) => _treeView = editorWindowView;
        
        

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTree = new List<SearchTreeEntry>();
            searchTree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node"), 0));

            searchTree.AddRange(CreateSubSearchTreeEntry<ActionNode>("Action", t => CreateNode(t, context)));
            searchTree.AddRange(CreateSubSearchTreeEntry<SubsetNode>("Subset", t => CreateNode(t, context)));
            searchTree.AddRange(CreateSubSearchTreeEntry<CompositeNode>("Composite", t => CreateNode(t, context)));
            searchTree.AddRange(CreateSubSearchTreeEntry<DecoratorNode>("Decorator", t => CreateNode(t, context)));

            return searchTree;
        }


        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Action entryIAction = SearchTreeEntry.userData as Action;

            if (entryIAction != null)
            {
                entryIAction.Invoke();
                return true;
            }

            Debug.LogError($"{nameof(NodeCreationWindow)} Error : Entry is empty");
            return false;
        }


        private SearchTreeEntry[] CreateSubSearchTreeEntry<T>(string title, Action<Type> invoke, int layerLevel = 1, Type[] filter = null) where T : Node
        {
            TypeCache.TypeCollection typeList = TypeCache.GetTypesDerivedFrom<T>();
            SearchTreeEntry[] entries = new SearchTreeEntry[typeList.Count + 1];
            entries[0] = new SearchTreeGroupEntry(new GUIContent(title)) {
                level = layerLevel,
            };

            for (int i = 1; i < entries.Length; ++i)
            {
                Type currentNodeType = typeList[i - 1];

                if (filter == null || !filter.Where(t => t == currentNodeType).Any())
                {
                    Action createNodeEvent = () => invoke.Invoke(currentNodeType);

                    entries[i] = new SearchTreeEntry(new GUIContent(currentNodeType.Name));
                    entries[i].content.text = currentNodeType.Name;
                    entries[i].userData = createNodeEvent;
                    entries[i].level = layerLevel + 1;
                }
            }
            return entries;
        }


        private NodeView CreateNode(Type type, SearchWindowContext context)
        {
            BehaviourTreeEditorWindow editorWindow = BehaviourTreeEditorWindow.Instance;

            Vector2 targetVector = context.screenMousePosition - editorWindow.position.position;
            Vector2 mousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, targetVector);

            Vector2 graphMousePosition = editorWindow.View.contentViewContainer.WorldToLocal(mousePosition);
            Vector2 nodeResultPosition = _nodeOffset + graphMousePosition;

            nodeResultPosition = new Vector2() {
                x = Mathf.RoundToInt(nodeResultPosition.x / graphMousePosition.x) * graphMousePosition.x,
                y = Mathf.RoundToInt(nodeResultPosition.y / graphMousePosition.y) * graphMousePosition.y
            };

            NodeView nodeView = _treeView.CreateNodeAndView(type, nodeResultPosition);
            _treeView.SelectNode(nodeView);
            return nodeView;
        }
    }
}
