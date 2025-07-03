using System;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class Variable : ISerializationCallbackReceiver, IComparable<Variable>
    {
        [SerializeField]
        protected string _typeName;

        protected Type _type;


        public Type type
        {
            get { return _type; }
            set { _type = value; }
        }

        public abstract EComparisonType comparison
        {
            get;
        }


        public static Variable Create(Type type)
        {
            Debug.Assert(type is not null, "Failed to create a variable.");
            Variable newVariable = Activator.CreateInstance(type) as Variable;
            Debug.Assert(newVariable is not null, "Failed to create a variable.");

            newVariable._type = type;
            return newVariable;
        }


        public virtual void OnBeforeSerialize()
        {
            Debug.Assert(_type is not null, "Failed to serialize a property.");
            this._typeName = _type.AssemblyQualifiedName;
        }


        public virtual void OnAfterDeserialize()
        {
            Debug.Assert(string.IsNullOrEmpty(_typeName) == false, "Failed to deserialize a property.");
            this._type = Type.GetType(_typeName);
        }


        public abstract void CloneValue(Variable originalVariable);

        public abstract int CompareTo(Variable other);

        public abstract BlackboardVariable AsBlackboardVariable();
    }


    [Serializable]
    public abstract class Variable<T> : Variable
    {
        [SerializeField]
        protected T _value;


        public virtual T value
        {
            get { return _value; }

            set { _value = value; }
        }


        public override sealed BlackboardVariable AsBlackboardVariable()
        {
            return new BlackboardVariable<T>(this);
        }


        public override sealed int CompareTo(Variable other)
        {
            if (other is Variable<T> == false)
            {
                Debug.LogError("Failed to compare two variables.");
                return Int32.MinValue;
            }
            
            return this.CompareTo(other as Variable<T>);
        }


        public override void CloneValue(Variable originalVariable)
        {
            Variable<T> converted = originalVariable as Variable<T>;
            Debug.Assert(converted is not null, "Failed to clone a property. The new property is null.");
            this._value = converted._value;
        }
        
        
        protected abstract int CompareTo(Variable<T> other);
    }
}