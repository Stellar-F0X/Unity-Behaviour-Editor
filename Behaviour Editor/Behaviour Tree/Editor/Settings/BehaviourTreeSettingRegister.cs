using BehaviourSystem.BT;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace BehaviourSystemEditor.BT
{
    //https://docs.unity3d.com/ScriptReference/SettingsProvider.html
    public class BehaviourTreeSettingRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            return new SettingsProvider("Project/BehaviourTreeProjectSettings", SettingsScope.Project)
            {
                label = "Behaviour Tree",
                activateHandler = ProvideSettingHandler
            };
        }

        private static void ProvideSettingHandler(string searchContext, VisualElement rootElement)
        {
            Label title = new Label();
            title.text = "Behaviour Tree Settings";
            title.AddToClassList("title");
            title.AddToClassList("header-label");
            title.style.fontSize = 19;
            title.style.paddingLeft = new StyleLength(10f);
            title.style.paddingTop = new StyleLength(2f);
            title.style.unityFontStyleAndWeight = FontStyle.Bold; 
            rootElement.Add(title);

            VisualElement properties = new VisualElement();
            properties.style.flexDirection = FlexDirection.Column;
            properties.AddToClassList("property-list");
            rootElement.Add(properties);

            var settings = EditorHelper.FindAssetByName<BehaviourTreeEditorSettings>($"t:{nameof(BehaviourTreeEditorSettings)}");

            properties.Add(new InspectorElement(settings));
            rootElement.Bind(new SerializedObject(settings));
        }
    }
}