using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace BehaviourSystem.BT
{
#if UNITY_EDITOR
    public class GroupDataSet : ScriptableObject
    {
        [SerializeField]
        private List<GroupData> _groupDataList = new List<GroupData>();


        public IReadOnlyList<GroupData> dataList
        {
            get { return _groupDataList; }
        }

        
        public GroupDataSet Clone()
        {
            GroupDataSet newSet = CreateInstance<GroupDataSet>();
            newSet._groupDataList = new List<GroupData>(this._groupDataList.Count);
            
            foreach (GroupData data in this._groupDataList)
            {
                newSet._groupDataList.Add(Instantiate(data));
            }
            
            return newSet;
        }
        
        
        public GroupData CreateGroupData(string title, Vector2 position)
        {
            GroupData newGroupData = CreateInstance<GroupData>();
            newGroupData.hideFlags = HideFlags.HideInHierarchy;
            newGroupData.title = title;
            newGroupData.position =  position;

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(this, "Behaviour Tree (CreateGroup)");
            }

            _groupDataList.Add(newGroupData);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RegisterCreatedObjectUndo(newGroupData, "Behaviour Tree (CreateGroup)");
                AssetDatabase.AddObjectToAsset(newGroupData, this);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }

            return newGroupData;
        }


        public void DeleteGroupData(GroupData data)
        {
            if (data is null)
            {
                return;
            }
            
            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(this, "Behaviour Tree (RemoveGroup)");
            }

            _groupDataList.Remove(data);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.DestroyObjectImmediate(data);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }
    }
#endif
}