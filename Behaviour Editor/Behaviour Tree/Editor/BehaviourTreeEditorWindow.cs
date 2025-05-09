using System;
using System.Linq;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;

namespace BehaviourSystemEditor.BT
{
    public class BehaviourTreeEditorWindow : EditorWindow
    {
        [MenuItem("Tools/Behaviour Tree Editor")]
        private static void OpenWindow()
        {
            BehaviourTreeEditorWindow wnd = GetWindow<BehaviourTreeEditorWindow>();
            wnd.titleContent = new GUIContent("BT Editor");
            Instance         = wnd;
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if (Selection.activeObject is BehaviourTree)
            {
                OpenWindow();
                return true;
            }

            return false;
        }


        private static BehaviourTreeEditorSettings _settings;

        public static BehaviourTreeEditorWindow Instance
        {
            get;
            private set;
        }

        public static BehaviourTreeEditorSettings Settings
        {
            get { return _settings ??= BTEditorUtility.FindAssetByName<BehaviourTreeEditorSettings>($"t:{nameof(BehaviourTreeEditorSettings)}"); }
        }



        private BehaviourTree  _tree;
        private BehaviourActor _actor;

        private BehaviourTreeView          _treeView;
        private InspectorView              _inspectorView;
        private BlackboardPropertyViewList _blackboardPropList;


        public bool CanEditTree
        {
            get;
            private set;
        }

        public BehaviourTree Tree
        {
            get { return _tree; }
        }

        public BehaviourTreeView View
        {
            get { return _treeView; }
        }


        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= this.OnPlayNodeStateChanged;
            EditorApplication.playModeStateChanged += this.OnPlayNodeStateChanged;
        }


        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= this.OnPlayNodeStateChanged;
        }


        /// <summary> 새로운 Behaviour Tree Asset이 추가되거나 제거될 때 호출됨. </summary>
        private void OnProjectChange()
        {
            if (_tree is not null && AssetDatabase.Contains(_tree) == false)
            {
                //상호 연관이 적은 것부터 삭제.
                this._blackboardPropList?.ClearBlackboardPropertyViews();
                this._inspectorView?.Clear();
                this._treeView?.ClearEditorViewer();

                this.CanEditTree = false;
                this._actor      = null;
                this._tree       = null;
            }
            else
            {
                EditorApplication.delayCall -= this.OnSelectionChange;
                EditorApplication.delayCall += this.OnSelectionChange;
            }
        }



        private void Update()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            if (_actor is null)
            {
                return;
            }

            _treeView.UpdateNodeView();
        }


        private void CreateGUI()
        {
            Settings.behaviourTreeEditorXml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(Settings.behaviourTreeStyle);

            Instance            = this;
            _inspectorView      = rootVisualElement.Q<InspectorView>();
            _treeView           = rootVisualElement.Q<BehaviourTreeView>();
            _blackboardPropList = rootVisualElement.Q<BlackboardPropertyViewList>();

            _treeView.popupSearchField = rootVisualElement.Q<ToolbarPopupSearchField>("search-node-field");
            _treeView.popupSearchField.RegisterValueChangedCallback(_treeView.SearchNodeByNameOrTag);
            _treeView.onNodeSelected += _inspectorView.UpdateSelection;

            this._blackboardPropList.SetUp(rootVisualElement.Q<ToolbarMenu>("add-element-button"));
            
            this.OnSelectionChange();
        }


        private void OnPlayNodeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode: this.OnSelectionChange(); break;

                case PlayModeStateChange.EnteredPlayMode: this.OnSelectionChange(); break;
            }
        }


        private void OnSelectionChange()
        {
            switch (Selection.activeObject)
            {
                case BehaviourTree treeObj: _tree = treeObj; break;

                case GameObject gameObj: _tree = gameObj.TryGetComponent(out _actor) ? _actor.runtimeTree : null; break;

                default: return;
            }

            CanEditTree = false;

            if (_tree is not null)
            {
                CanEditTree = Application.isPlaying == false;

                bool openedEditorWindow = AssetDatabase.CanOpenAssetInEditor(_tree.GetInstanceID());

                if (_actor is not null && Application.isPlaying || openedEditorWindow)
                {
                    _inspectorView?.Clear();
                    _treeView?.OnGraphEditorView(_tree);
                    
                    _blackboardPropList?.ClearBlackboardPropertyViews();
                    _blackboardPropList?.ChangeBehaviourTree(_tree);
                }
            }
        }
    }
}