using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public class ParallelNode : ParallelNodeBase
    {
        public enum EParallelPolicy
        {
            [Tooltip("Returns Success only when all child nodes succeed. Returns Failure immediately if any child fails.")]
            RequireAllSuccess,

            [Tooltip("Returns Success immediately when at least one child node succeeds. Returns Failure only when all children fail.")]
            RequireOneSuccess,

            [Tooltip("Returns Success only when all child nodes fail. Returns Failure immediately if any child succeeds.")]
            RequireAllFailure,

            [Tooltip("Returns Success immediately when at least one child node fails. Returns Failure only when all children succeed.")]
            RequireOneFailure,
            
            [Tooltip("Always returns Success after all child nodes complete, regardless of their individual results.")]
            WaitForAllUnconditional
        };

        public EParallelPolicy parallelPolicy;

        private int _successfulChildCount = 0;
        private int _failedChildCount = 0;


        public override string tooltip
        {
            get { return "Executes multiple child nodes simultaneously. \nSuccess/failure is determined by the configured policy."; }
        }
        

        protected override void OnEnter()
        {
            _failedChildCount = 0;
            _successfulChildCount = 0;

            base.OnEnter();
        }


        protected override EBehaviourResult OnUpdate(in float deltaTime)
        {
            int count = children.Count;

            for (int i = 0; i < count; ++i)
            {
                if (_isChildStopped[i] == false)
                {
                    switch (children[i].UpdateNode())
                    {
                        case EBehaviourResult.Success:
                        {
                            _successfulChildCount++;
                            _isChildStopped[i] = true;
                            break;
                        }

                        case EBehaviourResult.Failure:
                        {
                            _failedChildCount++;
                            _isChildStopped[i] = true;
                            break;
                        }
                    }
                }
            }

            return this.EvaluatePolicy();
        }
        
        
        public void Stop()
        {
            base.Stop();
            
            _failedChildCount = 0;
            _successfulChildCount = 0;
        }
        
        
        protected override EBehaviourResult EvaluatePolicy()
        {
            switch (parallelPolicy)
            {
                case EParallelPolicy.RequireAllSuccess:
                {
                    if (_successfulChildCount == children.Count)
                    {
                        return EBehaviourResult.Success;
                    }

                    if (_failedChildCount > 0)
                    {
                        return EBehaviourResult.Failure;
                    }

                    return EBehaviourResult.Running;
                }

                case EParallelPolicy.RequireAllFailure:
                {
                    if (_failedChildCount == children.Count)
                    {
                        return EBehaviourResult.Success;
                    }

                    if (_successfulChildCount > 0)
                    {
                        return EBehaviourResult.Failure;
                    }

                    return EBehaviourResult.Running;
                }

                case EParallelPolicy.RequireOneSuccess:
                {
                    if (_successfulChildCount > 0)
                    {
                        return EBehaviourResult.Success;
                    }

                    if (_successfulChildCount + _failedChildCount == children.Count)
                    {
                        return EBehaviourResult.Failure;
                    }

                    return EBehaviourResult.Running;
                }

                case EParallelPolicy.RequireOneFailure:
                {
                    if (_failedChildCount > 0)
                    {
                        return EBehaviourResult.Success;
                    }

                    if (_successfulChildCount + _failedChildCount == children.Count)
                    {
                        return EBehaviourResult.Failure;
                    }

                    return EBehaviourResult.Running;
                }

                case EParallelPolicy.WaitForAllUnconditional:
                {
                    if (_successfulChildCount + _failedChildCount == children.Count)
                    {
                        return EBehaviourResult.Success;
                    }

                    return EBehaviourResult.Running;
                }
            }

            return EBehaviourResult.Running;
        }
    }
}
