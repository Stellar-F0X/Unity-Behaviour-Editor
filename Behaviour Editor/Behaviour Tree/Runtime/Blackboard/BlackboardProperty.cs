using System;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class BlackboardProperty<T> : IBlackboardProperty, ISerializationCallbackReceiver, IComparable<IBlackboardProperty>
    {
        [SerializeField]
        private string _key;

        [SerializeField]
        private T _value;
        
        [SerializeField]
        private string _typeName;
        private Type _type;

        public virtual string key
        {
            get { return _key; }
            set { _key = value; }
        }

        public virtual T value
        {
            get { return _value; }
            set { _value = value; }
        }

        public Type type
        {
            get { return _type; }
            set { _type = value; }
        }

        public virtual EConditionType comparableConditions
        {
            get { return EConditionType.None; }
        }

        public void OnBeforeSerialize()
        {
            _typeName = _type.AssemblyQualifiedName;
        }

        public void OnAfterDeserialize()
        {
            _type = Type.GetType(_typeName);
        }

        public IBlackboardProperty Clone(IBlackboardProperty origin)
        {
            BlackboardProperty<T> newProperty = IBlackboardProperty.Create(origin.type) as BlackboardProperty<T>;

            if (newProperty is null)
            {
                throw new TypeAccessException("Failed to clone a property. The new property is null.");
            }
            
            if (origin is BlackboardProperty<T> originalProp)
            {
                newProperty._key      = originalProp._key;
                newProperty._value    = originalProp._value;
                newProperty._typeName = originalProp._type.AssemblyQualifiedName;
                newProperty._type     = originalProp._type;
            }

            return newProperty;
        }

        public virtual int CompareTo(IBlackboardProperty other)
        {
            return -1;
        }
    }
}