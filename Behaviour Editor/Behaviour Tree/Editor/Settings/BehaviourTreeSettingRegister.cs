using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    //https://docs.unity3d.com/ScriptReference/SettingsProvider.html
    public class BehaviourTreeSettingRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            return new SettingsProvider("Project/Behaviour Tree Settings", SettingsScope.Project)
            {
                label = "Behaviour System",
                activateHandler = ProvideSettingHandler
            };
        }

        private static void ProvideSettingHandler(string searchContext, VisualElement rootElement)
        {
            var settings = EditorHelper.FindAssetByName<BehaviourTreeEditorSettings>($"t:{nameof(BehaviourTreeEditorSettings)}");

            rootElement.Add(new InspectorElement(settings));
            rootElement.Bind(new SerializedObject(settings));
        }
    }
}