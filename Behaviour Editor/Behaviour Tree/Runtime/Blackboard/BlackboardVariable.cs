using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class BlackboardVariable
    {
        [SerializeReference]
        protected Variable _variable;

        [SerializeField]
        protected string _name;

        [SerializeField]
        protected int _nameHash;


        public Variable variable
        {
            get { return _variable; }

            set { _variable = value; }
        }

        public Type type
        {
            get { return _variable.type; }

            set { _variable.type = value; }
        }

        public string name
        {
            get { return _name; }

            set { this.TryChangeName(value); }
        }

        public int nameHash
        {
            get { return _nameHash; }
        }

        public EComparisonType comparison
        {
            get { return _variable.comparison; }
        }


        protected void TryChangeName(string newKey)
        {
            if (string.IsNullOrEmpty(newKey))
            {
                Debug.LogError("Key value is empty. Please enter a valid key.");
                return;
            }

            // Prevent blackboard key changes during runtime
            if (Application.isPlaying)
            {
                Debug.LogError("Cannot change blackboard key while the game is running.");
                return;
            }

            this._name = newKey;
            this._nameHash = GraphFactory.StringToHash(this.name);
        }


        public static BlackboardVariable Create(Type variableType)
        {
            if (Activator.CreateInstance(variableType) is Variable variable)
            {
                BlackboardVariable blackboardVariable = variable.AsBlackboardVariable();
                blackboardVariable.name = $"New {variableType.Name}";
                blackboardVariable.type = variableType;
                return blackboardVariable;
            }

            Debug.LogAssertion($"Failed to create variable of type {variableType}");
            return null;
        }


        public static BlackboardVariable Clone(BlackboardVariable origin)
        {
            BlackboardVariable newVariable = BlackboardVariable.Create(origin.type);
            Debug.Assert(newVariable is not null, "Failed to clone a variable.");

            newVariable._name = origin._name;
            newVariable._nameHash = origin._nameHash;
            newVariable._variable.type = origin._variable.type;
            newVariable._variable.CloneValue(origin._variable);
            return newVariable;
        }
    }


    [Serializable]
    public class BlackboardVariable<T> : BlackboardVariable
    {
        public BlackboardVariable(Variable<T> variable)
        {
            this._variable = variable;
            this._value = variable.value;
        }


        [SerializeReference]
        private T _value;


        public T value
        {
            get { return ((Variable<T>)_variable).value ?? (_value); }

            set { ((Variable<T>)_variable).value = (_value) = value; }
        }
    }
}