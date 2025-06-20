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
                BehaviourTree asset = AssetDatabase.LoadAssetAtPath<BehaviourTree>(path);

                if (asset != null)
                {
                    bool changed = false;

                    if (asset.treeGuid.IsEmpty() || IsGuidDuplicated(asset))
                    {
                        asset.treeGuid = UGUID.Create();
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



        private static void ChangeNodesOfTreeGuid(BehaviourTree asset)
        {
            if (asset.nodeSet == null || asset.nodeSet.nodeList.Count == 0)
            {
                return;
            }

            foreach (var nodeBase in asset.nodeSet.nodeList)
            {
                UGUID originalGuid = nodeBase.guid;
                nodeBase.guid = UGUID.Create();

                if (asset.groupDataSet.dataList.Count == 0)
                {
                    continue;
                }

                foreach (GroupData groupData in asset.groupDataSet.dataList)
                {
                    if (groupData.containedNodeCount > 0 && groupData.Contains(originalGuid))
                    {
                        groupData.RemoveNodeGuid(originalGuid);
                        groupData.AddNodeGuid(nodeBase.guid);
                    }
                }
            }
        }



        private static bool TryInitializeNodeSetList(BehaviourTree asset)
        {
            if (asset.nodeSet != null)
            {
                return false;
            }

            asset.nodeSet = ScriptableObject.CreateInstance<BehaviourNodeSet>();
            asset.nodeSet.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(asset.nodeSet, asset);

            if (asset.nodeSet.rootNode == null)
            {
                asset.nodeSet.rootNode = asset.nodeSet.CreateNode(typeof(RootNode));
                EditorUtility.SetDirty(asset);
            }

            return true;
        }



        private static bool TryInitializeGroupDataSet(BehaviourTree asset)
        {
            if (asset.groupDataSet != null)
            {
                return false;
            }
            
            asset.groupDataSet = ScriptableObject.CreateInstance<GroupDataSet>();
            asset.groupDataSet.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(asset.groupDataSet, asset);
            return false;
        }


        private static bool IsGuidDuplicated(BehaviourTree currentAsset)
        {
            if (currentAsset.treeGuid.IsEmpty())
            {
                return false;
            }

            string[] guids = AssetDatabase.FindAssets("t:BehaviourTree");

            int count = 0;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                BehaviourTree asset = AssetDatabase.LoadAssetAtPath<BehaviourTree>(assetPath);

                if (asset != null && asset.treeGuid == currentAsset.treeGuid)
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