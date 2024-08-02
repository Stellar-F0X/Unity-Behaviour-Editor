using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTechnique.BehaviourTreeEditor.Setting
{
    [CreateAssetMenu]
    public class BehaviourTreeEditorSettings : ScriptableObject
    {
        [Space]
        public float enlargementScale = 2.5f;
        public float reductionScale = 0.2f;
    }
}
