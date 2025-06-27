using BehaviourSystem.BT.State;
using UnityEditor;

namespace BehaviourSystemEditor.BT
{
    [CustomEditor(typeof(StateNodeBase), true)]
    public class StateNodeBaseCustomEditor : NodeBaseCustomEditor
    {
        public override void OnInspectorGUI()
        {
            base.ParentSerializedField();
            
            base.ChildSerializedFields(serializedObject.FindProperty("position"));
        }
    }
}