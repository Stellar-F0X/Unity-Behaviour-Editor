using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    public class BehaviourTreeEditorSettings : ScriptableObject
    {
        public Color miniMapBackgroundColor = new Color32(30, 30, 30, 255);
        public Color nodeGroupColor = new Color32(65, 65, 65, 255);
        
        
        [Header("View Color Options")]
        public Color nodeSuccessColor = new Color32(0, 100, 0, 255);
        public Color nodeFailureColor = new Color32(100, 0, 0, 255);
        
        
        [Space]
        public Gradient nodeStatusLinearColor;
        public Gradient edgeStatusLinearColor;

        
        [Header("Runtime Options")]
        public float nodeViewHighlightingDuration = 0.2f;
        public float nodeViewUpdateInterval = 0.04f; // 0.1초마다 업데이트 (10Hz)

        
        [Header("Layout References")]
        [HideInInspector] public VisualTreeAsset behaviourTreeEditorXml;
        [HideInInspector] public StyleSheet behaviourTreeStyle;
        [HideInInspector] public VisualTreeAsset nodeViewXml;
        [HideInInspector] public StyleSheet nodeViewStyle;
        [HideInInspector] public VisualTreeAsset blackboardPropertyViewXml;
        [HideInInspector] public StyleSheet blackboardPropertyViewStyle;
        [HideInInspector] public VisualTreeAsset editorSettingsXml;
        [HideInInspector] public StyleSheet editorSettingsStyle;
    }
}
