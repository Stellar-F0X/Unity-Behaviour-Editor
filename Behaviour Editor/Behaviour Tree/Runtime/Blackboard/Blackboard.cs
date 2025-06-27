using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Pool;

namespace BehaviourSystem.BT
{
    [CreateAssetMenu(fileName = "New Blackboard", menuName = "Behaviour System/Blackboard Asset")]
    public class Blackboard : ScriptableObject
    {
        [SerializeReference, HideInInspector]
        private List<IBlackboardProperty> _properties = new List<IBlackboardProperty>();

        public List<IBlackboardProperty> properties
        {
            get { return _properties; }
        }


        public Blackboard Clone()
        {
            Blackboard newBlackboard = ScriptableObject.CreateInstance<Blackboard>();
            newBlackboard._properties = new List<IBlackboardProperty>(this.properties.Count);

            for (int i = 0; i < this.properties.Count; ++i)
            {
                IBlackboardProperty prop = this._properties[i];
                newBlackboard._properties.Add(prop.Clone(prop));
            }

            return newBlackboard;
        }


        public static int StringToHash(in string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Blackboard key cannot be null or empty.");
                return -1;
            }

            return Animator.StringToHash(key);
        }
        
        
        public IBlackboardProperty[] FindProperties(in string key)
        {
            int hashCode = StringToHash(key);

            if (hashCode == -1)
            {
                return null;
            }

            List<IBlackboardProperty> foundProperties = ListPool<IBlackboardProperty>.Get();

            foreach (IBlackboardProperty prop in properties)
            {
                if (prop.hashCode == hashCode)
                {
                    foundProperties.Add(prop);
                }
            }

            IBlackboardProperty[] result = foundProperties.ToArray();
            ListPool<IBlackboardProperty>.Release(foundProperties);
            return result;
        }


        public IBlackboardProperty FindProperty(in string key)
        {
            int hashCode = StringToHash(key);

            if (hashCode == -1)
            {
                return null;
            }

            foreach (IBlackboardProperty prop in properties)
            {
                if (prop.hashCode == hashCode)
                {
                    return prop;
                }
            }

            return null;
        }


        public IBlackboardProperty FindProperty(in int key)
        {
            foreach (IBlackboardProperty prop in properties)
            {
                if (prop.hashCode == key)
                {
                    return prop;
                }
            }

            return null;
        }

        
        /// 중복된 키 이름을 방지하여 고유한 키를 생성합니다.
        /// 같은 이름이 있으면 (0), (1), (2) 등의 숫자를 붙입니다.
        public bool CheckAndGenerateUniqueKey(IBlackboardProperty property, bool created = false)
        {
            IBlackboardProperty[] foundProps = this.FindProperties(property.key);
            
            if (foundProps == null || (created ? foundProps.Length == 0 : foundProps.Length == 1))
            {
                return false;
            }

            int newIndex = 0;
            string baseKey = property.key;
            Match match = Regex.Match(property.key, @"\((\d+)\)$");
            
            if (match.Success)
            {
                baseKey = property.key.Substring(0, match.Index);
                baseKey = baseKey.TrimEnd();
            }
            
            while (this.FindProperty($"{baseKey} ({newIndex})") != null)
            {
                newIndex++;
            }

            property.key = $"{baseKey} ({newIndex})";
            return true;
        }
    }
}