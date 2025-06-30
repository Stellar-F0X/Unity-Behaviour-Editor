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
        public static GraphAsset[] CollectCurrentAndSubGraphAssets(GraphAsset graphAsset)
        {
            List<GraphAsset> collectedAllCachedGraphs = ListPool<GraphAsset>.Get();
            List<GraphAsset> graphTraversalStack = ListPool<GraphAsset>.Get();
            graphTraversalStack.Add(graphAsset);

            while (graphTraversalStack.Count > 0)
            {
                int lastIndex = graphTraversalStack.Count - 1;
                GraphAsset asset = graphTraversalStack[lastIndex];
                graphTraversalStack.RemoveAt(lastIndex);

                collectedAllCachedGraphs.Add(asset);

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

            GraphAsset[] result = collectedAllCachedGraphs.ToArray();
            ListPool<GraphAsset>.Release(collectedAllCachedGraphs);
            ListPool<GraphAsset>.Release(graphTraversalStack);
            return result;
        }


        public static void BindNodeProperties(ReflectionHelper.FieldAccessor accessor, NodeBase node, Blackboard blackboard, GraphAsset[] clonedGraphAssets)
        {
            switch (accessor.getter(node))
            {
                case IBlackboardProperty property:
                {
                    IBlackboardProperty foundProperty = blackboard.FindProperty(property.hashCode);
                    Debug.Assert(foundProperty != null, "Blackboard property not found.");
                    accessor.setter(node, foundProperty);
                    return;
                }

                case GraphAsset graphAsset:
                {
                    GraphAsset foundAsset = clonedGraphAssets.FirstOrDefault(asset => asset.guid == graphAsset.guid);
                    Debug.Assert(foundAsset != null, "GraphAsset not found.");
                    accessor.setter(node, foundAsset);
                    return;
                }

                case IList<BlackboardBasedCondition> conditions:
                {
                    foreach (var condition in conditions)
                    {
                        if (string.IsNullOrEmpty(condition.property?.key))
                        {
                            IBlackboardProperty foundProperty = blackboard.FindProperty(condition.property.hashCode);
                            Debug.Assert(foundProperty != null, "BlackboardBasedCondition not found.");
                            condition.property = foundProperty;
                        }

                        if (string.IsNullOrEmpty(condition.comparableValue?.key))
                        {
                            IBlackboardProperty foundProperty = blackboard.FindProperty(condition.comparableValue.hashCode);
                            Debug.Assert(foundProperty != null, "Comparable blackboardBasedCondition not found.");
                            condition.comparableValue = foundProperty;
                        }
                    }

                    return;
                }

                case IList<Transition> transitions:
                {
                    foreach (var transition in transitions)
                    {
                        foreach (var condition in transition.conditions)
                        {
                            if (string.IsNullOrEmpty(condition.property?.key))
                            {
                                IBlackboardProperty foundProperty = blackboard.FindProperty(condition.property.hashCode);
                                Debug.Assert(foundProperty != null, "BlackboardBasedCondition not found.");
                                condition.property = foundProperty;
                            }

                            if (string.IsNullOrEmpty(condition.comparableValue?.key))
                            {
                                IBlackboardProperty foundProperty = blackboard.FindProperty(condition.comparableValue.hashCode);
                                Debug.Assert(foundProperty != null, "Comparable blackboardBasedCondition not found.");
                                condition.comparableValue = foundProperty;
                            }
                        }
                    }

                    return;
                }
            }
        }


        /// <summary> 그래프의 모든 자식 노드를 초기화한다. </summary>
        /// <param name="graph"></param>
        public abstract void ClearAllNodesOfGraph(GraphAsset graph);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemRunner"></param>
        /// <param name="targetGraph"></param>
        /// <param name="blackboard"></param>
        /// <returns></returns>
        public abstract Graph CloneGraph(BehaviorSystemRunner systemRunner, Graph targetGraph, Blackboard blackboard);
    }
}