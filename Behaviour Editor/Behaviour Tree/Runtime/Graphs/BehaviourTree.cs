using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BehaviourSystem.BT
{
    public class BehaviourTree : Graph
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

        public TreeInterruptor interrupter
        {
            get;
            private set;
        }


        public override Graph CloneGraph(Blackboard clonedBlackboard)
        {
            BehaviourTree clonedSet = CreateInstance<BehaviourTree>();
            FixedQueue<TreeTraversal> cloneQueue = new FixedQueue<TreeTraversal>(this.nodes.Count);

            clonedSet.entry = Instantiate(this.entry) as RootNode;

            BehaviourNodeBase originalEntry = (BehaviourNodeBase)this.entry;
            BehaviourNodeBase clonedEntry = (BehaviourNodeBase)clonedSet.entry;

            cloneQueue.Enqueue(new TreeTraversal(originalEntry, clonedEntry, 0, 0));

            int callStackSize = 0;

            while (cloneQueue.count > 0)
            {
                TreeTraversal currentClone = cloneQueue.Dequeue();
                this.ProcessClone(currentClone, cloneQueue, clonedSet, ref callStackSize);
                NodePropertyFieldBinder.BindNodeProperties(currentClone.clone, clonedBlackboard);
            }

            interrupter = new TreeInterruptor(this, callStackSize);
            return clonedSet;
        }


        public override EStatus UpdateGraph()
        {
            if (entry is BehaviourNodeBase behaviourNode)
            {
                return behaviourNode.UpdateNode();
            }
            else
            {
                return EStatus.Failure;
            }
        }


        public override void ResetGraph()
        {
            
        }


        public override void StopGraph()
        {
            interrupter.AbortSubtree(entry.callStackID);
        }


#region Clone Methods
        /// <summary>
        /// 단일 클론 브랜치를 처리
        /// JobSystem으로 병렬화할 수 있는 영역
        /// </summary>
        /// <param name="info">클론 정보</param>
        /// <param name="queue">클론 작업 큐</param>
        /// <param name="stack">후처리 스택</param>
        /// <param name="runner">트리 러너</param>
        /// <param name="newSet">클론된 노드셋</param>s
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
                BehaviourNodeBase childClone = Instantiate(origin.child);
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
                BehaviourNodeBase childClone = Instantiate(origin.child);
                childClone.parent = clone;
                clone.child = childClone;
                cloneQueue.Enqueue(new TreeTraversal(origin.child, childClone, nextDepth, stackID));
            }
        }


        /// <summary> 컴포지트 노드의 자식들을 클론 처리. Parallel 노드의 경우 새로운 CallStack ID 할당하여 CallStack 공간 배정 </summary>
        private void CloneNode(CompositeNode origin, CompositeNode clone, int nextDepth, int stackID, ref int callStackSize, FixedQueue<TreeTraversal> cloneQueue)
        {
            if (origin.children is not null && origin.children.Count > 0)
            {
                bool isParallelNode = origin is ParallelNode;

                for (int i = 0; i < origin.children.Count; ++i)
                {
                    int newStackID = isParallelNode ? (++callStackSize) : stackID;
                    BehaviourNodeBase childClone = Instantiate(origin.children[i]);
                    childClone.parent = clone;
                    clone.children[i] = childClone;
                    cloneQueue.Enqueue(new TreeTraversal(origin.children[i], childClone, nextDepth, newStackID));
                }
            }
        }

#endregion



#if UNITY_EDITOR
        public void AddChild(BehaviourNodeBase parent, BehaviourNodeBase child)
        {
            Undo.RecordObject(parent, "Behaviour Tree (AddChild)");

            switch (parent.nodeType)
            {
                case BehaviourNodeBase.EBehaviourNodeType.Root:
                    ((RootNode)parent).child = child;
                    child.parent = parent;
                    break;

                case BehaviourNodeBase.EBehaviourNodeType.Decorator:
                    ((DecoratorNode)parent).child = child;
                    child.parent = parent;
                    break;

                case BehaviourNodeBase.EBehaviourNodeType.Composite:
                    ((CompositeNode)parent).children.Add(child);
                    child.parent = parent;
                    break;
            }

            EditorUtility.SetDirty(parent);
            EditorUtility.SetDirty(child);
        }


        public void RemoveChild(BehaviourNodeBase parent, BehaviourNodeBase child)
        {
            Undo.RecordObject(parent, "Behaviour Tree (RemoveChild)");

            switch (parent.nodeType)
            {
                case BehaviourNodeBase.EBehaviourNodeType.Root:
                    ((RootNode)parent).child = null;
                    child.parent = null;
                    break;

                case BehaviourNodeBase.EBehaviourNodeType.Decorator:
                    ((DecoratorNode)parent).child = null;
                    child.parent = null;
                    break;

                case BehaviourNodeBase.EBehaviourNodeType.Composite:
                    ((CompositeNode)parent).children.Remove(child);
                    child.parent = null;
                    break;
            }

            EditorUtility.SetDirty(child);
        }
#endif
    }
}