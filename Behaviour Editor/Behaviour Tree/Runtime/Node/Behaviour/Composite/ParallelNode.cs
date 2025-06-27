using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public class ParallelNode : CompositeNode
    {
        public enum EParallelPolicy : byte
        {
            [Tooltip("Returns Success only when all child nodes succeed. Returns Failure immediately if any child fails.")]
            RequireAllSuccess,

            [Tooltip("Returns Success immediately when at least one child node succeeds. Returns Failure only when all children fail.")]
            RequireOneSuccess,

            [Tooltip("Returns Success only when all child nodes fail. Returns Failure immediately if any child succeeds.")]
            RequireAllFailure,

            [Tooltip("Returns Success immediately when at least one child node fails. Returns Failure only when all children succeed.")]
            RequireOneFailure
        };


        public EParallelPolicy parallelPolicy;

        private int _successfulChildCount = 0;
        private int _failedChildCount = 0;

        private List<bool> _isChildStopped;


        public override string tooltip
        {
            get { return "Executes multiple child nodes simultaneously. \nSuccess/failure is determined by the configured policy."; }
        }


        public override void PostCreation()
        {
            _isChildStopped = new List<bool>(children.Count);

            for (int i = 0; i < children.Count; ++i)
            {
                _isChildStopped.Add(false);
            }
        }


        protected override void OnEnter()
        {
            _failedChildCount = 0;
            _successfulChildCount = 0;

            if (children is null)
            {
                return;
            }

            int count = children.Count;

            for (int i = 0; i < count; ++i)
            {
                _isChildStopped[i] = false;
            }
        }


        protected override EStatus OnUpdate()
        {
            int count = children.Count;

            for (int i = 0; i < count; ++i)
            {
                if (_isChildStopped[i] == false)
                {
                    switch (children[i].UpdateNode())
                    {
                        case EStatus.Success:
                        {
                            _successfulChildCount++;
                            _isChildStopped[i] = true;
                            break;
                        }

                        case EStatus.Failure:
                        {
                            _failedChildCount++;
                            _isChildStopped[i] = true;
                            break;
                        }
                    }
                }

                EStatus result = this.EvaluatePolicy();

                if (result != EStatus.Running)
                {
                    return result;
                }
            }
            
            return this.EvaluatePolicy();
        }


        protected override void OnExit()
        {
            if (children is null)
            {
                return;
            }

            int count = children.Count;

            for (int i = 0; i < count; ++i)
            {
                if (_isChildStopped[i] == false)
                {
                    int id = children[i].callStackID;
                    runner.handler.AbortSubtree(id);
                }
                else
                {
                    // Reset the stopped state
                    _isChildStopped[i] = false;
                }
            }
            
            this._failedChildCount = 0;
            this._successfulChildCount = 0;
        }


        protected virtual EStatus EvaluatePolicy()
        {
            switch (parallelPolicy)
            {
                case EParallelPolicy.RequireAllSuccess:
                {
                    if (_successfulChildCount == children.Count)
                    {
                        return EStatus.Success;
                    }

                    if (_failedChildCount > 0)
                    {
                        return EStatus.Failure;
                    }

                    return EStatus.Running;
                }

                case EParallelPolicy.RequireAllFailure:
                {
                    if (_failedChildCount == children.Count)
                    {
                        return EStatus.Success;
                    }

                    if (_successfulChildCount > 0)
                    {
                        return EStatus.Failure;
                    }

                    return EStatus.Running;
                }

                case EParallelPolicy.RequireOneSuccess:
                {
                    if (_successfulChildCount > 0)
                    {
                        return EStatus.Success;
                    }

                    if (_successfulChildCount + _failedChildCount == children.Count)
                    {
                        return EStatus.Failure;
                    }

                    return EStatus.Running;
                }

                case EParallelPolicy.RequireOneFailure:
                {
                    if (_failedChildCount > 0)
                    {
                        return EStatus.Success;
                    }

                    if (_successfulChildCount + _failedChildCount == children.Count)
                    {
                        return EStatus.Failure;
                    }

                    return EStatus.Running;
                }
            }

            return EStatus.Running;
        }
    }
}