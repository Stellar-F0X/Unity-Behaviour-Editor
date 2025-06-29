using System;
using BehaviourSystem.BT;
using BehaviourSystem.BT.State;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    public class GraphAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
                GraphAsset graphAsset = AssetDatabase.LoadAssetAtPath<GraphAsset>(path);

                if (graphAsset != null)
                {
                    bool flag1 = GraphFactory.ValidateOrRenewGuid(graphAsset);
                    bool flag2 = GraphAssetPostprocessor.TrySetupGraph(graphAsset);
                    bool flag3 = GraphAssetPostprocessor.TrySetupGroup(graphAsset);

                    if (flag1 || flag2 || flag3)
                    {
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }
        

        private static bool TrySetupGraph(GraphAsset graphAsset)
        {
            if (graphAsset.graph != null)
            {
                return false;
            }

            GraphFactory.CreateGraphAndAddToAsset(graphAsset);
            
            graphAsset.rootGraphAsset = graphAsset;
            graphAsset.isRootGraph = true;
            
            EditorUtility.SetDirty(graphAsset);
            Debug.Assert(graphAsset.graph is not null, $"{nameof(GraphAsset)}: Graph is null");
            return true;
        }


        private static bool TrySetupGroup(GraphAsset graphAsset)
        {
            if (graphAsset.graphGroup != null)
            {
                return false;
            }

            GraphFactory.CreateGraphGroup(graphAsset);
            EditorUtility.SetDirty(graphAsset);
            
            Debug.Assert(graphAsset.graphGroup is not null, $"{nameof(GraphAsset)}: GraphGroup is null");
            return true;
        }
    }
}