using System;
using System.Collections.Generic;
using System.Linq;
using BehaviourSystem.BT;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    [UxmlElement]
    public partial class NodeSearchFieldView : ToolbarSearchField
    {
        private readonly List<NodeView> _itemSource = new List<NodeView>();
        private readonly ListView _nodeListView = new ListView();

        private BehaviourTreeView _treeView;
        private InspectorView _inspectorView;


        public void Setup(InspectorView inspectorView, BehaviourTreeView treeView)
        {
            this.UnregisterValueChangedCallback(this.OnSearchingNodesCallback);
            this.RegisterValueChangedCallback(this.OnSearchingNodesCallback);

            this.UnregisterCallback<FocusInEvent>(this.OnFocusInSearchField);
            this.RegisterCallback<FocusInEvent>(this.OnFocusInSearchField);

            _treeView = treeView;
            _inspectorView = inspectorView;

            _nodeListView.itemsSource = this._itemSource;

            _nodeListView.itemsChosen -= this.OnFocusNodeViewInList;
            _nodeListView.itemsChosen += this.OnFocusNodeViewInList;

            _nodeListView.makeItem -= this.MakeItem;
            _nodeListView.makeItem += this.MakeItem;

            _nodeListView.bindItem -= this.BindItem;
            _nodeListView.bindItem += this.BindItem;
        }


        private void OnFocusInSearchField(FocusInEvent evt)
        {
            _inspectorView.BorrowInspectorGUI(_nodeListView);
        }


        private void OnSearchingNodesCallback(ChangeEvent<string> evt)
        {
            _nodeListView.itemsSource.Clear();
            
            if (string.IsNullOrEmpty(evt.newValue) || string.IsNullOrWhiteSpace(evt.newValue))
            {
                _nodeListView.RefreshItems();
                return;
            }

            foreach (var node in _treeView.nodes)
            {
                NodeView nodeView = (NodeView)node;
                NodeBase nodeBase = nodeView.targetNode;
                bool found = false;

                if (string.IsNullOrEmpty(nodeBase.name) == false && nodeBase.name.Contains(evt.newValue, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                }
                else if (string.IsNullOrEmpty(nodeBase.tag) == false && nodeBase.tag.Contains(evt.newValue, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                }

                if (found)
                {
                    _nodeListView.itemsSource.Add(nodeView);
                    _nodeListView.RefreshItems();
                }
            }
        }


        private VisualElement MakeItem()
        {
            VisualElement subContainer = new VisualElement();
            VisualElement element = new VisualElement();
            element.style.flexGrow = 1;
            element.Add(subContainer);
            
            subContainer.style.flexDirection = FlexDirection.Row;
            subContainer.style.alignItems = Align.Center;
            subContainer.style.flexGrow = 1;
            subContainer.style.marginBottom = new StyleLength(1f);
            subContainer.style.backgroundColor = new StyleColor(new Color32(40, 40, 40, 255));
            return element;
        }

        
        private void BindItem(VisualElement container, int index)
        {
            container[0].Clear();
            NodeView view = _nodeListView.itemsSource[index] as NodeView;

            Label indexLabel = new Label($"{index + 1}. ");
            indexLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            indexLabel.style.fontSize = 14;
            container[0].Add(indexLabel);
            
            Label nameLabel = new Label(view!.targetNode.name);
            nameLabel.style.letterSpacing = new StyleLength(5f);
            nameLabel.style.fontSize = 13;
            container[0].Add(nameLabel);
            
            if (string.IsNullOrEmpty(view.targetNode.tag) == false)
            {
                Label tagLabel = new Label(view.targetNode.tag);
                tagLabel.style.paddingLeft = new StyleLength(2f);
                tagLabel.style.fontSize = 11;
                tagLabel.style.color = Color.cyan;
                container[0].Add(tagLabel);
            }
        }


        private void OnFocusNodeViewInList(IEnumerable<object> items)
        {
            NodeView view = items.First() as NodeView;
            _treeView.SelectNode(view);
            _treeView.FrameSelection();
        }
    }
}