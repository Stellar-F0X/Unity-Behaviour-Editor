using UnityEditor;

namespace BehaviourSystemEditor.BT
{
    public static class CreationNodeScript
    {
        [MenuItem("Assets/Create/Behaviour Tree/Scripting/Action Node")]
        public static void CreateActionNodeMenuItem()
        {
            string path = EditorHelper.FindAssetPath("NewActionNode.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewActionNode.cs");
        }
        
        
        [MenuItem("Assets/Create/Behaviour Tree/Scripting/Composite Node")]
        public static void CreateCompositeNodeMenuItem()
        {
            string path = EditorHelper.FindAssetPath("NewCompositeNode.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewCompositeNode.cs");
        }
        
        
        [MenuItem("Assets/Create/Behaviour Tree/Scripting/Decorator Node")]
        public static void CreateDecoratorNodeMenuItem()
        {
            string path = EditorHelper.FindAssetPath("NewDecoratorNode.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewDecoratorNode.cs");
        }
    }
}
