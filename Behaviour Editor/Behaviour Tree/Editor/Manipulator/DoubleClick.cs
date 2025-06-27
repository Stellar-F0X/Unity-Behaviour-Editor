using BehaviourSystem.BT;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    //Referenced: https://github.com/thekiwicoder0/UnityBehaviourTreeEditor/blob/main/Editor/DoubleClickNode.cs
    public class DoubleClick : MouseManipulator
    {
        private double _measurementStartTime = EditorApplication.timeSinceStartup;

        private double _doubleClickDuration = 0.3;


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
            if (clickedElement.targetNode is SubGraphNode subtreeNode)
            {
                var treeToFocus = subtreeNode.subGraph;

                if (treeToFocus != null)
                {
                    BehaviourSystemEditor.Instance.DirectoryPath.PushItem(treeToFocus.name, () =>
                    {
                        BehaviourSystemEditor.Instance.ChangeGraph(treeToFocus);
                        //BehaviourSystemEditor.Instance.DirectoryPath.PopItem();
                    });

                    BehaviourSystemEditor.Instance.ChangeGraph(treeToFocus);
                }
            }
        }
    }
}