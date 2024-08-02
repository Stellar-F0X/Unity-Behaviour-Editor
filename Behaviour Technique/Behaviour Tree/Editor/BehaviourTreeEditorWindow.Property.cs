using BehaviourTechnique.BehaviourTreeEditor.Setting;
using UnityEngine.UIElements;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public partial class BehaviourTreeEditorWindow
    {
        public static BehaviourTreeEditorWindow Instance
        {
            get;
            private set;
        }

        public static VisualTreeAsset BehaviourTreeEditorXml
        {
            get { return EditorUtility.FindAsset<VisualTreeAsset>("Behaviour Technique t:Folder", "Behaviour Tree/Layout/BehaviourTreeEditor.uxml"); }
        }

        public static StyleSheet BehaviourTreeStyle
        {
            get { return EditorUtility.FindAsset<StyleSheet>("Behaviour Technique t:Folder", "Behaviour Tree/Layout/BehaviourTreeEditorStyle.uss"); }
        }

        public static VisualTreeAsset NodeViewXml
        {
            get { return EditorUtility.FindAsset<VisualTreeAsset>("Behaviour Technique t:Folder", "Behaviour Tree/Layout/NodeView.uxml"); }
        }

        public static StyleSheet NodeViewStyle
        {
            get { return EditorUtility.FindAsset<StyleSheet>("Behaviour Technique t:Folder", "Behaviour Tree/Layout/NodeViewStyle.uss"); }
        }

        public static BehaviourTreeEditorSettings Settings
        {
            get { return EditorUtility.FindAssetByName<BehaviourTreeEditorSettings>($"t: {nameof(BehaviourTreeEditorSettings)}"); }
        }
    }
}
