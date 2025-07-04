﻿using System;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [CreateAssetMenu(fileName = "New Behaviour Tree", menuName = "Behaviour Tree/Behaviour Tree Asset")]
    public sealed class BehaviourTree : ScriptableObject, IEquatable<BehaviourTree>
    {
        [HideInInspector]
        public BehaviourNodeSet nodeSet;
        
        [ReadOnly]
        public Blackboard blackboard;

#if UNITY_EDITOR
        [HideInInspector]
        public GroupDataSet groupDataSet;
#endif
        
        [field: SerializeField, ReadOnly]
        public UGUID treeGuid
        {
            get;
            internal set;
        }


        internal static BehaviourTree MakeRuntimeTree(BehaviourTreeRunner treeRunner, BehaviourTree targetTree)
        {
            Debug.Assert(treeRunner is not null && targetTree is not null, "BehaviourActor or BehaviourTree is null.");

            BehaviourTree runtimeTree = Instantiate(targetTree);

            Debug.Assert(runtimeTree is not null, "Failed to create runtime tree.");

#if UNITY_EDITOR
            runtimeTree.groupDataSet = targetTree.groupDataSet.Clone();
#endif
            runtimeTree.blackboard = targetTree.blackboard?.Clone();
            runtimeTree.nodeSet = targetTree.nodeSet.Clone(runtimeTree.blackboard);
            
            foreach (NodeBase currentNode in runtimeTree.nodeSet.nodeList)
            {
                currentNode.runner = treeRunner;
                currentNode.PostTreeCreation();
            }

            return runtimeTree;
        }


        public bool Equals(BehaviourTree other)
        {
            if (other is null || this.GetType() != other.GetType())
            {
                return false;
            }

            if (this.treeGuid == other.treeGuid)
            {
                return false;
            }

            if (this.nodeSet.nodeList.Count != other.nodeSet.nodeList.Count)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < this.nodeSet.nodeList.Count; i++)
                {
                    if (this.nodeSet.nodeList[i].guid != other.nodeSet.nodeList[i].guid)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}