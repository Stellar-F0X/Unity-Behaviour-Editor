using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BehaviourSystem.BT
{
    public class BehaviourNodeSet : ScriptableObject
    {
        /// <summary> 클론 작업 정보를 담는 구조체 </summary>
        private struct CloneInfo
        {
            public CloneInfo(NodeBase origin, NodeBase clone, int depth, int stackID)
            {
                this.origin = origin;
                this.clone = clone;
                this.depth = depth;
                this.stackID = stackID;
            }

            public readonly NodeBase origin;
            public readonly NodeBase clone;
            public readonly int depth;
            public readonly int stackID;
        }


        [HideInInspector]
        public NodeBase rootNode;

        [HideInInspector]
        public List<NodeBase> nodeList = new List<NodeBase>();
        
        [field: NonSerialized]
        public int callStackSize
        {
            get;
            private set;
        }


        internal BehaviourNodeSet Clone(Blackboard clonedBlackboard)
        {
            BehaviourNodeSet clonedSet = CreateInstance<BehaviourNodeSet>();
            FixedQueue<CloneInfo> cloneQueue = new FixedQueue<CloneInfo>(this.nodeList.Count);
            
            clonedSet.rootNode = Instantiate(this.rootNode) as RootNode;
            cloneQueue.Enqueue(new CloneInfo(this.rootNode, clonedSet.rootNode, 0, 0));

            while (cloneQueue.count > 0)
            {
                CloneInfo currentClone = cloneQueue.Dequeue();
                this.ProcessClone(currentClone, cloneQueue, clonedSet);
                NodePropertyFieldBinder.BindNodeProperties(currentClone.clone, clonedBlackboard);
            }

            clonedSet.callStackSize = this.callStackSize;
            return clonedSet;
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
        private void ProcessClone(CloneInfo info, FixedQueue<CloneInfo> queue, BehaviourNodeSet newSet)
        {
            info.clone.depth = info.depth;
            info.clone.callStackID = info.stackID;
            info.clone.name = info.clone.name.Remove(info.clone.name.Length - 7); //명시되어있는 (Clone) 접미사 제거. 

            newSet.nodeList.Add(info.clone);
            
            int depthInTree = info.depth + 1;

            switch (info.origin.nodeType)
            {
                case NodeBase.ENodeType.Root: this.CloneNode((RootNode)info.origin, (RootNode)info.clone, depthInTree, info.stackID, queue); break;

                case NodeBase.ENodeType.Decorator: this.CloneNode((DecoratorNode)info.origin, (DecoratorNode)info.clone, depthInTree, info.stackID, queue); break;

                case NodeBase.ENodeType.Composite: this.CloneNode((CompositeNode)info.origin, (CompositeNode)info.clone, depthInTree, info.stackID, queue); break;
            }
        }


        /// <summary> 루트 노드의 자식 클론 처리 </summary>
        private void CloneNode(RootNode origin, RootNode clone, int nextDepth, int stackID, FixedQueue<CloneInfo> cloneQueue)
        {
            if (origin.child is not null)
            {
                NodeBase childClone = Instantiate(origin.child);
                childClone.parent = clone;
                clone.child = childClone;
                cloneQueue.Enqueue(new CloneInfo(origin.child, childClone, nextDepth, stackID));
            }
        }


        /// <summary> 데코레이터 노드의 자식을 클론 처리 </summary>
        private void CloneNode(DecoratorNode origin, DecoratorNode clone, int nextDepth, int stackID, FixedQueue<CloneInfo> cloneQueue)
        {
            if (origin.child is not null)
            {
                NodeBase childClone = Instantiate(origin.child);
                childClone.parent = clone;
                clone.child = childClone;
                cloneQueue.Enqueue(new CloneInfo(origin.child, childClone, nextDepth, stackID));
            }
        }


        /// <summary> 컴포지트 노드의 자식들을 클론 처리. Parallel 노드의 경우 새로운 CallStack ID 할당하여 CallStack 공간 배정 </summary>
        private void CloneNode(CompositeNode origin, CompositeNode clone, int nextDepth, int stackID, FixedQueue<CloneInfo> cloneQueue)
        {
            if (origin.children is not null && origin.children.Count > 0)
            {
                bool isParallelNode = origin is ParallelNode;

                for (int i = 0; i < origin.children.Count; ++i)
                {
                    int newStackID = isParallelNode ? (++callStackSize) : stackID;
                    NodeBase childClone = Instantiate(origin.children[i]);
                    childClone.parent = clone;
                    clone.children[i] = childClone;
                    cloneQueue.Enqueue(new CloneInfo(origin.children[i], childClone, nextDepth, newStackID));
                }
            }
        }

#endregion



#if UNITY_EDITOR

        public void AddChild(NodeBase parent, NodeBase child)
        {
            Undo.RecordObject(parent, "Behaviour Tree (AddChild)");

            switch (parent.nodeType)
            {
                case NodeBase.ENodeType.Root:
                    ((RootNode)parent).child = child;
                    child.parent = parent;
                    break;

                case NodeBase.ENodeType.Decorator:
                    ((DecoratorNode)parent).child = child;
                    child.parent = parent;
                    break;

                case NodeBase.ENodeType.Composite:
                    ((CompositeNode)parent).children.Add(child);
                    child.parent = parent;
                    break;
            }

            EditorUtility.SetDirty(parent);
            EditorUtility.SetDirty(child);
        }


        public void RemoveChild(NodeBase parent, NodeBase child)
        {
            Undo.RecordObject(parent, "Behaviour Tree (RemoveChild)");

            switch (parent.nodeType)
            {
                case NodeBase.ENodeType.Root: 
                    ((RootNode)parent).child = null; 
                    child.parent = null;
                    break;

                case NodeBase.ENodeType.Decorator: 
                    ((DecoratorNode)parent).child = null;
                    child.parent = null;
                    break;

                case NodeBase.ENodeType.Composite:
                    ((CompositeNode)parent).children.Remove(child);
                    child.parent = null;
                    break;
            }

            EditorUtility.SetDirty(child);
        }


        public NodeBase CreateNode(Type nodeType)
        {
            NodeBase node = NodeFactory.CreateNode(nodeType);

            if (node is null)
            {
                throw new Exception("Node is null");
            }

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(this, "Behaviour Tree (CreateNode)");
            }

            nodeList.Add(node);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");
                AssetDatabase.AddObjectToAsset(node, this);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }

            return node;
        }



        public void DeleteNode(NodeBase node)
        {
            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(this, "Behaviour Tree (DeleteNode)");
            }

            nodeList.Remove(node);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.DestroyObjectImmediate(node);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }
#endif
    }
}