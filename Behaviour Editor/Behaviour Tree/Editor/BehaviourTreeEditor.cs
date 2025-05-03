using BehaviourSystem.BT;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine.SceneManagement;

namespace BehaviourSystemEditor.BT
{
    public class BehaviourTreeEditor : EditorWindow
    {
        private static BehaviourTreeEditorSettings _settings;

        private BehaviourTree _tree;

        private BehaviourTreeRunner _treeRunner;

        private MiniMapView _miniMapView;

        private InspectorView _inspectorView;

        private BehaviourTreeView _treeView;

        private NodeSearchFieldView _nodeSearchField;

        private BlackboardPropertyListView _blackboardView;



        public static BehaviourTreeEditorSettings Settings
        {
            get
            {
                if (_settings is null)
                {
                    string filter = $"t:{nameof(BehaviourTreeEditorSettings)}";
                    _settings = EditorHelper.FindAssetByName<BehaviourTreeEditorSettings>(filter);
                }

                return _settings;
            }
        }

        public static BehaviourTreeEditor Instance
        {
            get;
            private set;
        }

        public static bool CanEditTree
        {
            get;
            private set;
        }

        public static bool isInLoadingBTAsset
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



        [MenuItem("Tools/Behaviour Tree Editor")]
        private static void OpenWindow()
        {
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("BT Editor");
            Instance = wnd;
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


        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= this.OnPlayNodeStateChanged;
            EditorApplication.playModeStateChanged += this.OnPlayNodeStateChanged;

            Undo.undoRedoPerformed -= this.BehaviourEditorUndoPerformed;
            Undo.undoRedoPerformed += this.BehaviourEditorUndoPerformed;

            EditorSceneManager.sceneClosed -= this.OnSceneClosed;
            EditorSceneManager.sceneClosed += this.OnSceneClosed;

            if (EditorApplication.isPlaying)
            {
                EditorApplication.update -= this.RuntimeUpdate;
                EditorApplication.update += this.RuntimeUpdate;
            }
        }


        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= this.OnPlayNodeStateChanged;
            EditorSceneManager.sceneClosed -= this.OnSceneClosed;

            Undo.undoRedoPerformed -= this.BehaviourEditorUndoPerformed;
            EditorApplication.update -= this.RuntimeUpdate;
        }


        /// <summary> 새로운 Behaviour Tree Asset이 추가되거나 제거될 때 호출됨. </summary>
        private void OnProjectChange()
        {
            if (_tree is not null && AssetDatabase.Contains(_tree) == false)
            {
                //상호 연관이 적은 것부터 삭제.
                this._blackboardView.ClearBlackboardView();
                this._inspectorView.Clear();
                this._treeView?.ClearEditorView();

                CanEditTree = false;
                this._treeRunner = null;
                this._tree = null;
            }
            else
            {
                EditorApplication.delayCall -= this.OnSelectionChange;
                EditorApplication.delayCall += this.OnSelectionChange;
            }
        }



        private void OnSceneClosed(Scene scene)
        {
            if (_tree is null)
            {
                CanEditTree = false;

                this._blackboardView?.ClearBlackboardView();
                this._inspectorView?.Clear();
                this._treeView?.ClearEditorView();

                this._treeRunner = null;
                this._tree = null;
            }
        }



        private void RuntimeUpdate()
        {
            if (Application.isPlaying == false || EditorApplication.isPaused)
            {
                return;
            }

            if (_treeRunner is null || _treeRunner.runtimeTree is null)
            {
                return;
            }

            _treeView?.UpdateNodeView();
        }



        private void BehaviourEditorUndoPerformed()
        {
            _treeView?.OnGraphEditorView(_tree);
            _inspectorView?.ClearInspectorView();
            _blackboardView?.RefreshItemsWhenUndoPerformed();
            AssetDatabase.SaveAssets();
        }



        private void CreateGUI()
        {
            Instance = this;

            Settings.behaviourTreeEditorXml.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(Settings.behaviourTreeStyle);

            _treeView = rootVisualElement.Q<BehaviourTreeView>();
            _miniMapView = rootVisualElement.Q<MiniMapView>();
            _inspectorView = rootVisualElement.Q<InspectorView>();
            _nodeSearchField = rootVisualElement.Q<NodeSearchFieldView>();
            _blackboardView = rootVisualElement.Q<BlackboardPropertyListView>();

            var elementAddButton = rootVisualElement.Q<ToolbarMenu>("element-add-button");
            var minimapActivateButton = rootVisualElement.Q<ToolbarToggle>("active-minimap");
            var blackboardBindField = rootVisualElement.Q<ObjectField>("blackboard-field");

            _blackboardView.Setup(elementAddButton, blackboardBindField);
            _miniMapView.Setup(minimapActivateButton, _treeView);
            _nodeSearchField.Setup(_inspectorView, _treeView);

            _treeView.onNodeSelected -= _inspectorView.UpdateSelection;
            _treeView.onNodeSelected += _inspectorView.UpdateSelection;

            this.OnSelectionChange();
        }


        private void OnPlayNodeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                {
                    EditorApplication.update -= this.RuntimeUpdate;
                    this.OnSelectionChange();
                    return;
                }

                case PlayModeStateChange.EnteredPlayMode:
                {
                    EditorApplication.update += this.RuntimeUpdate;
                    this.OnSelectionChange();
                    return;
                }
            }
        }


        private void OnSelectionChange()
        {
            if (this.TryGetTreeAsset(out BehaviourTree selectedTreeAsset))
            {
                _tree = selectedTreeAsset;
                CanEditTree = Application.isPlaying == false;

                bool openedEditorWindow = AssetDatabase.CanOpenAssetInEditor(_tree.GetInstanceID());

                if (_treeRunner is not null && Application.isPlaying || openedEditorWindow)
                {
                    isInLoadingBTAsset = true;

                    _inspectorView?.ClearInspectorView();
                    _blackboardView?.ClearBlackboardView();
                    _blackboardView?.OnBehaviourTreeChanged(_tree);
                    _treeView?.OnGraphEditorView(_tree);

                    isInLoadingBTAsset = false;
                }
            }
        }


        private bool TryGetTreeAsset(out BehaviourTree tree)
        {
            Object selectedObject = Selection.activeObject;

            if (selectedObject is BehaviourTree treeAsset)
            {
                tree = treeAsset;
                return true;
            }
            
            if (selectedObject is GameObject gobj && gobj.TryGetComponent(out BehaviourTreeRunner runner))
            {
                tree = runner.runtimeTree;
                _treeRunner = runner;
                return tree is not null;
            }

            tree = null;
            return false;
        }
    }
}