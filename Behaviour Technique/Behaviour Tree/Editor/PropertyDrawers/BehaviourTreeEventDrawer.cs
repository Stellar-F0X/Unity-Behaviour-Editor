using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    [CustomPropertyDrawer(typeof(BehaviourTreeEvent))]
    public class BehaviourTreeEventDrawer : PropertyDrawer
    {
        private StringBuilder _eventHashKeyBuilder = new StringBuilder();

        private BehaviourActor _behaviourActor;
        private SerializedObject _serializedRuntimeTree;
        private SerializedProperty _serializedEventList;
        private SerializedProperty _serializedTargetEvent;

        private const string EVENT_KEY = "key";
        private const string EVENT_VALUE = "value";
        private const string EVENT_LIST_FIELD = "_behaviourEvents";


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (this.CachingSerializedRuntimeTreeElement(GetTargetEventKey(property)))
            {
                float height = EditorGUI.GetPropertyHeight(_serializedTargetEvent.FindPropertyRelative(EVENT_VALUE));
                return height + EditorGUIUtility.singleLineHeight;
            }
            else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (this.CachingSerializedRuntimeTreeElement(GetTargetEventKey(property)))
            {
                using (var scope = new EditorGUI.PropertyScope(position, label, property))
                {
                    EditorGUI.BeginChangeCheck();
                    position.y += EditorGUIUtility.singleLineHeight;
                    this.CreatePropertyField(ref position, _serializedTargetEvent, property.displayName);
                    property.boxedValue = _behaviourActor.GetBehaviourEvent(GetTargetEventKey(property));

                    if (EditorGUI.EndChangeCheck())
                    {
                        property.serializedObject.ApplyModifiedProperties();
                        _serializedEventList.serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            else
            {
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(position, "Event requires BehaviourTreeActor in Scene");
            }
        }


        /// <summary>
        /// 수정 중인 BehaviourTree Asset이 등록된 BehaviourActor 컴포넌터를 씬 내에서 찾고 직렬화시킨 뒤 파생되는 직렬화 객체들을 저장한다. 
        /// </summary>
        /// <param name="findEventKey"> 이벤트 리스트에서 찾을 이벤트 ID </param>
        /// <returns> 성공적으로 찾았는지 여부를 반환한다. </returns>
        private bool CachingSerializedRuntimeTreeElement(string findEventKey)
        {
            _behaviourActor = BehaviourTreeEditorWindow.Instance.Actor;

            if (_behaviourActor != null)
            {
                _serializedRuntimeTree = new SerializedObject(_behaviourActor);
                _serializedEventList = _serializedRuntimeTree.FindProperty(EVENT_LIST_FIELD);
                _serializedTargetEvent = this.FindTargetEventWithKey(findEventKey);
                return !ReferenceEquals(_serializedTargetEvent, null);
            }

            return false;
        }


        /// <summary>
        /// 직렬화된 이벤트 리스트에서 현재 찾는 이벤트 ID를 찾아내서 반환한다. 없으면 EventID와 함께 새로 만든다. 
        /// </summary>
        /// <param name="findEventKey"> 이벤트 리스트에서 찾을 이벤트 ID </param>
        /// <returns> 찾은 이벤트 ID와 일치하는 객체를 직렬화해서 반환한다. </returns>
        private SerializedProperty FindTargetEventWithKey(string findEventKey)
        {
            for (int i = 0; i < _serializedEventList.arraySize; ++i)
            {
                SerializedProperty arrayProperty = _serializedEventList.GetArrayElementAtIndex(i);
                SerializedProperty keyProperty = arrayProperty.FindPropertyRelative(EVENT_KEY);

                //찾을 경우
                if (string.Compare(keyProperty.stringValue, findEventKey) == 0)
                {
                    return arrayProperty;
                }
            }

            //없을 경우, 새로 만들어냄.
            _behaviourActor.AddBehaviourEvent(new BehaviourTreeEvent(findEventKey, new UnityEvent()));

            //SerializedObject는 오브젝트를 복사해서 만들어지기 때문에 이렇게 새롭게
            //SerializedObject를 만들지 않으면 _serializedEventList.arraySize 가 0으로 뜸.
            _serializedRuntimeTree = new SerializedObject(_behaviourActor);
            _serializedEventList = _serializedRuntimeTree.FindProperty(EVENT_LIST_FIELD);
            return _serializedEventList.GetArrayElementAtIndex(_serializedEventList.arraySize - 1);
        }


        /// <summary>
        ///  유니티 이벤트 필드를 Behaviour Tree Editor의 인스펙터에 생성한다.
        /// </summary>
        /// <param name="position"> 생성할 위치 </param>
        /// <param name="eventPoperty"> Event ID로 찾아낸 Event 직렬화 객체 </param>
        /// <param name="originalPropertyName"> 직렬화된 노드 객체의 유니티 이벤트 필드 변수명 </param>
        private void CreatePropertyField(ref Rect position, SerializedProperty eventPoperty, string originalPropertyName)
        {
            SerializedProperty property = eventPoperty.FindPropertyRelative(EVENT_VALUE);
            EditorGUI.PropertyField(position, property, new GUIContent(originalPropertyName));

            float spacingHeigth = EditorGUI.GetPropertyHeight(property);
            position.y += spacingHeigth + EditorGUIUtility.standardVerticalSpacing;
            position.height = spacingHeigth;
        }


        /// <summary>
        /// EventID를 만든다.
        /// </summary>
        /// <param name="property"> 직렬화된 노드 객체의 필드 </param>
        /// <returns> 생성된 EventID </returns>
        private string GetTargetEventKey(SerializedProperty property)
        {
            _eventHashKeyBuilder.Clear();

            _eventHashKeyBuilder.Append((property.serializedObject.targetObject as Node).guid);
            _eventHashKeyBuilder.Append(property.displayName.GetHashCode());

            return _eventHashKeyBuilder.ToString();
        }
    }
}
