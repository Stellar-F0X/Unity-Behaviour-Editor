using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace BehaviourTechnique.BehaviourTreeEditor.Setting
{
    //https://docs.unity3d.com/ScriptReference/SettingsProvider.html
    public class BehaviourTreeSettingRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            return new SettingsProvider("Project/BehaviourTreeProjectSettings", SettingsScope.Project) 
            {
                label = "Behaviour Tree Editor",
                activateHandler = ProvideSettingHandler
            };
        }

        private static void ProvideSettingHandler(string searchContext, VisualElement rootElement)
        {
            Label title = new Label();
            title.text = "Behaviour Tree Settings";
            title.AddToClassList("title");
            rootElement.Add(title);

            VisualElement properties = new VisualElement();
            properties.style.flexDirection = FlexDirection.Column;
            properties.AddToClassList("property-list");
            rootElement.Add(properties);

            var settings = EditorUtility.FindAssetByName<BehaviourTreeEditorSettings>(
                $"t:{nameof(BehaviourTreeEditorSettings)}"
            );

            properties.Add(new InspectorElement(settings));
            rootElement.Bind(new SerializedObject(settings));
        }
    }
}
