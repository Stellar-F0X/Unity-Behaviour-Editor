using System;
using UnityEngine;

namespace BehaviourTechnique
{ [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
        public readonly bool isRuntimeOnly;

        public ReadOnlyAttribute(bool isRuntimeOnly = false)
        {
            this.isRuntimeOnly = isRuntimeOnly;
        }
    }
}
