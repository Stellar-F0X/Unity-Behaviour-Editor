using System.Collections.Generic;
using BehaviourSystem.BT.State;
using UnityEditor.Experimental.GraphView;

namespace BehaviourSystemEditor.BT
{
    public class StateCreationWindow : CreationWindowBase
    {
        protected override void RegisterSubSearchTrees(List<SearchTreeEntry> searchTree, SearchWindowContext context)
        {
            searchTree.AddRange(this.CreateSearchTreeEntry<CustomStateNode>("State", type => this.CreateNode(type, context)));
        }
    }
}