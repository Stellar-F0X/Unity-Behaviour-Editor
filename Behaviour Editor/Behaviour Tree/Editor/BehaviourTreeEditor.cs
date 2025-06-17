using System;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;

namespace BehaviourSystemEditor.BT
{
    public class BehaviourTreeEditor : EditorWindow
    {
        private static BehaviourTreeEditorSettings _settings;

        private BehaviourTree _tree;

        private BehaviourTree _editorOnlyTree;

        private BehaviourTreeRunner _treeRunner;

        private MiniMapView _miniMapView;

        private InspectorView _inspectorView;

        private BehaviourTreeView _treeView;

        private NodeSearchFieldView _nodeSearchField;

        private BlackboardPropertyListView _blackboardView;


        /// <summary>Behaviour Tree 에디터 설정 정보를 가져옵니다.</summary>
        public static BehaviourTreeEditorSettings Settings
        {
            get
            {
                if (_settings is null)
                {
                    string filter = $"t:{nameof(BehaviourTreeEditorSettings)}";
                    _settings = EditorHelper.FindAssetByName<BehaviourTreeEditorSettings>(filter);
                    Debug.Assert(_settings is not null, "BehaviourTreeEditorSettings asset not found.");
                }

                return _settings;
            }
        }

        
        /// <summary>현재 활성화된 Behaviour Tree 에디터 인스턴스를 가져옵니다.</summary>
        public static BehaviourTreeEditor Instance
        {
            get;
            private set;
        }

        
        /// <summary>현재 트리를 편집할 수 있는지 여부를 나타냅니다.</summary>
        public static bool CanEditTree
        {
            get;
            private set;
        }

        
        /// <summary>Behaviour Tree 에셋을 Editor View에 로딩 중인지 여부를 나타냅니다.</summary>
        public static bool IsLoadingTreeToView
        {
            get;
            private set;
        }

        
        /// <summary>현재 Behaviour Tree 뷰를 가져옵니다.</summary>
        public BehaviourTreeView View
        {
            get { return _treeView; }
        }

        
        /// <summary>현재 편집 중인 Behaviour Tree를 가져옵니다.</summary>
        public BehaviourTree Tree
        {
            get { return _tree; }
        }


        /// <summary>Unity 메뉴에서 Behaviour Tree 에디터 창을 엽니다.</summary>
        [MenuItem("Tools/Behaviour Tree Editor")]
        private static void OpenWindow()
        {
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("BT Editor");
            Instance = wnd;
        }

        
        /// <summary>Behaviour Tree 에셋을 더블클릭했을 때 에디터를 엽니다.</summary>
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

        
        /// <summary>에디터를 초기 상태로 초기화합니다.</summary>
        private void Initialize()
        {
            IsLoadingTreeToView = false;
            CanEditTree = false;

            this._inspectorView?.Clear();
            this._treeView?.ClearEditorView();
            this._blackboardView?.ClearBlackboardView();

            this.ResetTreeObjects();
        }

        
        /// <summary>트리 관련 객체들을 초기화합니다.</summary>
        private void ResetTreeObjects()
        {
            this._tree = null;
            this._treeRunner = null;
        }

        
        /// <summary>에디터 UI를 생성하고 초기화합니다.</summary>
        private void CreateGUI()
        {
            Instance = this;

            Debug.Assert(rootVisualElement is not null, "Root Visual Element is null.");
            Settings.behaviourTreeEditorXml.CloneTree(rootVisualElement);

            Debug.Assert(Settings.behaviourTreeEditorXml is not null, "BehaviourTreeEditorXml is null.");
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

        
        /// <summary>에디터가 활성화될 때 이벤트를 등록합니다.</summary>
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= this.OnPlayNodeStateChanged;
            EditorApplication.playModeStateChanged += this.OnPlayNodeStateChanged;

            Undo.undoRedoPerformed -= this.BehaviourEditorUndoPerformed;
            Undo.undoRedoPerformed += this.BehaviourEditorUndoPerformed;

            EditorSceneManager.sceneClosed -= this.OnSceneClosed;
            EditorSceneManager.sceneClosed += this.OnSceneClosed;

            AssemblyReloadEvents.afterAssemblyReload -= this.ResetTreeObjects;
            AssemblyReloadEvents.afterAssemblyReload += this.ResetTreeObjects;

            if (EditorApplication.isPlaying)
            {
                EditorApplication.update -= this.RuntimeUpdate;
                EditorApplication.update += this.RuntimeUpdate;
            }
        }

        
        /// <summary>에디터가 비활성화될 때 이벤트를 해제합니다.</summary>
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= this.OnPlayNodeStateChanged;
            EditorApplication.delayCall -= this.OnSelectionChange;
            EditorApplication.update -= this.RuntimeUpdate;

            Undo.undoRedoPerformed -= this.BehaviourEditorUndoPerformed;

            EditorSceneManager.sceneClosed -= this.OnSceneClosed;
            AssemblyReloadEvents.afterAssemblyReload -= this.ResetTreeObjects;
        }

        
        /// <summary>새로운 Behaviour Tree Asset이 추가되거나 제거될 때 호출됩니다.</summary>
        private void OnProjectChange()
        {
            if (_tree is not null && AssetDatabase.Contains(_tree) == false)
            {
                this.Initialize();
            }
            else
            {
                EditorApplication.delayCall -= this.OnSelectionChange;
                EditorApplication.delayCall += this.OnSelectionChange;
            }
        }

        
        /// <summary>씬이 닫힐 때 에디터를 초기화합니다.</summary>
        private void OnSceneClosed(Scene _)
        {
            this.Initialize();
        }

        
        /// <summary>플레이 모드에서 노드 뷰를 업데이트합니다.</summary>
        private void RuntimeUpdate()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            if (_treeRunner?.runtimeTree is null)
            {
                return;
            }

            _treeView?.UpdateNodeView();
        }

        
        /// <summary>언두/리두 작업이 수행될 때 에디터를 업데이트합니다.</summary>
        private void BehaviourEditorUndoPerformed()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            _treeView?.OnGraphEditorView(_tree);
            _inspectorView?.ClearInspectorView();
            _blackboardView?.RefreshItemsWhenUndoPerformed();

            AssetDatabase.SaveAssets();
        }

        
        /// <summary>플레이 모드 상태가 변경될 때 호출됩니다.</summary>
        private void OnPlayNodeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                {
                    EditorApplication.update -= this.RuntimeUpdate;
                    bool clickedNewAsset = this.TryGetTreeAsset(out BehaviourTree selectedTreeAsset);
                    this.ChangeBehaviourTree(clickedNewAsset ? selectedTreeAsset : _editorOnlyTree);
                    return;
                }

                case PlayModeStateChange.EnteredPlayMode:
                {
                    EditorApplication.update += this.RuntimeUpdate;
                    //Play mode 진입할 땐, 알아서 CreateGUI-OnSelectionChange가 호출됨.
                    return;
                }
            }
        }

        
        /// <summary>에디터에서 선택된 객체가 변경될 때 호출됩니다.</summary>
        private void OnSelectionChange()
        {
            bool foundTree = this.TryGetTreeAsset(out BehaviourTree selectedTreeAsset);

            if (foundTree && _tree != selectedTreeAsset)
            {
                this.ChangeBehaviourTree(selectedTreeAsset);
            }
        }

        
        /// <summary>현재 선택된 객체에서 Behaviour Tree를 찾습니다.</summary>
        private bool TryGetTreeAsset(out BehaviourTree tree)
        {
            UObject selectedObject = Selection.activeObject;

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

        
        /// <summary>에디터에서 편집할 Behaviour Tree를 변경합니다.</summary>
        private void ChangeBehaviourTree(BehaviourTree treeAsset)
        {
            if (treeAsset is null)
            {
                return;
            }

            CanEditTree = !Application.isPlaying;
            _tree = treeAsset;

            if (CanEditTree)
            {
                _editorOnlyTree = treeAsset;
            }

            bool isValidTreeRunner = _treeRunner is not null && _treeRunner.runtimeTree == _tree;
            bool openedEditorWindow = AssetDatabase.CanOpenAssetInEditor(_tree.GetInstanceID());

            if ((isValidTreeRunner && Application.isPlaying) || openedEditorWindow)
            {
                IsLoadingTreeToView = true;

                _treeView?.OnGraphEditorView(_tree);
                _inspectorView?.ClearInspectorView();
                _blackboardView?.ClearBlackboardView();
                _blackboardView?.OnBehaviourTreeChanged(_tree);

                IsLoadingTreeToView = false;
            }
        }
    }
}