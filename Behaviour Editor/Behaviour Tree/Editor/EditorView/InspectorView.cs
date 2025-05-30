using UnityEditor;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using UnityEditor.UIElements;

namespace BehaviourSystemEditor.BT
{
    [UxmlElement]
    public partial class InspectorView : InspectorElement
    {
        private Editor _editor;


        public void ClearInspectorView()
        {
            base.Clear();
            this._editor = null;
            Object.DestroyImmediate(_editor);
        }


        public void UpdateSelection(NodeView view)
        {
            base.Clear();
            Object.DestroyImmediate(this._editor);
            this._editor = Editor.CreateEditor(view.targetNode);
            base.Add(new IMGUIContainer(DrawInspectorGUI));
        }


        private void DrawInspectorGUI()
        {
            if (this._editor.target is null || this._editor.serializedObject.targetObject is null)
            {
                return;
            }
            
            this._editor.OnInspectorGUI();
        }
    }
}