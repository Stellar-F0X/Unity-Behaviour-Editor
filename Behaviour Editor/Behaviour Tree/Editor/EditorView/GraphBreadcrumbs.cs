using System;
using System.Collections.Generic;
using System.Linq;
using BehaviourSystem.BT;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    [UxmlElement]
    public partial class GraphBreadcrumbs : ToolbarBreadcrumbs
    {
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
                else
                {
                    targetIndex++;
                }
            }
        }
    }
}