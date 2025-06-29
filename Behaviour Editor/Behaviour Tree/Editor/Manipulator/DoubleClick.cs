using BehaviourSystem.BT;
using BehaviourSystem.BT.State;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    //Referenced: https://github.com/thekiwicoder0/UnityBehaviourTreeEditor/blob/main/Editor/DoubleClickNode.cs
    public class DoubleClick : MouseManipulator
    {
        public DoubleClick(float doubleClickDuration)
        {
            _doubleClickDuration = doubleClickDuration;
        }

        private double _measurementStartTime = EditorApplication.timeSinceStartup;

        private double _doubleClickDuration;


        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }


        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        }


        private void OnMouseDown(MouseDownEvent evt)
        {
            if (base.CanStopManipulation(evt) == false)
            {
                return;
            }

            NodeView clickedElement = evt.target as NodeView;

            if (clickedElement is null)
            {
                VisualElement element = evt.target as VisualElement;

                clickedElement = element.GetFirstAncestorOfType<NodeView>();
            }

            if (clickedElement is not null)
            {
                double duration = EditorApplication.timeSinceStartup - _measurementStartTime;

                if (duration < _doubleClickDuration)
                {
                    this.OnDoubleClick(evt, clickedElement);
                    evt.StopImmediatePropagation();
                }

                _measurementStartTime = EditorApplication.timeSinceStartup;
            }
        }


        private void OnDoubleClick(MouseDownEvent evt, NodeView clickedElement)
        {
            GraphAsset graphAsset = null;

            switch (clickedElement.targetNode)
            {
                case SubGraphNode subGraphNode: graphAsset = subGraphNode.subGraph; break;

                case SubGraphState subGraphState: graphAsset = subGraphState.subGraph; break;
            }

            if (graphAsset != null)
            {
                BehaviorEditor.Instance.ChangeGraph(graphAsset, true);
            }
        }
    }
}