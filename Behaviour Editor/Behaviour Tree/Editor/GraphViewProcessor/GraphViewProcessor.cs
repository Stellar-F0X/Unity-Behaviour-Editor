using System.Collections.Generic;
using BehaviourSystem.BT;
using UnityEditor.Experimental.GraphView;

namespace BehaviourSystemEditor.BT
{
    public abstract class GraphViewProcessor
    {
        protected BehaviourGraphView graphView
        {
            get { return BehaviourSystemEditor.Instance.View; }
        }

        
        public virtual void NotifyNodePositionChanged(List<GraphElement> elements) { }
        
        public abstract bool TryConnectNodesByEdge(NodeView connectionSource, NodeView connectionTarget, out Edge linkedEdge);
        
        public abstract void CreateAndConnectNodes(GraphAsset graphAsset);

        public abstract void OnDeleteSelectionElements(List<ISelectable> selection);
        
        public abstract NodeView RecreateNodeViewOnLoad(NodeBase node);

        public abstract void TryDisconnectParentToChild(NodeView parentNodeView);

        public abstract void TryDisconnectChildToParent(NodeView childNodeView);
        
        public abstract void DisconnectNodesByEdge(GraphAsset graphAsset, Edge edge);
        
        public abstract void ConnectNodesByEdges(GraphAsset graphAsset, List<Edge> edges);
    }
}