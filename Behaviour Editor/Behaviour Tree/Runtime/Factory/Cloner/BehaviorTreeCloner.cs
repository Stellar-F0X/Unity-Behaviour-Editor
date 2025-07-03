using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public class BehaviorTreeCloner : GraphCloner
    {
        /// <summary> 클론 작업 정보를 담는 구조체 </summary>
        private struct TreeTraversal
        {
            public TreeTraversal(BehaviourNodeBase origin, BehaviourNodeBase clone, int depth, int stackID)
            {
                this.origin = origin;
                this.clone = clone;
                this.depth = depth;
                this.stackID = stackID;
            }

            public readonly BehaviourNodeBase origin;
            public readonly BehaviourNodeBase clone;
            public readonly int depth;
            public readonly int stackID;
        }


        public override EGraphType cloneGraphType
        {
            get { return EGraphType.BT; }
        }


        public override void ClearAllNodesOfGraph(GraphAsset graph)
        {
            throw new System.NotImplementedException();
        }


        public override Graph CloneGraph(BehaviorSystemRunner systemRunner, Graph targetGraph, BlackboardAsset blackboardAsset)
        {
            BehaviourTree originalBt = targetGraph as BehaviourTree;
            BehaviourTree clonedBt = ScriptableObject.CreateInstance<BehaviourTree>();
            FixedQueue<TreeTraversal> cloneQueue = new FixedQueue<TreeTraversal>(originalBt.nodes.Count);

            clonedBt.entry = Object.Instantiate(originalBt.entry) as RootNode;

            BehaviourNodeBase originalEntry = (BehaviourNodeBase)originalBt.entry;
            BehaviourNodeBase clonedEntry = (BehaviourNodeBase)clonedBt.entry;

            cloneQueue.Enqueue(new TreeTraversal(originalEntry, clonedEntry, 0, 0));

            int callStackSize = 0;

            while (cloneQueue.count > 0)
            {
                TreeTraversal currentClone = cloneQueue.Dequeue();
                this.ProcessClone(currentClone, cloneQueue, clonedBt, ref callStackSize);
            }

            clonedBt.interrupter = new TreeInterruptor(clonedBt, callStackSize);
            return targetGraph;
        }



        /// <summary>  </summary>
        /// <param name="info"></param>
        /// <param name="queue"></param>
        /// <param name="newSet"></param>
        /// <param name="callStack"></param>
        private void ProcessClone(TreeTraversal info, FixedQueue<TreeTraversal> queue, BehaviourTree newSet, ref int callStack)
        {
            info.clone.depth = info.depth;
            info.clone.callStackID = info.stackID;
            info.clone.name = info.clone.name.Remove(info.clone.name.Length - 7); //명시되어있는 (Clone) 접미사 제거. 

            newSet.nodes.Add(info.clone);
            int depthInTree = info.depth + 1;

            switch (info.origin.nodeType)
            {
                case BehaviourNodeBase.EBehaviourNodeType.Root:
                {
                    this.CloneNode((RootNode)info.origin, (RootNode)info.clone, depthInTree, info.stackID, queue);
                    break;
                }

                case BehaviourNodeBase.EBehaviourNodeType.Decorator:
                {
                    this.CloneNode((DecoratorNode)info.origin, (DecoratorNode)info.clone, depthInTree, info.stackID, queue);
                    break;
                }

                case BehaviourNodeBase.EBehaviourNodeType.Composite:
                {
                    this.CloneNode((CompositeNode)info.origin, (CompositeNode)info.clone, depthInTree, info.stackID, ref callStack, queue);
                    break;
                }
            }
        }


        /// <summary> 루트 노드의 자식 클론 처리 </summary>
        private void CloneNode(RootNode origin, RootNode clone, int nextDepth, int stackID, FixedQueue<TreeTraversal> cloneQueue)
        {
            if (origin.child is not null)
            {
                BehaviourNodeBase childClone = Object.Instantiate(origin.child);
                childClone.parent = clone;
                clone.child = childClone;
                cloneQueue.Enqueue(new TreeTraversal(origin.child, childClone, nextDepth, stackID));
            }
        }


        /// <summary> 데코레이터 노드의 자식을 클론 처리 </summary>
        private void CloneNode(DecoratorNode origin, DecoratorNode clone, int nextDepth, int stackID, FixedQueue<TreeTraversal> cloneQueue)
        {
            if (origin.child is not null)
            {
                BehaviourNodeBase childClone = Object.Instantiate(origin.child);
                childClone.parent = clone;
                clone.child = childClone;
                cloneQueue.Enqueue(new TreeTraversal(origin.child, childClone, nextDepth, stackID));
            }
        }


        /// <summary> 컴포지트 노드의 자식들을 클론 처리. Parallel 노드의 경우 새로운 CallStack ID 할당하여 CallStack 공간 배정 </summary>
        private void CloneNode(CompositeNode origin,
                               CompositeNode clone,
                               int nextDepth,
                               int stackID,
                               ref int callStackSize,
                               FixedQueue<TreeTraversal> cloneQueue)
        {
            if (origin.children is not null && origin.children.Count > 0)
            {
                bool isParallelNode = origin is ParallelNode;

                for (int i = 0; i < origin.children.Count; ++i)
                {
                    int newStackID = isParallelNode ? (++callStackSize) : stackID;
                    BehaviourNodeBase childClone = Object.Instantiate(origin.children[i]);
                    childClone.parent = clone;
                    clone.children[i] = childClone;
                    cloneQueue.Enqueue(new TreeTraversal(origin.children[i], childClone, nextDepth, newStackID));
                }
            }
        }
    }
}