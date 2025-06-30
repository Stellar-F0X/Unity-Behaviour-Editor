using System;
using System.Reflection;

namespace BehaviourSystem.BT
{
    public ref struct FieldReflectionDesc
    {
        public FieldReflectionDesc(BindingFlags flagSettings, Type[] includeTypes)
        {
            this.flagSettings = flagSettings;
            this.includeTypes = includeTypes;
        }
        
        public BindingFlags flagSettings;
        public Type[] includeTypes;
    }
}