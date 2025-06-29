using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    public class StateNodeEdgeConnectorListener : EdgeConnectorListener
    {
        protected override void CreateAndLinkFromOriginalToNewNode(NodeView sourceNodeView, NodeView targetNodeView, Vector2 position)
        {
            var processor = BehaviorEditor.Instance.view.graphViewProcessor;
            
            processor.TryDisconnectParentToChild(sourceNodeView);
        }
        
        
        protected override void CreateAndLinkFromNewToOriginalNode(NodeView sourceNodeView, NodeView targetNodeView, Vector2 position)
        {
            var processor = BehaviorEditor.Instance.view.graphViewProcessor;
            
            processor.TryDisconnectChildToParent(targetNodeView);
        }
    }
}