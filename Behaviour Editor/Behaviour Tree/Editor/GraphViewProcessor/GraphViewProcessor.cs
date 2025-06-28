using System.Collections.Generic;
using BehaviourSystem.BT;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    public abstract class GraphViewProcessor
    {
        protected CreationWindowBase _creationWindow;


        public CreationWindowBase GetGraphNodeCreationWindow()
        {
            if (_creationWindow is null)
            {
                _creationWindow = this.CreateGraphNodeCreationWindow();
            }
            
            Debug.Assert(_creationWindow != null, "CreationWindow is null");

            return _creationWindow;
        }
        
        
        public virtual void NotifyNodePositionChanged(List<GraphElement> elements, BehaviourGraphView graphView) { }
        
        public abstract bool TryConnectNodesByEdge(NodeView connectionSource, NodeView connectionTarget, out Edge linkedEdge);
        
        public abstract void CreateAndConnectNodes(GraphAsset graphAsset, BehaviourGraphView graphView);

        public abstract void OnDeleteSelectionElements(List<ISelectable> selection);
        
        public abstract NodeView RecreateNodeViewOnLoad(NodeBase node);

        public abstract void TryDisconnectParentToChild(NodeView parentNodeView);

        public abstract void TryDisconnectChildToParent(NodeView childNodeView);
        
        public abstract void DisconnectNodesByEdge(GraphAsset graphAsset, Edge edge);
        
        public abstract void ConnectNodesByEdges(GraphAsset graphAsset, List<Edge> edges);
        
        protected abstract CreationWindowBase CreateGraphNodeCreationWindow();
    }
}