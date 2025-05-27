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
            Type tConditionList = typeof(IList<BlackboardBasedCondition>);

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
        /// <param name="infos">클론 작업 큐</param>
        /// <param name="nodes">후처리 스택</param>
        /// <param name="runner">트리 러너</param>
        /// <param name="newSet">클론된 노드셋</param>s
        private void ProcessClone(CloneInfo info, FixedQueue<CloneInfo> infos, FixedStack<NodeBase> nodes, BehaviourTreeRunner runner, BehaviourNodeSet newSet)
        {
            info.clone.runner = runner;
            info.clone.depth = info.depth;
            info.clone.callStackID = info.stackID;
            info.clone.name = info.clone.name.Remove(info.clone.name.Length - 7); //명시되어있는 (Clone) 접미사 제거. 

            newSet.nodeList.Add(info.clone);
            nodes.Push(info.clone);

            switch (info.origin.nodeType)
            {
                case NodeBase.ENodeType.Root: 
                    this.ProcessRootNodeClone((RootNode)info.origin, (RootNode)info.clone, info.depth + 1, info.stackID, infos); 
                    break;

                case NodeBase.ENodeType.Decorator:
                    this.ProcessDecoratorNodeClone((DecoratorNode)info.origin, (DecoratorNode)info.clone, info.depth + 1, info.stackID, infos); 
                    break;

                case NodeBase.ENodeType.Composite:
                    this.ProcessChildrenNodeClone((CompositeNode)info.origin, (CompositeNode)info.clone, info.depth + 1, info.stackID, infos); 
                    break;
            }
        }


        /// <summary> 루트 노드 클론 처리 </summary>
        private void ProcessRootNodeClone(RootNode origin, RootNode clone, int nextDepth, int stackID, FixedQueue<CloneInfo> cloneQueue)
        {
            if (origin.child is not null)
            {
                NodeBase childClone = UObject.Instantiate(origin.child);
                childClone.parent = clone;
                clone.child = childClone;
                cloneQueue.Enqueue(new CloneInfo(origin.child, childClone, nextDepth, stackID));
            }
        }


        /// <summary> 데코레이터 노드 클론 처리 </summary>
        private void ProcessDecoratorNodeClone(DecoratorNode origin, DecoratorNode clone, int nextDepth, int stackID, FixedQueue<CloneInfo> cloneQueue)
        {
            if (origin.child is not null)
            {
                NodeBase childClone = UObject.Instantiate(origin.child);
                childClone.parent = clone;
                clone.child = childClone;
                cloneQueue.Enqueue(new CloneInfo(origin.child, childClone, nextDepth, stackID));
            }
        }


        /// <summary> 컴포지트 노드 클론 처리 - 병렬 노드의 경우 새로운 CallStack ID 할당 </summary>
        private void ProcessChildrenNodeClone(CompositeNode origin, CompositeNode clone, int nextDepth, int stackID, FixedQueue<CloneInfo> cloneQueue)
        {
            if (origin.children is not null && origin.children.Count > 0)
            {
                bool isParallelNode = origin is ParallelNode;

                for (int i = 0; i < origin.children.Count; ++i)
                {
                    int newStackID = isParallelNode ? (++callStackSize) : stackID;
                    NodeBase childClone = UObject.Instantiate(origin.children[i]);
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

                    if (accessor.getter(clonedNode) is List<BlackboardBasedCondition> conditionList)
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


        private void UpdateConditionProperties(BlackboardBasedCondition condition, Blackboard board)
        {
            if (condition is not null)
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
            if (CreateInstance(nodeType) is NodeBase node)
            {
                node.name = Regex.Replace(nodeType.Name.Replace("Node", ""), "(?<!^)([A-Z])", " $1");
                node.guid = GUID.Generate().ToString();
                node.hideFlags = HideFlags.HideInHierarchy;

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

            throw new Exception("Node is null");
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