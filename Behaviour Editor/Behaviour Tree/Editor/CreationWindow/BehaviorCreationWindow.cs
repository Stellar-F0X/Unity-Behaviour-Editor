using System.Collections.Generic;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    public class BehaviorCreationWindow : CreationWindowBase
    {
        protected override void RegisterSubSearchTrees(List<SearchTreeEntry> searchTree, SearchWindowContext context)
        {
            searchTree.AddRange(this.CreateSearchTreeEntry<ActionNode>("Action", type => this.CreateNode(type, context)));

            searchTree.AddRange(this.CreateSearchTreeEntry<CompositeNode>("Composite", type => this.CreateNode(type, context)));

            searchTree.AddRange(this.CreateSearchTreeEntry<DecoratorNode>("Decorator", type => this.CreateNode(type, context)));

            searchTree.AddRange(this.CreateSearchTreeEntry<SubGraphNode>("Graph", type => this.AllocateSubGraphAssets(this.CreateNode(type, context))));
        }
    }
}