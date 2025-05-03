using UnityEditor;
using System;
using BehaviourSystem.BT;
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
                    asset.treeGuid = $"{Guid.NewGuid()}";
                    EditorUtility.SetDirty(asset);
                    
                    if (asset.nodeSet is null)
                    {
                        asset.nodeSet = ScriptableObject.CreateInstance<BehaviourNodeSet>();
                        asset.nodeSet.hideFlags = HideFlags.HideInHierarchy;
                        AssetDatabase.AddObjectToAsset(asset.nodeSet, asset);

                        if (asset.nodeSet.rootNode is null)
                        {
                            asset.nodeSet.rootNode = asset.nodeSet.CreateNode(typeof(RootNode));
                            EditorUtility.SetDirty(asset);
                        }
                        
                        AssetDatabase.SaveAssets();
                    }

                    if (asset.groupDataSet is null)
                    {
                        asset.groupDataSet = ScriptableObject.CreateInstance<GroupDataSet>();
                        asset.groupDataSet.hideFlags = HideFlags.HideInHierarchy;
                        AssetDatabase.AddObjectToAsset(asset.groupDataSet, asset);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }
    }
}