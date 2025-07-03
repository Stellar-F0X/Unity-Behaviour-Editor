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

        private Dictionary<int, BlackboardVariable> _propertyCache = new Dictionary<int, BlackboardVariable>();

        
        public List<BlackboardVariable> variables
        {
            get { return _variables; }
        }


        public BlackboardAsset Clone()
        {
            BlackboardAsset newBlackboardAsset = ScriptableObject.CreateInstance<BlackboardAsset>();
            newBlackboardAsset._variables = new List<BlackboardVariable>(this.variables.Count);

            for (int i = 0; i < this.variables.Count; ++i)
            {
                newBlackboardAsset._variables.Add(BlackboardVariable.Clone(this._variables[i]));
            }

            return newBlackboardAsset;
        }

        
        public BlackboardVariable[] FindProperties(in string key)
        {
            int hashCode = GraphFactory.StringToHash(key);

            if (hashCode == -1)
            {
                return null;
            }

            List<BlackboardVariable> foundProperties = ListPool<BlackboardVariable>.Get();

            foreach (BlackboardVariable prop in variables)
            {
                if (prop.nameHash == hashCode)
                {
                    foundProperties.Add(prop);
                }
            }

            BlackboardVariable[] result = foundProperties.ToArray();
            ListPool<BlackboardVariable>.Release(foundProperties);
            return result;
        }


        public BlackboardVariable FindProperty(in string key)
        {
            int hashCode = GraphFactory.StringToHash(key);

            if (hashCode == -1)
            {
                return null;
            }

            foreach (BlackboardVariable prop in variables)
            {
                if (prop.nameHash == hashCode)
                {
                    return prop;
                }
            }

            return null;
        }


        public BlackboardVariable FindProperty(in int key)
        {
            foreach (BlackboardVariable prop in variables)
            {
                if (prop.nameHash == key)
                {
                    return prop;
                }
            }

            return null;
        }

        
        /// 중복된 키 이름을 방지하여 고유한 키를 생성합니다.
        /// 같은 이름이 있으면 (0), (1), (2) 등의 숫자를 붙입니다.
        public bool CheckAndGenerateUniqueKey(BlackboardVariable property, bool created = false)
        {
            BlackboardVariable[] foundProps = this.FindProperties(property.name);
            
            if (foundProps == null || (created ? foundProps.Length == 0 : foundProps.Length == 1))
            {
                return false;
            }

            int newIndex = 0;
            string baseKey = property.name;
            Match match = Regex.Match(property.name, @"\((\d+)\)$");
            
            if (match.Success)
            {
                baseKey = property.name.Substring(0, match.Index);
                baseKey = baseKey.TrimEnd();
            }
            
            while (this.FindProperty($"{baseKey} ({newIndex})") != null)
            {
                newIndex++;
            }

            property.name = $"{baseKey} ({newIndex})";
            return true;
        }

        
        //TODO : 이거 구현
        //Dic to List
        public void OnBeforeSerialize()
        {
            
        }
        
        //List to Dic 
        public void OnAfterDeserialize()
        {
            
        }
    }
}