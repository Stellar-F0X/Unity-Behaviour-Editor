using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    //TODO: Settings에 코드 제네레이션할 타입들을 저장할건데, 그걸 여기서 바인딩하는 로직을 추가할거라, 굳이 UI Toolkit을 사용했음.  
    [CustomEditor(typeof(BehaviourTreeEditorSettings))]
    public class BehaviourTreeEditorSettingsCustomInspector : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            BehaviourTreeEditorSettings settings = (BehaviourTreeEditorSettings)target;
            VisualElement inspectorView = settings.editorSettingsXml.CloneTree();

            FloatField updateInterval = inspectorView.Q<FloatField>("update-interval");
            FloatField highlightDuration = inspectorView.Q<FloatField>("highlight-duration");
            ColorField miniMapBackgroundColor = inspectorView.Q<ColorField>("minimap-background-color");
            ColorField nodeGroupColor = inspectorView.Q<ColorField>("node-group-color");
            ColorField nodeSuccessColor = inspectorView.Q<ColorField>("node-success-color");
            ColorField nodeFailureColor = inspectorView.Q<ColorField>("node-failure-color");
            GradientField nodeGradientField = inspectorView.Q<GradientField>("node-gradient-field");
            GradientField edgeGradientField = inspectorView.Q<GradientField>("edge-gradient-field");
            
            updateInterval.value = settings.nodeViewUpdateInterval;
            highlightDuration.value = settings.nodeViewHighlightingDuration;
            miniMapBackgroundColor.value = settings.miniMapBackgroundColor;
            nodeGroupColor.value = settings.nodeGroupColor;
            nodeSuccessColor.value = settings.nodeSuccessColor;
            nodeFailureColor.value = settings.nodeFailureColor;
            nodeGradientField.value = settings.nodeStatusLinearColor;
            edgeGradientField.value = settings.edgeStatusLinearColor;

            
            updateInterval.RegisterValueChangedCallback(evt =>
            {
                settings.nodeViewUpdateInterval = Mathf.Clamp(evt.newValue, 0.01f, settings.nodeViewHighlightingDuration);
                updateInterval.SetValueWithoutNotify(settings.nodeViewUpdateInterval);
                EditorUtility.SetDirty(settings);
            });

            highlightDuration.RegisterValueChangedCallback(evt =>
            {
                settings.nodeViewHighlightingDuration = Mathf.Max(evt.newValue, settings.nodeViewUpdateInterval);
                highlightDuration.SetValueWithoutNotify(settings.nodeViewHighlightingDuration);
                EditorUtility.SetDirty(settings);
            });

            miniMapBackgroundColor.RegisterValueChangedCallback(evt =>
            {
                settings.miniMapBackgroundColor = evt.newValue;
                EditorUtility.SetDirty(settings);
            });

            nodeGroupColor.RegisterValueChangedCallback(evt =>
            {
                settings.nodeGroupColor = evt.newValue;
                EditorUtility.SetDirty(settings);
            });
            
            nodeSuccessColor.RegisterValueChangedCallback(evt =>
            {
                settings.nodeSuccessColor = evt.newValue;
                EditorUtility.SetDirty(settings);
            });

            nodeFailureColor.RegisterValueChangedCallback(evt =>
            {
                settings.nodeFailureColor = evt.newValue;
                EditorUtility.SetDirty(settings);
            });
            
            nodeGradientField.RegisterValueChangedCallback(evt =>
            {
                settings.nodeStatusLinearColor = evt.newValue;
                EditorUtility.SetDirty(settings);
            });

            edgeGradientField.RegisterValueChangedCallback(evt =>
            {
                settings.edgeStatusLinearColor = evt.newValue;
                EditorUtility.SetDirty(settings);
            });

            return inspectorView;
        }
    }
}