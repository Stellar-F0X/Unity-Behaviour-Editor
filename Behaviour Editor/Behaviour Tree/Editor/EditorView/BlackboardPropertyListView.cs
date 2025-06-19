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

        
        /// <summary>블랙보드 프로퍼티 리스트 뷰를 초기화하고 UI 요소들을 설정합니다.</summary>
        public void Setup(ToolbarMenu toolbarMenu, ObjectField blackboardBindingField)
        {
            this._propertyAddMenu = toolbarMenu;
            this._blackboardBindingField = blackboardBindingField;

            this.bindItem = this.BindItemToList;
            this.fixedItemHeight = 50f;
            this.itemIndexChanged += this.OnPropertyIndicesSwapped;

            this._blackboardBindingField.UnregisterValueChangedCallback(this.OnBindBlackboardAsset);
            this._blackboardBindingField.RegisterValueChangedCallback(this.OnBindBlackboardAsset);
        }
        

        /// <summary>블랙보드 에셋이 바인딩될 때 호출되는 콜백 메서드입니다.</summary>
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
        

        /// <summary>언두 작업이 수행될 때 아이템을 새로고침합니다.</summary>
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
        

        /// <summary>블랙보드 뷰를 초기화하고 모든 데이터를 제거합니다.</summary>
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
        

        /// <summary>Behaviour Tree가 변경될 때 블랙보드 뷰를 업데이트합니다.</summary>
        public void OnBehaviourTreeChanged(BehaviourTree tree)
        {
            if (tree is null || BehaviourTreeEditor.Instance is null)
            {
                return;
            }

            this.enabledSelf = !Application.isPlaying;

            this._blackboard = tree.blackboard;
            this._blackboardBindingField.value = tree.blackboard;
            this._blackboardBindingField.enabledSelf = BehaviourTreeEditor.CanEditTree;

            this.RefreshBlackboardProperties();
        }

        
        /// <summary>블랙보드 프로퍼티들을 새로고침하고 UI를 업데이트합니다.</summary>
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
                //IBlackboardProperty를 상속받은 자식 클래스들 타입을 가져와, 블랙보드 프로퍼티 추가 메뉴를 설정합니다.
                TypeCache.GetTypesDerivedFrom<IBlackboardProperty>()
                         .OrderByNameAndFilterAbstracts()
                         .ForEach(t => _propertyAddMenu.menu.AppendAction(t.Name, _ => this.MakeProperty(t)));
            }
        }

        
        /// <summary>새로운 블랙보드 프로퍼티를 생성합니다.</summary>
        private void MakeProperty(Type type)
        {
            Undo.RecordObject(_blackboard, "Behaviour Tree (AddBlackboardProperty)");

            itemsSource.Add(IBlackboardProperty.Create(type));
            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_blackboard);
            this.RefreshItems();
        }

        
        /// <summary>지정된 인덱스의 블랙보드 프로퍼티를 삭제합니다.</summary>
        private void DeleteProperty(int index)
        {
            Undo.RecordObject(_blackboard, "Behaviour Tree (RemoveBlackboardProperty)");

            itemsSource.RemoveAt(index);
            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_blackboard);
            this.RefreshItems();
        }

        
        /// <summary>리스트 아이템을 UI 요소에 바인딩합니다.</summary>
        private void BindItemToList(VisualElement element, int index)
        {
            if (_serializedListProperty.arraySize > index)
            {
                this.BindDeleteButton(element, index);
                this.BindIMGUIContainer(element, index);
                this.BindPropertyKeyField(element, index);
            }
        }

        
        /// <summary>삭제 버튼을 바인딩하고 클릭 이벤트를 설정합니다.</summary>
        private void BindDeleteButton(VisualElement element, int index)
        {
            Button buttonField = element.Q<Button>("delete-button");
            buttonField.clickable = null; //reset all callback
            buttonField.clicked += () => this.DeleteProperty(index);
            buttonField.enabledSelf = BehaviourTreeEditor.CanEditTree;
        }

        
        /// <summary>IMGUI 컨테이너를 바인딩하고 프로퍼티 값을 표시합니다.</summary>
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
        

        /// <summary>프로퍼티 키 필드를 바인딩하고 변경 이벤트를 설정합니다.</summary>
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
        

        /// <summary>프로퍼티 아이템을 IMGUI로 그립니다.</summary>
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

        
        /// <summary>유효하지 않은 프로퍼티 타입에 대한 경고 메시지를 IMGUI로 그립니다.</summary>
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
        

        /// <summary>프로퍼티 키가 변경될 때 호출되는 콜백 메서드입니다.</summary>
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

        
        /// <summary>프로퍼티의 순서가 변경될 때 호출되는 콜백 메서드입니다.</summary>
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