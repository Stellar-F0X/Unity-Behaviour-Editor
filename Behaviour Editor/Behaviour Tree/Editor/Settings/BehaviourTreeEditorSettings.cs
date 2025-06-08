using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    public class BehaviourTreeEditorSettings : ScriptableObject
    {
        [Header("View Options")]
        public float maxZoomScale = 1.5f;
        public float minZoomScale = 0.5f;
        public Color miniMapBackgroundColor = new Color32(30, 30, 30, 255);
        public Color nodeGroupColor = new Color32(65, 65, 65, 255);
        
        [Header("Highlight Options")]
        public float highlightingDuration = 0.5f;
        public Color nodeAppearingColor = new Color32(54, 154, 204, 255);
        public Color nodeDisappearingColor = new Color32(24, 93, 125, 255);
        public Color edgeAppearingColor = new Color32(54, 154, 204, 255);
        public Color edgeDisappearingColor = new Color32(200, 200, 200, 255);
        
        [Header("Runtime Options")]
        public float nodeViewUpdateInterval = 0.1f; // 0.1초마다 업데이트 (10Hz)

        [Header("Layout References")]
        [HideInInspector] public VisualTreeAsset behaviourTreeEditorXml;
        [HideInInspector] public StyleSheet behaviourTreeStyle;
        [HideInInspector] public VisualTreeAsset nodeViewXml;
        [HideInInspector] public StyleSheet nodeViewStyle;
        [HideInInspector] public VisualTreeAsset blackboardPropertyViewXml;
        [HideInInspector] public StyleSheet blackboardPropertyViewStyle;
    }
}
