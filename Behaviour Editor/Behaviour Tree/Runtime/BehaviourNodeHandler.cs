using System;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace BehaviourSystem.BT
{
    internal class BehaviourNodeHandler
    {
        public BehaviourNodeHandler(BehaviourNodeSet nodeSet)
        {
            int count = nodeSet.callStackSize + 1;
            int excludingRootCount = nodeSet.nodeList.Count - 1;

            this._nodeSet = nodeSet;
            this._runtimeCallStack = new FixedList<Stack<NodeBase>>(count);
            this._abortQueue = new FixedQueue<AbortInfo>(excludingRootCount); //count excluding root node

            for (int i = 0; i < count; ++i)
            {
                _runtimeCallStack.Add(new Stack<NodeBase>());
            }
        }

        /// <summary> 중단(Abort) 작업 정보를 담는 구조체 </summary>
        private struct AbortInfo
        {
            public AbortInfo(int callStackID, NodeBase targetNode = null)
            {
                this.callStackID = callStackID;
                this.targetNode = targetNode;
            }

            public readonly int callStackID;
            public readonly NodeBase targetNode; //null이면 전체 스택 중단
        }


        private FixedList<Stack<NodeBase>> _runtimeCallStack;

        private FixedQueue<AbortInfo> _abortQueue;

        private BehaviourNodeSet _nodeSet;


        /// <summary> 트리에서 지정된 태그가 수식된 노드들을 찾습니다. </summary>
        /// <param name="nodeTag">수식된 태그</param>
        /// <param name="nodeSet">트리 집합</param>
        /// <param name="accessors">찾은 노드들</param>
        /// <returns>노드 탐색 성공 여부</returns>
        public NodeAccessor[] GetNodeByTag(string nodeTag)
        {
            Span<int> indexArray = stackalloc int[_nodeSet.nodeList.Count];
            int count = 0;

            for (int i = 0; i < _nodeSet.nodeList.Count; ++i)
            {
                NodeBase currentNode = _nodeSet.nodeList[i];

                if (currentNode.tag.CompareTo(nodeTag) == 0)
                {
                    indexArray[count] = i;
                    count++;
                }
            }

            if (count > 0)
            {
                NodeAccessor[] accessors = new NodeAccessor[count];

                //만약 Tag가 모든 노드를 대상으로 한다면 시간 복잡도는 O(2n)이 되므로 GC 측면에선 좋으나, 빠른 검색의 관점에선 잘 모르겠다. 
                for (int i = 0; i < count; ++i)
                {
                    NodeBase targetNode = _nodeSet.nodeList[indexArray[i]];
                    accessors[i] = new NodeAccessor(targetNode);
                }

                return accessors;
            }

            return null;
        }


        /// <summary> 트리 디렉토리를 토대로 경로상에 위치한 노드를 찾습니다. </summary>
        /// <param name="treePath">트리 디렉토리</param>
        /// <param name="nodeSet">트리 집합</param>
        /// <param name="node">찾은 노드</param>
        /// <returns>노드 탐색 성공 여부</returns>
        public bool TryGetNodeByPath(string treePath, out NodeAccessor node)
        {
            if (string.IsNullOrEmpty(treePath) || string.IsNullOrWhiteSpace(treePath))
            {
                node = default;
                return false;
            }

            Span<char> pathBuffer = stackalloc char[256];
            Span<int> pathStartIndices = stackalloc int[128];
            Span<int> pathLengths = stackalloc int[128];

            int pathCount = 0;
            int currentStart = 0;
            int pathLength = Math.Min(treePath.Length, 256);

            treePath.AsSpan(0, pathLength).CopyTo(pathBuffer);

            for (int i = 0; i < pathLength; i++)
            {
                if (pathBuffer[i] == '/')
                {
                    if (i > currentStart) // 빈 경로 세그먼트 방지
                    {
                        pathStartIndices[pathCount] = currentStart;
                        pathLengths[pathCount] = i - currentStart;
                        pathCount++;
                    }

                    currentStart = i + 1;
                }
            }

            if (currentStart < pathLength)
            {
                pathStartIndices[pathCount] = currentStart;
                pathLengths[pathCount] = pathLength - currentStart;
                pathCount++;
            }

            if (pathCount == 0)
            {
                node = default;
                return false;
            }

            ReadOnlySpan<char> rootNamePath = pathBuffer.Slice(pathStartIndices[0], pathLengths[0]);

            if (rootNamePath.Equals(_nodeSet.rootNode.name.AsSpan(), StringComparison.Ordinal) == false)
            {
                node = default;
                return false;
            }

            NodeBase nodeBase = _nodeSet.rootNode;

            for (int i = 1; i < pathCount; i++)
            {
                bool find = false;
                ReadOnlySpan<char> currentPath = pathBuffer.Slice(pathStartIndices[i], pathLengths[i]);

                if (nodeBase is IBehaviourIterable iterable)
                {
                    List<NodeBase> children = iterable.GetChildren();
                    int count = children.Count;

                    for (int j = 0; j < count; ++j)
                    {
                        if (currentPath.Equals(children[j].name.AsSpan(), StringComparison.Ordinal))
                        {
                            nodeBase = children[j];
                            find = true;
                            break;
                        }
                    }

                    if (find == false)
                    {
                        node = default;
                        return false;
                    }
                }
            }

            node = new NodeAccessor(nodeBase);
            return true;
        }


        /// <summary> 지정된 호출 스택의 현재 실행 중인 노드를 반환 </summary>
        /// <param name="callStackID">호출 스택 ID</param>
        /// <returns>현재 노드, 없으면 null</returns>
        public NodeBase GetCurrentNode(in int callStackID)
        {
            if (this.IsValidCallStack(callStackID) == false || _runtimeCallStack[callStackID].Count == 0)
            {
                return null;
            }

            return _runtimeCallStack[callStackID].Peek();
        }


        /// <summary> 지정된 호출 스택에 노드를 푸시 </summary>
        /// <param name="callStackID">호출 스택 ID</param>
        /// <param name="node">푸시할 노드</param>
        public void PushInCallStack(in int callStackID, NodeBase node)
        {
            _runtimeCallStack[callStackID].Push(node);
        }


        /// <summary> 지정된 호출 스택에서 최상단 노드를 팝 </summary>
        /// <param name="callStackID">호출 스택 ID</param>
        public void PopInCallStack(in int callStackID)
        {
            if (this.IsValidCallStack(callStackID) == false || _runtimeCallStack[callStackID].Count == 0)
            {
                Debug.LogWarning($"호출 스택 ID {callStackID}에서 꺼낼 노드가 없습니다.");
                return;
            }

            _runtimeCallStack[callStackID].Pop();
        }


        /// <summary>
        /// 지정된 노드부터 상위까지의 서브트리를 중단
        /// 해당 노드보다 깊은 depth의 노드들을 모두 정리
        /// </summary>
        /// <param name="callStackID">호출 스택 ID</param>
        /// <param name="node">중단할 기준 노드</param>
        public void AbortSubtreeFrom(in int callStackID, NodeBase node)
        {
            _abortQueue.Clear();
            _abortQueue.Enqueue(new AbortInfo(callStackID, node));

            this.ProcessAbortQueue(true);
        }


        /// <summary> 지정된 호출 스택의 전체 서브트리를 중단 </summary>
        /// <param name="callStackID">중단할 호출 스택 ID</param>
        public void AbortSubtree(in int callStackID)
        {
            _abortQueue.Clear();
            _abortQueue.Enqueue(new AbortInfo(callStackID));

            this.ProcessAbortQueue(false);
        }


        /// <summary>
        /// 중단 큐를 처리하여 노드들을 정리
        /// JobSystem으로 병렬화 가능하지만 상태 변경이 있어 주의 필요
        /// </summary>
        /// <param name="abortQueue">중단할 노드들의 큐</param>
        /// <param name="hasTargetNode">특정 노드까지만 중단할지 여부</param>
        private void ProcessAbortQueue(bool hasTargetNode)
        {
            while (_abortQueue.count > 0)
            {
                AbortInfo current = _abortQueue.Dequeue();

                if (this.IsValidCallStack(current.callStackID) == false || _runtimeCallStack[current.callStackID].Count == 0)
                {
                    continue;
                }

                if (hasTargetNode)
                {
                    this.ProcessTargetedAbort(current);
                }
                else
                {
                    this.ProcessFullStackAbort(current);
                }
            }
        }


        /// <summary> 특정 노드까지의 타겟 중단을 처리 </summary>
        private void ProcessTargetedAbort(AbortInfo abortInfo)
        {
            int currentID = abortInfo.callStackID;
            NodeBase targetNode = abortInfo.targetNode;

            if (_runtimeCallStack[currentID].Count == 0)
            {
                return;
            }

            NodeBase stackNode = _runtimeCallStack[currentID].Peek();

            // 타겟 노드보다 깊은 depth의 노드들을 모두 정리
            while (stackNode.Equals(targetNode) == false && stackNode.depth > targetNode.depth)
            {
                this.ProcessNodeExit(stackNode);

                if (_runtimeCallStack[currentID].Count == 0)
                {
                    break;
                }

                stackNode = _runtimeCallStack[currentID].Peek();
            }
        }


        /// <summary> 전체 스택 중단을 처리 </summary>
        private void ProcessFullStackAbort(AbortInfo abortInfo)
        {
            int currentID = abortInfo.callStackID;

            if (_runtimeCallStack[currentID].Count == 0)
            {
                return;
            }

            NodeBase stackNode = _runtimeCallStack[currentID].Peek();
            this.ProcessNodeExit(stackNode);

            if (_runtimeCallStack[currentID].Count > 0)
            {
                // 다음 노드도 중단 큐에 추가
                NodeBase nextNode = _runtimeCallStack[currentID].Peek();
                _abortQueue.Enqueue(new AbortInfo(nextNode.callStackID));
            }
        }


        /// <summary> 노드 종료 처리 (병렬 노드의 경우 자식들도 중단) </summary>
        private void ProcessNodeExit(NodeBase node)
        {
            if (node is ParallelNode parallelNode)
            {
                parallelNode.Stop();

                List<NodeBase> children = parallelNode.GetChildren();
                int count = children.Count;

                // 병렬 노드의 모든 자식들을 중단 큐에 추가
                for (int i = 0; i < count; ++i)
                {
                    _abortQueue.Enqueue(new AbortInfo(children[i].callStackID));
                }
            }

            node.ExitNode();
        }

        /// <summary> 호출 스택이 유효한지 확인  </summary>
        private bool IsValidCallStack(int callStackID)
        {
            return callStackID >= 0 && callStackID < _runtimeCallStack.count;
        }
    }
}