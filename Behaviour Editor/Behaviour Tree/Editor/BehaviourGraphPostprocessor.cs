using System;
using BehaviourSystem.BT;
using BehaviourSystem.BT.State;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    public class BehaviourGraphPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
                GraphAsset graphAsset = AssetDatabase.LoadAssetAtPath<GraphAsset>(path);

                if (graphAsset != null)
                {
                    bool changed = false;

                    if (graphAsset.guid.IsEmpty() || IsGuidDuplicated(graphAsset))
                    {
                        graphAsset.guid = UGUID.Create();
                        ChangeNodesOfTreeGuid(graphAsset);
                        EditorUtility.SetDirty(graphAsset);
                        changed = true;
                    }

                    changed |= TryInitializeGraph(graphAsset);
                    changed |= TryInitializeGroupDataSet(graphAsset);

                    if (changed)
                    {
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }



        private static void ChangeNodesOfTreeGuid(GraphAsset graphAsset)
        {
            if (graphAsset.graph == null || graphAsset.graph.nodes.Count == 0)
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



        private static bool TryInitializeGraph(GraphAsset graphAsset)
        {
            if (graphAsset.graph != null)
            {
                return false;
            }

            switch (graphAsset.graphType)
            {
                case EGraphType.BehaviourTree: graphAsset.graph = ScriptableObject.CreateInstance<BehaviourTree>(); break;

                case EGraphType.StateMachine: graphAsset.graph = ScriptableObject.CreateInstance<FiniteStateMachine>(); break;
            }
            
            graphAsset.graph.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(graphAsset.graph, graphAsset);

            if (graphAsset.graph.entry == null)
            {
                switch (graphAsset.graphType)
                {
                    case EGraphType.BehaviourTree:
                        graphAsset.graph.CreateNode(typeof(RootNode));
                        graphAsset.graph.entry = graphAsset.graph.nodes[0]; 
                        break;

                    case EGraphType.StateMachine:
                        graphAsset.graph.CreateNode(typeof(EnterState), new Vector2Int(0, 0)); 
                        graphAsset.graph.CreateNode(typeof(ExitState), new Vector2Int(200, 0));
                        graphAsset.graph.CreateNode(typeof(AnyState), new Vector2Int(-200, 0));
                        graphAsset.graph.entry = graphAsset.graph.nodes[0];
                        break;
                }
                
                EditorUtility.SetDirty(graphAsset);
            }

            return true;
        }



        private static bool TryInitializeGroupDataSet(GraphAsset graphAsset)
        {
            if (graphAsset.graphGroup != null)
            {
                return false;
            }
            
            graphAsset.graphGroup = ScriptableObject.CreateInstance<GraphGroup>();
            graphAsset.graphGroup.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(graphAsset.graphGroup, graphAsset);
            return false;
        }


        private static bool IsGuidDuplicated(GraphAsset graphAsset)
        {
            if (graphAsset.guid.IsEmpty())
            {
                return false;
            }

            string[] guids = AssetDatabase.FindAssets("t:BehaviourTree");

            int count = 0;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GraphAsset findGraphAsset = AssetDatabase.LoadAssetAtPath<GraphAsset>(assetPath);

                if (graphAsset != null && graphAsset.guid == findGraphAsset.guid)
                {
                    count++;

                    if (count > 1) // 자기 자신 포함해서 2개 이상이면 중복
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}