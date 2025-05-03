using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using UnityEditor.UIElements;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    [UxmlElement]
    public partial class InspectorView : InspectorElement
    {
        private bool _isBorrowing;
        private Editor _editor;
        
        
        public void ClearInspectorView()
        {
            base.Clear();
            Object.DestroyImmediate(_editor);
        }


        public void UpdateSelection(NodeView view)
        {
            this.ClearInspectorView();
            
            if (view is not null && view.targetNode is not null)
            {
                this._editor = Editor.CreateEditor(view.targetNode);
                base.Add(new IMGUIContainer(this.DrawInspectorGUI));
            }
        }


        public void BorrowInspectorGUI(VisualElement element)
        {
            if (_isBorrowing)
            {
                Debug.LogError("Already borrowing inspector GUI");
                return;
            }

            this.ClearInspectorView();
            base.Add(element);
        }


        private void DrawInspectorGUI()
        {
            if (this._editor is null || this._editor.target is null || this._editor.serializedObject.targetObject is null)
            {
                this.ClearInspectorView();
                return;
            }

            this._editor.OnInspectorGUI();
        }
    }
}