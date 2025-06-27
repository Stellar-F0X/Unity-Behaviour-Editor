using System;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    public class BehaviourTreePostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
                GraphAsset asset = AssetDatabase.LoadAssetAtPath<GraphAsset>(path);

                if (asset != null)
                {
                    bool changed = false;

                    if (asset.guid.IsEmpty() || IsGuidDuplicated(asset))
                    {
                        asset.guid = UGUID.Create();
                        ChangeNodesOfTreeGuid(asset);
                        EditorUtility.SetDirty(asset);
                        changed = true;
                    }

                    changed |= TryInitializeNodeSetList(asset);
                    changed |= TryInitializeGroupDataSet(asset);

                    if (changed)
                    {
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }



        private static void ChangeNodesOfTreeGuid(GraphAsset asset)
        {
            if (asset.graph == null || asset.graph.nodes.Count == 0)
            {
                return;
            }

            foreach (var nodeBase in asset.graph.nodes)
            {
                UGUID originalGuid = nodeBase.guid;
                nodeBase.guid = UGUID.Create();

                if (asset.graphGroup.dataList.Count == 0)
                {
                    continue;
                }

                foreach (GroupData groupData in asset.graphGroup.dataList)
                {
                    if (groupData.containedNodeCount > 0 && groupData.Contains(originalGuid))
                    {
                        groupData.RemoveNodeGuid(originalGuid);
                        groupData.AddNodeGuid(nodeBase.guid);
                    }
                }
            }
        }



        private static bool TryInitializeNodeSetList(GraphAsset asset)
        {
            if (asset.graph != null)
            {
                return false;
            }

            asset.graph = ScriptableObject.CreateInstance<BehaviourTree>();
            asset.graph.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(asset.graph, asset);

            if (asset.graph.entry == null)
            {
                asset.graph.entry = asset.graph.CreateNode(typeof(RootNode));
                EditorUtility.SetDirty(asset);
            }

            return true;
        }



        private static bool TryInitializeGroupDataSet(GraphAsset asset)
        {
            if (asset.graphGroup != null)
            {
                return false;
            }
            
            asset.graphGroup = ScriptableObject.CreateInstance<GraphGroup>();
            asset.graphGroup.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(asset.graphGroup, asset);
            return false;
        }


        private static bool IsGuidDuplicated(GraphAsset currentAsset)
        {
            if (currentAsset.guid.IsEmpty())
            {
                return false;
            }

            string[] guids = AssetDatabase.FindAssets("t:BehaviourTree");

            int count = 0;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GraphAsset asset = AssetDatabase.LoadAssetAtPath<GraphAsset>(assetPath);

                if (asset != null && asset.guid == currentAsset.guid)
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