using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace BehaviourSystem.BT
{
    public abstract class GraphCloner
    {
        public abstract EGraphType cloneGraphType
        {
            get;
        }

        /// <summary> 인자로 받은 그래프를 제외한 모든 자식 그래프를 DFS 방식으로 찾아 리스트 반환한다. </summary>
        /// <returns> 인자로 받은 그래프를 제외한 모든 자식 그래프 </returns>
        /// /// <param name="graphAsset"></param>
        public static Dictionary<UGUID, GraphAsset> CollectCurrentAndSubGraphAssets(GraphAsset graphAsset)
        {
            Dictionary<UGUID, GraphAsset> collectedAllCachedGraphs = new Dictionary<UGUID, GraphAsset>();
            List<GraphAsset> graphTraversalStack = ListPool<GraphAsset>.Get();
            graphTraversalStack.Add(graphAsset);

            while (graphTraversalStack.Count > 0)
            {
                int lastIndex = graphTraversalStack.Count - 1;
                GraphAsset asset = graphTraversalStack[lastIndex];
                graphTraversalStack.RemoveAt(lastIndex);

                collectedAllCachedGraphs.Add(asset.guid, asset);

                if (asset.subGraphAssets is null || asset.subGraphAssets.Count == 0)
                {
                    continue;
                }

                for (int i = asset.subGraphAssets.Count - 1; i >= 0; --i)
                {
                    GraphAsset subAsset = asset.subGraphAssets[i];
                    graphTraversalStack.Add(subAsset);
                }
            }
            
            ListPool<GraphAsset>.Release(graphTraversalStack);
            return collectedAllCachedGraphs;
        }


        /// <summary> 그래프의 모든 자식 노드를 초기화한다. </summary>
        /// <param name="graph"></param>
        public abstract void ClearAllNodesOfGraph(GraphAsset graph);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemRunner"></param>
        /// <param name="targetGraph"></param>
        /// <param name="blackboardAsset"></param>
        /// <returns></returns>
        public abstract Graph CloneGraph(BehaviorSystemRunner systemRunner, Graph targetGraph, BlackboardAsset blackboardAsset);
    }
}