using System.Collections.Generic;
using System.Linq;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    public class NodeGroupView : Group
    {
        public NodeGroupView(GroupDataSet dataSet, GroupData dataContainer) : base()
        {
            this._data = dataContainer;
            this._groupDataDataSet = dataSet;

            this.title = dataContainer.title;
            this.style.backgroundColor = BehaviourTreeEditor.Settings.nodeGroupColor;
        }

        private readonly GroupDataSet _groupDataDataSet;
        private readonly GroupData _data;


        public GroupData data
        {
            get { return _data; }
        }


        protected override void OnGroupRenamed(string oldName, string newName)
        {
            Undo.RecordObject(_data, "Behaviour Tree (NodeGroupViewNameChanged)");
            base.OnGroupRenamed(oldName, newName);
            _data.title = newName;
            EditorUtility.SetDirty(_groupDataDataSet);
        }


        protected override void SetScopePositionOnly(Rect newPos)
        {
            this._data.ChangeNodePosition(newPos.position);
            base.SetScopePositionOnly(newPos);
        }


        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            if (BehaviourTreeEditor.Instance is null || _data is null)
            {
                return;
            }

            if (BehaviourTreeEditor.CanEditTree && BehaviourTreeEditor.isInLoadingBTAsset == false)
            {
                _data.AddNodeGuids(elements.Where(x => x.selected && x is NodeView).ConvertAll(x => ((NodeView)x).targetNode));
            }
        }


        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            if (BehaviourTreeEditor.Instance is null || _data is null)
            {
                return;
            }

            if (BehaviourTreeEditor.CanEditTree && BehaviourTreeEditor.isInLoadingBTAsset == false)
            {
                _data.RemoveNodeGuids(elements.Where(x => x.selected && x is NodeView).ConvertAll(x => ((NodeView)x).targetNode));
            }
        }
    }
}