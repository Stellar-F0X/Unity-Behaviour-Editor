using UnityEditor.Experimental.GraphView;

namespace BehaviourSystemEditor.BT
{
    /// <summary>
    /// This class was originally created to allow users to control the zoom level while the graph editor is running, 
    /// but the feature was canceled, so the class is no longer necessary. 
    /// </summary>
    public class GraphZoomer : ContentZoomer
    {
        public GraphZoomer(float maxZoomScale, float minZoomScale)
        {
            base.maxScale = maxZoomScale;
            base.minScale = minZoomScale;
        }
    }
}