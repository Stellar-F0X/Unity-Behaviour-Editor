using UnityEditor;

namespace BehaviourSystemEditor.BT
{
    public static class CreationNodeScript
    {
        [MenuItem("Assets/Create/Behavior System/Scripting/BT/Action Node")]
        public static void CreateActionNodeMenuItem()
        {
            string path = EditorHelper.FindAssetPath("NewActionNode.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewActionNode.cs");
        }
        
        
        [MenuItem("Assets/Create/Behavior System/Scripting/BT/Composite Node")]
        public static void CreateCompositeNodeMenuItem()
        {
            string path = EditorHelper.FindAssetPath("NewCompositeNode.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewCompositeNode.cs");
        }
        
        
        [MenuItem("Assets/Create/Behavior System/Scripting/BT/Decorator Node")]
        public static void CreateDecoratorNodeMenuItem()
        {
            string path = EditorHelper.FindAssetPath("NewDecoratorNode.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewDecoratorNode.cs");
        }


        [MenuItem("Assets/Create/Behavior System/Scripting/FSM/State Node")]
        public static void CreateStateNodeMenuItem()
        {
            string path = EditorHelper.FindAssetPath("NewStateNode.cs t:TextAsset");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(path, "NewStateNode.cs");
        }
    }
}
