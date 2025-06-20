using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystem.BT
{
#if UNITY_EDITOR
    [Serializable]
    public class GroupData : ScriptableObject, ISerializationCallbackReceiver
    {
        public string title;
        public Vector2 position;

        [SerializeField]
        private List<string> _nodeGuidList = new List<string>();

        private HashSet<string> _nodeGuidSet = new HashSet<string>(StringComparer.Ordinal);


        public int containedNodeCount
        {
            get { return _nodeGuidList.Count; }
        }


        public void OnBeforeSerialize()
        {
            if (_nodeGuidSet is not null)
            {
                _nodeGuidList.Clear();
                _nodeGuidList.AddRange(_nodeGuidSet);
            }
        }


        public void OnAfterDeserialize()
        {
            if (_nodeGuidSet is null)
            {
                _nodeGuidSet = new HashSet<string>(_nodeGuidList, StringComparer.Ordinal);
            }
            else
            {
                _nodeGuidSet.Clear();
                _nodeGuidList.ForEach(e => _nodeGuidSet.Add(e));
            }
        }


        public bool Contains(string nodeGuid)
        {
            return _nodeGuidSet.Contains(nodeGuid);
        }


        public void ChangeNodePosition(Vector2 newPosition)
        {
            if (this.containedNodeCount > 0)
            {
                position = newPosition;
            }
            else
            {
                //NodeView도 위치를 Record하는데, GroupView를 움직이면 NodeView도 움직이며 위치가 기록되어 Undo 기록이 중첩됨.
                //따라서 Group에 요소가 있는 상태로 움직인 후 GroupView가 정상적으로 동작하려면 여러번 Undo해야 되며
                //또한 NodeView를 기준으로 GroupView 위치가 정해지기 때문에 Group에 요소가 없는 상태일 때만 기록시킴.  
                Undo.RecordObject(this, "Behaviour Tree (NodeGroupViewPositionChanged)");
                position = newPosition;
                EditorUtility.SetDirty(this);
            }
        }


        public void AddNodeGuid(NodeBase nodeBase)
        {
            if (nodeBase == null || _nodeGuidSet.Contains(nodeBase.guid))
            {
                return;
            }

            Undo.RecordObject(this, "Behaviour Tree (AddNodeGuidToGroup)");
            _nodeGuidSet.Add(nodeBase.guid);
            EditorUtility.SetDirty(this);
        }


        public void RemoveNodeGuid(NodeBase nodeBase)
        {
            //Object에서 연산자 오버라이딩된 == null을 통해 해당 프레임에서 삭제된 객체를 체크.
            if (nodeBase == null || _nodeGuidSet.Contains(nodeBase.guid) == false)
            {
                return;
            }

            Undo.RecordObject(this, "Behaviour Tree (RemoveNodeGuidToGroup)");
            _nodeGuidSet.Remove(nodeBase.guid);
            EditorUtility.SetDirty(this);
        }
    }
#endif
}