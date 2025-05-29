using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    public class BehaviourTreeEditorSettings : ScriptableObject
    {
        [Header("Grid Options")]
        public float maxZoomScale = 1.5f;

        public float minZoomScale = 0.5f;

        [Header("Highlight Options")]
        public float minimumFocusingDuration = 0.5f;
        
        public Color miniMapBackgroundColor = new Color32(30, 30, 30, 255);
        
        public Color nodeGroupColor = new Color32(65, 65, 65, 255);

        public Color nodeAppearingColor = new Color32(54, 154, 204, 255);
        public Color nodeDisappearingColor = new Color32(24, 93, 125, 255);

        public Color edgeAppearingColor = new Color32(54, 154, 204, 255);
        public Color edgeDisappearingColor = new Color32(200, 200, 200, 255);

        public Color portAppearingColor = new Color32(54, 154, 204, 255);
        public Color portDisappearingColor = new Color32(200, 200, 200, 255);

        [Header("Runtime Options")]
        public uint maxUpdateRate = 240;
        public float editorUpdateInterval = 0.1f; // 0.1초마다 업데이트 (10Hz)

        [Header("Layout References")]
        public VisualTreeAsset behaviourTreeEditorXml;

        public StyleSheet behaviourTreeStyle;
        public VisualTreeAsset nodeViewXml;
        public StyleSheet nodeViewStyle;
        public VisualTreeAsset blackboardPropertyViewXml;
        public StyleSheet blackboardPropertyViewStyle;
    }
}
