using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace BehaviourSystemEditor.BT
{
    [UxmlElement]
    public partial class InspectorView : InspectorElement
    {
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
            this.ClearInspectorView();
            base.Add(element);
        }


        private void DrawInspectorGUI()
        {
            if (this._editor?.target is null || this._editor.serializedObject.targetObject is null)
            {
                this.ClearInspectorView();
                return;
            }

            this._editor.OnInspectorGUI();
        }
    }
}