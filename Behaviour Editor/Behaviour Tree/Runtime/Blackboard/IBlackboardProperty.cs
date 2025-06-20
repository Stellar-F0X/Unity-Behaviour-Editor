using System;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public interface IBlackboardProperty
    {
        public string key
        {
            get;
            set;
        }

        public Type type
        {
            get;
            set;
        }
        
        public int hashCode
        {
            get;
        }

        public EConditionType comparableConditions
        {
            get;
        }

        public static IBlackboardProperty Create(Type propertyType)
        {
            if (Activator.CreateInstance(propertyType) is IBlackboardProperty property)
            {
                property.key = $"New {propertyType.Name}";
                property.type = propertyType;
                return property;
            }

            Debug.LogAssertion($"Failed to create property of type {propertyType}");
            return null;
        }

        public IBlackboardProperty Clone(IBlackboardProperty origin);
    }
}