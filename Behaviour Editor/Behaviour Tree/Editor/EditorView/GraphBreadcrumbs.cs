using System;
using System.Collections.Generic;
using System.Linq;
using BehaviourSystem.BT;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    [UxmlElement]
    public partial class GraphBreadcrumbs : ToolbarBreadcrumbs
    {
        public GraphBreadcrumbs() : base()
        {
            base.UnregisterCallback<GeometryChangedEvent>(this.GeometryChanged);
            base.RegisterCallback<GeometryChangedEvent>(this.GeometryChanged);
        }

        private const float _TEXT_SIZE_OFFSET = 15f;


        public void PushItem(GraphAsset asset, Action onItemClicked)
        {
            base.PushItem(asset.name, onItemClicked);
            VisualElement lastChild = this.Children().Last();
            lastChild.AddToClassList(asset.guid.ToString());
        }


        public void PopToClickItems(in UGUID uguid)
        {
            int targetIndex = 0;

            foreach (VisualElement child in this.Children())
            {
                if (child.ClassListContains(uguid.ToString()))
                {
                    for (int i = base.childCount - 1; i > targetIndex; i--)
                    {
                        this.PopItem();
                    }

                    return;
                }

                targetIndex++;
            }
        }


        private void GeometryChanged(GeometryChangedEvent evt)
        {
            foreach (VisualElement child in this.Children())
            {
                if (child is ToolbarButton button)
                {
                    Vector2 textSize = button.MeasureTextSize(button.text, 0, MeasureMode.Undefined, 0, MeasureMode.Undefined);
                    float buttonWidth = button.resolvedStyle.width;
                    
                    if (buttonWidth < textSize.x + _TEXT_SIZE_OFFSET)
                    {
                        button.visible = false;
                        button.SetEnabled(false);
                    }
                    else
                    {
                        button.visible = true;
                        button.SetEnabled(true);
                    }
                }
            }
        }
    }
}