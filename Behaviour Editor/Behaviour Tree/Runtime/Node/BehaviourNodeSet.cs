using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UObject = UnityEngine.Object;

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


        public int callStackSize
        {
            get;
            private set;
        }


        internal BehaviourNodeSet Clone(BehaviourTreeRunner treeRunner, Blackboard blackboard)
        {
            BehaviourNodeSet clonedSet = CreateInstance<BehaviourNodeSet>();
            clonedSet.rootNode = Instantiate(this.rootNode) as RootNode;

            FixedQueue<CloneInfo> cloneQueue = new FixedQueue<CloneInfo>(this.nodeList.Count);
            FixedStack<NodeBase> postInitStack = new FixedStack<NodeBase>(this.nodeList.Count);

            Type tProp = typeof(IBlackboardProperty);
            Type tCondition = typeof(BlackboardBasedCondition);
            Type tConditionList = typeof(ICollection<BlackboardBasedCondition>);

            cloneQueue.Enqueue(new CloneInfo(this.rootNode, clonedSet.rootNode, 0, 0));

            while (cloneQueue.count > 0)
            {
                CloneInfo currentClone = cloneQueue.Dequeue();
                this.ProcessClone(currentClone, cloneQueue, postInitStack, treeRunner, clonedSet);
                this.ProcessBlackboardProperties(currentClone.clone, blackboard, tProp, tCondition, tConditionList);
            }

            clonedSet.callStackSize = this.callStackSize;

            // PostTreeCreation을 위한 후처리
            while (postInitStack.count > 0)
            {
                NodeBase currentNode = postInitStack.Pop();
                currentNode.PostTreeCreation();
            }

            return clonedSet;
        }



#region Clone Methods

        /// <summary>
        /// 단일 클론 브랜치를 처리
        /// JobSystem으로 병렬화할 수 있는 영역
        /// </summary>
        /// <param name="info">클론 정보</param>
        /// <param name="queue">클론 작업 큐</param>
        /// <param name="nodes">후처리 스택</param>
        /// <param name="runner">트리 러너</param>
        /// <param name="newSet">클론된 노드셋</param>s
        private void ProcessClone(CloneInfo info, FixedQueue<CloneInfo> queue, FixedStack<NodeBase> nodes, BehaviourTreeRunner runner, BehaviourNodeSet newSet)
        {
            info.clone.runner = runner;
            info.clone.depth = info.depth;
            info.clone.callStackID = info.stackID;
            info.clone.name = info.clone.name.Remove(info.clone.name.Length - 7); //명시되어있는 (Clone) 접미사 제거. 

            newSet.nodeList.Add(info.clone);
            nodes.Push(info.clone);
            
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


        /// <summary> 블랙보드 프로퍼티 처리 </summary>
        private void ProcessBlackboardProperties(NodeBase clonedNode, Blackboard blackboard, Type tProperty, Type tCondition, Type tConditionList)
        {
            foreach (var fieldInfo in ReflectionHelper.GetCachedFieldInfo(clonedNode?.GetType()))
            {
                if (tProperty.IsAssignableFrom(fieldInfo.FieldType))
                {
                    ReflectionHelper.FieldAccessor accessor = ReflectionHelper.GetAccessor(fieldInfo);

                    if (accessor.getter(clonedNode) is IBlackboardProperty property)
                    {
                        IBlackboardProperty foundProperty = blackboard.FindProperty(property.key);

                        if (foundProperty != null)
                        {
                            accessor.setter(clonedNode, foundProperty);
                        }
                    }
                }
                else if (tConditionList.IsAssignableFrom(fieldInfo.FieldType))
                {
                    ReflectionHelper.FieldAccessor accessor = ReflectionHelper.GetAccessor(fieldInfo);

                    if (accessor.getter(clonedNode) is ICollection<BlackboardBasedCondition> conditionList)
                    {
                        foreach (var condition in conditionList)
                        {
                            this.UpdateConditionProperties(condition, blackboard);
                        }
                    }
                }
                else if (tCondition.IsAssignableFrom(fieldInfo.FieldType))
                {
                    ReflectionHelper.FieldAccessor accessor = ReflectionHelper.GetAccessor(fieldInfo);

                    if (accessor.getter(clonedNode) is BlackboardBasedCondition condition)
                    {
                        this.UpdateConditionProperties(condition, blackboard);
                    }
                }
            }
        }


        /// <summary> 블랙보드 기반 프로퍼티 컨디션 할당처리 </summary>
        private void UpdateConditionProperties(BlackboardBasedCondition condition, Blackboard board)
        {
            if (condition.property is not null)
            {
                IBlackboardProperty foundProperty = board.FindProperty(condition.property.key);

                if (foundProperty is not null)
                {
                    condition.property = foundProperty;
                }
            }

            if (condition.comparableValue is not null)
            {
                IBlackboardProperty foundComparableProperty = board.FindProperty(condition.comparableValue.key);

                if (foundComparableProperty is not null)
                {
                    condition.comparableValue = foundComparableProperty;
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
                case NodeBase.ENodeType.Root: ((RootNode)parent).child = null; break;

                case NodeBase.ENodeType.Decorator: ((DecoratorNode)parent).child = null; break;

                case NodeBase.ENodeType.Composite: ((CompositeNode)parent).children.Remove(child); break;
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