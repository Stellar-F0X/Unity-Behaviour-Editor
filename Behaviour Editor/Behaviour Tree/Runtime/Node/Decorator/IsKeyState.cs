using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace BehaviourSystem.BT
{
#if ENABLE_INPUT_SYSTEM
    [Serializable]
    public class IsKeyState : DecoratorNode
    {
        public enum EKeyState
        {
            Down,
            Pressing,
            Up
        }

        public InputActionAsset inputAsset;
        public string actionMapName;
        public EKeyState keyState;

        private bool _invalidKeyName;
        private InputAction _inputAction;


        public override void PostTreeCreation()
        {
            if (inputAsset is null)
            {
                Debug.LogError("No Input Map");
                _invalidKeyName = false;
            }
            else
            {
                _inputAction = inputAsset.FindAction(actionMapName);

                if (_inputAction is null)
                {
                    Debug.LogError($"No Input Action found through actionMapName: {actionMapName}");
                    _invalidKeyName = false;
                }
            }
        }


        protected override EStatus OnUpdate()
        {
            if (_invalidKeyName)
            {
                Debug.LogWarning("No Input Action found through actionMapName.");
            }
            else
            {
                switch (keyState)
                {
                    case EKeyState.Up:
                    {
                        if (_inputAction.WasReleasedThisFrame())
                        {
                            return EStatus.Success;
                        }

                        break;
                    }

                    case EKeyState.Pressing:
                    {
                        if (_inputAction.IsPressed())
                        {
                            return EStatus.Success;
                        }

                        break;
                    }

                    case EKeyState.Down:
                    {
                        if (_inputAction.WasPressedThisFrame())
                        {
                            return EStatus.Success;
                        }

                        break;
                    }
                }
            }

            return EStatus.Failure;
        }
#endif
    }
}