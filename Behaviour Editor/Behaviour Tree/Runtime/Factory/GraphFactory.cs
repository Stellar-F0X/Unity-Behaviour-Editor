using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BehaviourSystem.BT.State;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace BehaviourSystem.BT
{
    public static class GraphFactory
    {
        private readonly static List<GraphCloner> _GraphCloner = new List<GraphCloner>(2)
        {
            new BehaviorTreeCloner(),
            new StateMachineCloner()
        };

#if UNITY_EDITOR
        public static void RemoveObjectFromAssetAndDestroyImmediate(Object graphAsset)
        {
            if (graphAsset == null)
            {
                Debug.LogError("graphAsset is null");
                return;
            }

            AssetDatabase.RemoveObjectFromAsset(graphAsset);
            Object.DestroyImmediate(graphAsset, true);
        }
#endif

#region Create Methods

        //TODO: 리팩토링
        //그래프를 긁어서 모든 노드와 그 노드의 필드 세터를 배열로 얻어옴.
        //가져온 모든 노드를 Dictionary<GraphAsset, Nodes[]>로 분배.
        public static GraphAsset CloneGraph(BehaviorSystemRunner runner, GraphAsset targetGraph)
        {
            BlackboardAsset clonedBlackboardAsset = targetGraph.blackboardAsset?.Clone();
            GraphAsset[] graphAssets = GraphCloner.CollectCurrentAndSubGraphAssets(targetGraph);
            GraphAsset[] clonedGraphAssets = ArrayPool<GraphAsset>.Shared.Rent(graphAssets.Length);

            for (int index = 0; index < graphAssets.Length; ++index)
            {
                GraphAsset originalGraphAsset = graphAssets[index];

                clonedGraphAssets[index] = Object.Instantiate(originalGraphAsset);
                clonedGraphAssets[index].isRootGraph = index == 0;
#if UNITY_EDITOR
                clonedGraphAssets[index].graphGroup = originalGraphAsset.graphGroup.Clone();
#endif
                clonedGraphAssets[index].graph = _GraphCloner[(int)originalGraphAsset.graphType].CloneGraph(runner, originalGraphAsset.graph, clonedBlackboardAsset);
            }

            foreach (GraphAsset clonedGraph in clonedGraphAssets)
            {
                foreach (NodeBase node in clonedGraph.graph.nodes)
                {
                    
                    
                    node.runner = runner;
                    node.PostCreation();
                }
            }

            GraphAsset resultGraph = clonedGraphAssets[0];
            ArrayPool<GraphAsset>.Shared.Return(clonedGraphAssets);
            return resultGraph;
        }


#if UNITY_EDITOR
        public static TGraphAsset CreateGraphAsset<TGraphAsset>(GraphAsset parentGraphAsset = null) where TGraphAsset : GraphAsset
        {
            TGraphAsset graphAsset = ScriptableObject.CreateInstance<TGraphAsset>();
            graphAsset.hideFlags = HideFlags.HideInHierarchy;
            GraphFactory.ValidateOrRenewGuid(graphAsset);

            if (parentGraphAsset != null)
            {
                parentGraphAsset.AddSubGraphAsset(graphAsset);
            }

            GraphFactory.CreateGraphAndAddToAsset(graphAsset);
            GraphGroup.CreateGraphGroup(graphAsset);
            EditorUtility.SetDirty(graphAsset);
            return graphAsset;
        }


        public static void CreateGraphAndAddToAsset(GraphAsset graphAsset)
        {
            switch (graphAsset.graphType)
            {
                case EGraphType.BT: graphAsset.graph = BehaviourTreeAsset.CreateBehaviorTreeGraph(graphAsset); break;

                case EGraphType.FSM: graphAsset.graph = FiniteStateMachineAsset.CreateFiniteStateMachineGraph(graphAsset); break;
            }

            Debug.Assert(graphAsset.graph is not null, "graph is null");
        }
#endif

#endregion


#region GUID Methods

#if UNITY_EDITOR
        public static int StringToHash(in string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Blackboard key cannot be null or empty.");
                return -1;
            }

            return Animator.StringToHash(key);
        }
        
        /// <summary>  </summary>
        /// <param name="graphAsset"></param>
        /// <returns> Changed </returns>
        public static bool ValidateOrRenewGuid(GraphAsset graphAsset)
        {
            if (graphAsset.guid.IsEmpty() || GraphFactory.IsGraphGuidDuplicated(graphAsset))
            {
                graphAsset.guid = UGUID.Create();
                GraphFactory.ChangeNodesAndGroupGuidOfGraph(graphAsset);
                EditorUtility.SetDirty(graphAsset);
                return true;
            }

            return false;
        }


        public static void ChangeNodesAndGroupGuidOfGraph(GraphAsset graphAsset)
        {
            if (graphAsset?.graph?.nodes == null || graphAsset.graph.nodes.Count == 0)
            {
                return;
            }

            foreach (var nodeBase in graphAsset.graph.nodes)
            {
                UGUID originalGuid = nodeBase.guid;
                nodeBase.guid = UGUID.Create();

                if (graphAsset.graphGroup.dataList.Count == 0)
                {
                    continue;
                }

                foreach (GroupData groupData in graphAsset.graphGroup.dataList)
                {
                    if (groupData.containedNodeCount > 0 && groupData.Contains(originalGuid))
                    {
                        groupData.RemoveNodeGuid(originalGuid);
                        groupData.AddNodeGuid(nodeBase.guid);
                    }
                }
            }
        }


        public static bool IsGraphGuidDuplicated(GraphAsset graphAsset)
        {
            if (graphAsset == null || graphAsset.guid.IsEmpty())
            {
                return false;
            }

            string[] guids = AssetDatabase.FindAssets("t:BehaviourTree");

            if (guids is null || guids.Length == 0)
            {
                return false;
            }

            int count = 0;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                GraphAsset asset = AssetDatabase.LoadAssetAtPath<GraphAsset>(assetPath);

                if (graphAsset.guid != asset.guid)
                {
                    continue;
                }

                if (count++ > 1) // 자기 자신 포함해서 2개 이상이면 중복
                {
                    return true;
                }
            }

            return false;
        }
    }
#endif

#endregion
}