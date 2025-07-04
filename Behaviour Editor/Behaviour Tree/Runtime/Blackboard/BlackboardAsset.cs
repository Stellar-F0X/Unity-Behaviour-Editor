using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Pool;

namespace BehaviourSystem.BT
{
    [CreateAssetMenu(fileName = "New Blackboard", menuName = "Behavior System/Blackboard Asset", order = 3)]
    public class BlackboardAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeReference, HideInInspector]
        private List<BlackboardVariable> _variables = new List<BlackboardVariable>();

        private readonly Dictionary<int, BlackboardVariable> _variableCache = new Dictionary<int, BlackboardVariable>();


        public List<BlackboardVariable> variables
        {
            get { return _variables; }
        }


        public BlackboardAsset Clone()
        {
            BlackboardAsset newBlackboardAsset = ScriptableObject.CreateInstance<BlackboardAsset>();
            newBlackboardAsset._variables = new List<BlackboardVariable>(this._variables.Count);

            for (int i = 0; i < this.variables.Count; ++i)
            {
                newBlackboardAsset._variables.Add(BlackboardVariable.Clone(this._variables[i]));
            }

            return newBlackboardAsset;
        }


        public BlackboardVariable FindVariable(in string key)
        {
            int hashCode = GraphFactory.StringToHash(key);

            if (hashCode == -1)
            {
                return null;
            }

            if (_variableCache.TryGetValue(hashCode, out BlackboardVariable result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }



        public void AddVariable(BlackboardVariable variable)
        {
            BlackboardVariable foundVariable = this.FindVariable(variable.name);

            if (foundVariable != null)
            {
                variable.name = this.GenerateUniqueVariableName(variable.name);
            }

            _variableCache.Add(variable.nameHash, variable);
        }


        public void RemoveVariable(BlackboardVariable variable)
        {
            BlackboardVariable foundVariable = this.FindVariable(variable.name);

            if (foundVariable == null)
            {
                Debug.LogError("해당 블랙보드 변수를 찾을 수 없습니다.");
            }
            else
            {
                _variableCache.Remove(variable.nameHash);
            }
        }


        public bool TryChangeVariableName(BlackboardVariable variable, in string newName)
        {
            BlackboardVariable foundVariable = this.FindVariable(newName);

            if (foundVariable == variable)
            {
                return false;
            }
            
            if (foundVariable == null)
            {
                variable.name = newName;
                return true;
            }
            
            _variableCache.Remove(variable.nameHash);
            variable.name = this.GenerateUniqueVariableName(newName);
            _variableCache.Add(variable.nameHash, variable);
            return true;
        }
        

        private string GenerateUniqueVariableName(string variableName)
        {
            int newIndex = 0;
            string baseKey = variableName;
            Match match = Regex.Match(variableName, @"\((\d+)\)$");

            if (match.Success)
            {
                baseKey = variableName.Substring(0, match.Index);
                baseKey = baseKey.TrimEnd();
            }

            while (this.FindVariable($"{baseKey} ({newIndex})") != null)
            {
                newIndex++;
            }

            return $"{baseKey} ({newIndex})";
        }


        public void OnBeforeSerialize()
        {
            if (_variableCache is null || _variableCache.Count == 0)
            {
                return;
            }

            _variables.Clear();

            foreach (BlackboardVariable variable in _variableCache.Values)
            {
                _variables.Add(variable);
            }
        }


        public void OnAfterDeserialize()
        {
            if (_variables is null || _variableCache is null)
            {
                return;
            }

            foreach (BlackboardVariable variable in _variables)
            {
                _variableCache[variable.nameHash] = variable;
            }
        }
    }
}