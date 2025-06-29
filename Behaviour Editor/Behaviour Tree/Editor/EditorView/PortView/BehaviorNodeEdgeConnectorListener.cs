using BehaviourSystem.BT;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    public class BehaviorNodeEdgeConnectorListener : EdgeConnectorListener
    {
        protected override void CreateAndLinkFromOriginalToNewNode(NodeView sourceNodeView, NodeView targetNodeView, Vector2 position)
        {
            var processor = BehaviorEditor.Instance.view.graphViewProcessor;

            processor.TryDisconnectParentToChild(sourceNodeView);

            if (processor.TryConnectNodesByEdge(sourceNodeView, targetNodeView, out _) && BehaviorEditor.Instance.graph.graph is BehaviourTree tree)
            {
                tree.AddChild((BehaviourNodeBase)sourceNodeView.targetNode, (BehaviourNodeBase)targetNodeView.targetNode);
            }
        }


        protected override void CreateAndLinkFromNewToOriginalNode(NodeView sourceNodeView, NodeView targetNodeView, Vector2 position)
        {
            var processor = BehaviorEditor.Instance.view.graphViewProcessor;

            processor.TryDisconnectChildToParent(targetNodeView);

            if (processor.TryConnectNodesByEdge(sourceNodeView, targetNodeView, out _) && BehaviorEditor.Instance.graph.graph is BehaviourTree tree)
            {
                tree.AddChild((BehaviourNodeBase)sourceNodeView.targetNode, (BehaviourNodeBase)targetNodeView.targetNode);
            }
        }
    }
}