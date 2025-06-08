using UnityEngine;

namespace BehaviourSystem.BT
{
    public class LogTunnelNode : DecoratorNode
    {
        public string[] onEnterMessages;
        public string[] onUpdateMessages;
        public string[] onExitMessages;
        
        
        protected override void OnEnter()
        {
            if (onEnterMessages is null || onEnterMessages.Length == 0)
            {
                return;
            }

            for (int i = 0; i < onEnterMessages.Length; ++i)
            {
                Debug.Log(onEnterMessages[i]);
            }
        }

        
        protected override EBehaviourResult OnUpdate()
        {
            if (onUpdateMessages is not null && onUpdateMessages.Length > 0)
            {
                for (int i = 0; i < onUpdateMessages.Length; ++i)
                {
                    Debug.Log(onUpdateMessages[i]);
                }
            }
            
            return child.UpdateNode();
        }

        
        protected override void OnExit()
        {
            if (onExitMessages is null || onExitMessages.Length == 0)
            {
                return;
            }
            
            for (int i = 0; i < onExitMessages.Length; ++i)
            {
                Debug.Log(onExitMessages[i]);
            }
        }
    }
}