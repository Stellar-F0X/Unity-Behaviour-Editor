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
            this._propertyAddMenu = toolbarMenu;
            this._blackboardBindingField = blackboardBindingField;

            this.bindItem = this.BindItemToList;
            this.itemIndexChanged += this.OnPropertyIndicesSwapped;

            this._blackboardBindingField.UnregisterValueChangedCallback(this.OnBindBlackboardAsset);
            this._blackboardBindingField.RegisterValueChangedCallback(this.OnBindBlackboardAsset);
        }



        private void OnBindBlackboardAsset(ChangeEvent<Object> changeEvent)
        {
            if (BehaviourTreeEditor.Instance?.Tree is null)
            {
                return;
            }

            Blackboard newBlackboardAsset = changeEvent.newValue as Blackboard;
            BehaviourTreeEditor.Instance.Tree.blackboard = newBlackboardAsset;
            this._blackboard = newBlackboardAsset;

            this.RefreshBlackboardProperties();
        }



        public void RefreshItemsWhenUndoPerformed()
        {
            if (_serializedObject?.targetObject is null)
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
            this._blackboardBindingField.SetValueWithoutNotify(null);

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

            this.reorderable = !Application.isPlaying;

            this._blackboard = tree.blackboard;
            this._blackboardBindingField.value = tree.blackboard;
            this._blackboardBindingField.enabledSelf = BehaviourTreeEditor.CanEditTree;

            this.RefreshBlackboardProperties();
        }



        private void RefreshBlackboardProperties()
        {
            if (this._blackboard is null)
            {
                if (this.itemsSource is not null)
                {
                    this.itemsSource = null;
                    this.RefreshItems();
                }
                
                return;
            }

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
            if (_serializedListProperty.arraySize > index)
            {
                this.BindDeleteButton(element, index);
                this.BindIMGUIContainer(element, index);
                this.BindPropertyKeyField(element, index);
            }
        }

        
        
        private void BindDeleteButton(VisualElement element, int index)
        {
            Button buttonField = element.Q<Button>("delete-button");
            buttonField.clickable = null; //reset all callback
            buttonField.clicked += () => this.DeleteProperty(index);
            buttonField.enabledSelf = BehaviourTreeEditor.CanEditTree;
        }
        
        

        private void BindIMGUIContainer(VisualElement element, int index)
        {
            SerializedProperty elementProperty = _serializedListProperty.GetArrayElementAtIndex(index);

            if (elementProperty.boxedValue is not null)
            {
                IMGUIContainer imguiField = element.Q<IMGUIContainer>("property-imgui-field");
                SerializedProperty valueProp = elementProperty.FindPropertyRelative("_value");

                if (valueProp is null)
                {
                    imguiField.Unbind();
                    imguiField.onGUIHandler = () => DrawIMGUIForErrorMessage(elementProperty);
                }
                else
                {
                    imguiField.Unbind();
                    imguiField.TrackPropertyValue(valueProp, _ => imguiField.MarkDirtyRepaint());
                    imguiField.onGUIHandler = () => this.DrawIMGUIForItem(elementProperty, valueProp);
                }
            }
        }

        
        
        private void BindPropertyKeyField(VisualElement element, int index)
        {
            if (itemsSource[index] is IBlackboardProperty property)
            {
                element.tooltip = property.type.Name;

                TextField keyField = element.Q<TextField>("name-field");
                keyField.UnregisterAllValueChangedCallback<string>();
                keyField.RegisterValueChangedCallback<string>(e => this.OnChangePropertyKey(e.newValue, index));
                keyField.value = property.key;
                keyField.enabledSelf = BehaviourTreeEditor.CanEditTree;
            }
        }



        private void DrawIMGUIForItem(SerializedProperty property, SerializedProperty valueProp)
        {
            if (property is null || property.boxedValue is null || valueProp is null)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(valueProp, GUIContent.none, true);
            }
        }



        private void DrawIMGUIForErrorMessage(SerializedProperty property)
        {
            if (property is null || property.boxedValue is null)
            {
                return;
            }

            const float iconSize = 12f;

            using (new EditorGUI.DisabledScope(true))
            {
                Rect position = EditorGUILayout.GetControlRect();

                Rect iconRect = new Rect(position.x, position.y + (position.height - iconSize) * 0.5f, iconSize, iconSize);
                Rect textRect = new Rect(position.x + iconSize + 2f, position.y, position.width - iconSize - 2f, position.height);

                Texture warningImg = EditorGUIUtility.IconContent("console.warnicon").image;
                GUI.DrawTexture(iconRect, warningImg, ScaleMode.ScaleToFit);
                EditorGUI.LabelField(textRect, "Invalid blackboard property type.");
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