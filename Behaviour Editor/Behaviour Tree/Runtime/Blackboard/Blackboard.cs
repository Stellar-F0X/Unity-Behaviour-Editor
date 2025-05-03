using UnityEngine;
using System.Collections.Generic;

namespace BehaviourSystem.BT
{
    [CreateAssetMenu(fileName = "New Blackboard", menuName = "Behaviour Tree/Blackboard Asset")]
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


        public IBlackboardProperty FindProperty(in string key)
        {
            foreach (IBlackboardProperty prop in properties)
            {
                if (string.Compare(prop.key, key) == 0)
                {
                    return prop;
                }
            }

            return null;
        }
    }
}