using System;
using System.Collections.Generic;
using BehaviourSystem.BT;
using BehaviourSystem.BT.State;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    public class StateCreationWindow : CreationWindowBase
    {
        protected override void RegisterSubSearchTrees(List<SearchTreeEntry> searchTree, SearchWindowContext context)
        {
            searchTree.AddRange(this.CreateSearchTreeEntry<CustomStateNode>("State", type => this.CreateNode(type, context)));

            searchTree.AddRange(this.CreateSearchTreeEntry<SubGraphState>("Graph", type => this.AllocateSubGraphAssets(this.CreateNode(type, context))));
        }
    }
}