using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    [UxmlElement]
    public partial class MiniMapView : MiniMap
    {
        private bool _activated;

        public void Setup(ToolbarToggle minimapActivateButton, BehaviourTreeView btView)
        {
            this.anchored = true;

            minimapActivateButton.UnregisterValueChangedCallback(this.ActiveMinimap);
            minimapActivateButton.RegisterValueChangedCallback(this.ActiveMinimap);

            btView.UnregisterCallback<GeometryChangedEvent>(this.UpdatePosition);
            btView.RegisterCallback<GeometryChangedEvent>(this.UpdatePosition);
        }


        private void ActiveMinimap(ChangeEvent<bool> activeEvent)
        {
            this._activated = activeEvent.newValue;
            this.visible = activeEvent.newValue;
            this.enabledSelf = activeEvent.newValue;
        }


        private void UpdatePosition(GeometryChangedEvent evt)
        {
            if (evt.newRect.width >= 240 && evt.newRect.height >= 240)
            {
                if (_activated)
                {
                    this.visible = true;
                    this.enabledSelf = true;
                }

                float x = evt.newRect.width - 220;
                float y = evt.newRect.height - 220;

                this.SetPosition(new Rect(x, y, 200, 200));
            }
            else
            {
                this.visible = false;
                this.enabledSelf = false;
            }
        }
    }
}