using System;
using System.Collections.Generic;
using System.Linq;
using BehaviourSystem.BT.State;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviourSystem.BT
{
    public class GraphFactory
    {
#if UNITY_EDITOR
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
            GraphFactory.CreateGraphGroup(graphAsset);
            EditorUtility.SetDirty(graphAsset);
            return graphAsset;
        }


        public static BehaviourTree CreateBehaviorTreeGraph(GraphAsset graphAsset)
        {
            BehaviourTree graph = ScriptableObject.CreateInstance<BehaviourTree>();
            graph.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(graph, graphAsset);

            if (graph.entry == null)
            {
                graph.CreateNode(typeof(RootNode));
                graph.entry = graph.nodes[0];
            }

            return graph;
        }


        public static FiniteStateMachine CreateFiniteStateMachineGraph(GraphAsset graphAsset)
        {
            FiniteStateMachine graph = ScriptableObject.CreateInstance<FiniteStateMachine>();
            graph.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(graph, graphAsset);

            if (graph.entry == null)
            {
                graph.CreateNode(typeof(EnterState), new Vector2Int(0, 0));
                graph.CreateNode(typeof(ExitState), new Vector2Int(200, 0));
                graph.CreateNode(typeof(AnyState), new Vector2Int(-200, 0));
                graph.currentState = graph.nodes[0] as StateNodeBase;
                graph.entry = graph.nodes[0];
            }

            return graph;
        }


        public static void CreateGraphAndAddToAsset(GraphAsset graphAsset)
        {
            switch (graphAsset.graphType)
            {
                case EGraphType.BT: graphAsset.graph = GraphFactory.CreateBehaviorTreeGraph(graphAsset); break;

                case EGraphType.FSM: graphAsset.graph = GraphFactory.CreateFiniteStateMachineGraph(graphAsset); break;
            }

            Debug.Assert(graphAsset.graph is not null, "graph is null");
        }


        public static void CreateGraphGroup(GraphAsset graphAsset)
        {
            if (graphAsset.graphGroup != null)
            {
                return;
            }

            graphAsset.graphGroup = ScriptableObject.CreateInstance<GraphGroup>();
            graphAsset.graphGroup.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(graphAsset.graphGroup, graphAsset);
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
}