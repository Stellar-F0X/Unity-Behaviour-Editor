namespace BehaviourSystem.BT
{
    public enum ETickUpdateMode : byte
    {
        /// <summary> None </summary>
        None,
        
        /// <summary> Update </summary>
        NormalUpdate,
        
        /// <summary> Fixed Update </summary>
        FixedUpdate,
        
        /// <summary> Late Update </summary>
        LateUpdate,
        
        /// <summary> External Update </summary>
        ExternalUpdate,
    };
}