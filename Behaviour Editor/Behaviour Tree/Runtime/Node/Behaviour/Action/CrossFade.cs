using UnityEngine;

namespace BehaviourSystem.BT
{
    public class CrossFade : ActionNode
    {
        [SerializeReference]
        public BlackboardProperty<Animator> animator;

        public string animationName;
        
        [SerializeField, ReadOnly]
        private int _animationHash;
        
        public int layer = 0;
        
        [Tooltip("Duration of the crossfade transition in seconds.")]
        public float transitionDuration = 0.1f;
        public float timeOffset = 0;
        
        [Tooltip("Time in seconds to wait for the transition to complete before returning success.")]
        public float transitionTime = 0;

        [Header("Options")]
        public bool useFixedCrossFade;
        public bool waitForTransition;

        private float _playStartTime;
        private bool _invalidated;


        public override void PostCreation()
        {
            if (string.IsNullOrEmpty(animationName))
            {
                Debug.LogError($"{typeof(CrossFade)}: Animation Name is empty.");
                _animationHash = -1;
                return;
            }
            
            _animationHash = Animator.StringToHash(animationName);
        }


        protected override void OnEnter()
        {
            if (_animationHash == -1)
            {
                return;
            }
            
            if (useFixedCrossFade)
            {
                animator.value.CrossFadeInFixedTime(_animationHash, transitionDuration, layer, timeOffset, transitionTime);
            }
            else
            {
                animator.value.CrossFade(_animationHash, transitionDuration, layer, timeOffset, transitionTime);
            }

            _playStartTime = Time.time;
        }


        protected override EStatus OnUpdate()
        {
            if (_animationHash == -1)
            {
                return EStatus.Failure;
            }
            
            if (waitForTransition)
            {
                if (Time.time >= _playStartTime + transitionTime)
                {
                    return EStatus.Success;
                }

                return EStatus.Running;
            }
            else
            {
                //기다리지 않는다면, 즉시 성공 반환.
                return EStatus.Success;
            }
        }
    }
}