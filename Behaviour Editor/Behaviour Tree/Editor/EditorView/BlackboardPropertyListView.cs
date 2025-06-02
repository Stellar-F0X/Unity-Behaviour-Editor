using System;
using System.Linq;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace BehaviourSystemEditor.BT
{
    [UxmlElement]
    public partial class BlackboardPropertyListView : ListView
    {
        private SerializedProperty _serializedListProperty;
        private SerializedObject _serializedObject;

        private ObjectField _blackboardBindingField;
        private ToolbarMenu _propertyAddMenu;
        private Blackboard _blackboard;


        public void Setup(ToolbarMenu toolbarMenu, ObjectField blackboardBindingField)
        {
            this._blackboardBindingField = blackboardBindingField;
            this._propertyAddMenu = toolbarMenu;

            this.makeItem = BehaviourTreeEditor.Settings.blackboardPropertyViewXml.CloneTree;
            this.reorderable = BehaviourTreeEditor.CanEditTree;

            this.bindItem = this.BindItemToList;
            this.itemIndexChanged += this.OnPropertyIndicesSwapped;

            this._blackboardBindingField.UnregisterValueChangedCallback(this.OnBindBlackboardAsset);
            this._blackboardBindingField.RegisterValueChangedCallback(this.OnBindBlackboardAsset);
        }



        private void OnBindBlackboardAsset(ChangeEvent<Object> changeEvent)
        {
            if (BehaviourTreeEditor.Instance is null)
            {
                return;
            }
            
            this._blackboard = changeEvent.newValue as Blackboard;
            BehaviourTreeEditor.Instance.Tree.blackboard = changeEvent.newValue as Blackboard;
            
            this.RefreshBlackboardProperties();
        }



        public void RefreshItemsWhenUndoPerformed()
        {
            if (_serializedObject is null)
            {
                return;
            }

            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();
            this.RefreshItems();
        }



        public void ClearBlackboardView()
        {
            this.itemsSource = null;
            this._blackboard = null;
            this._serializedObject = null;
            this._serializedListProperty = null;

            _propertyAddMenu.menu.ClearItems();

            this.Clear();
            this.RefreshItems();
        }


        public void OnBehaviourTreeChanged(BehaviourTree tree)
        {
            if (tree is null || BehaviourTreeEditor.Instance is null)
            {
                return;
            }

            this._blackboard = tree.blackboard;
            this._blackboardBindingField.value = tree.blackboard;

            if (tree.blackboard is null)
            {
                return;
            }

            this.RefreshBlackboardProperties();
        }



        private void RefreshBlackboardProperties()
        {
            this._serializedObject = new SerializedObject(this._blackboard);
            this._serializedListProperty = _serializedObject.FindProperty("_properties");

            this.itemsSource = this._blackboard.properties;
            this.RefreshItems();

            if (BehaviourTreeEditor.CanEditTree)
            {
                TypeCache.GetTypesDerivedFrom<IBlackboardProperty>()
                         .OrderByNameAndFilterAbstracts()
                         .ForEach(t => _propertyAddMenu.menu.AppendAction(t.Name, _ => this.MakeProperty(t)));
            }
        }
        


        private void MakeProperty(Type type)
        {
            Undo.RecordObject(_blackboard, "Behaviour Tree (AddBlackboardProperty)");

            itemsSource.Add(IBlackboardProperty.Create(type));
            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_blackboard);
            this.RefreshItems();
        }


        private void DeleteProperty(int index)
        {
            Undo.RecordObject(_blackboard, "Behaviour Tree (RemoveBlackboardProperty)");

            itemsSource.RemoveAt(index);
            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_blackboard);
            this.RefreshItems();
        }


        //아이템이 추가, 제거, 순서가 변경될 때마다 호출되어 콜백들을 다시 등록하므로 인덱스가 캐싱돼도 문제되지 않는다.
        private void BindItemToList(VisualElement element, int index)
        {
            if (_serializedListProperty.arraySize <= index)
            {
                return;
            }

            var imguiField = element.Q<IMGUIContainer>("IMGUIContainer");
            var keyField = element.Q<TextField>("name-field");
            var buttonField = element.Q<Button>("delete-button");

            buttonField.clickable = null; //reset all callback
            buttonField.clicked += () => this.DeleteProperty(index);
            buttonField.enabledSelf = BehaviourTreeEditor.CanEditTree;

            SerializedProperty elementProperty = _serializedListProperty.GetArrayElementAtIndex(index);
            SerializedProperty valueProp = elementProperty.FindPropertyRelative("_value");

            if (elementProperty.boxedValue is not null && valueProp is not null)
            {
                imguiField.Unbind();
                imguiField.TrackPropertyValue(valueProp, _ => imguiField.MarkDirtyRepaint());
                imguiField.onGUIHandler = () => this.DrawIMGUIForItem(elementProperty);
            }

            if (keyField.userData is not null)
            {
                var previousCallback = (EventCallback<ChangeEvent<string>>)keyField.userData;
                keyField.UnregisterValueChangedCallback(previousCallback);
            }

            var newCallback = new EventCallback<ChangeEvent<string>>(e => this.OnChangePropertyKey(e.newValue, index));
            keyField.RegisterValueChangedCallback(newCallback);
            keyField.userData = newCallback;

            if (itemsSource[index] is IBlackboardProperty property)
            {
                element.tooltip = property.type.Name;
                keyField.value = property.key;
                keyField.enabledSelf = BehaviourTreeEditor.CanEditTree;
            }
        }


        private void DrawIMGUIForItem(SerializedProperty property)
        {
            if (property is null || property.boxedValue is null)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(property.FindPropertyRelative("_value"), GUIContent.none);
            }
        }


        private void OnChangePropertyKey(string newKey, int index)
        {
            if (itemsSource[index] is IBlackboardProperty property)
            {
                bool isKeyValid = string.IsNullOrEmpty(newKey);
                property.key = isKeyValid ? string.Empty : newKey;

                _serializedObject.Update();
                _serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_blackboard);
            }
        }


        private void OnPropertyIndicesSwapped(int a, int b)
        {
            if (BehaviourTreeEditor.CanEditTree)
            {
                _serializedObject.Update();
                _serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_blackboard);
            }
        }
    }
}