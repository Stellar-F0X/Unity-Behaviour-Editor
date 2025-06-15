using System;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    public struct GUIColorScope : IDisposable
    {
        public enum EGUIColorScope
        {
            Background,
            Content
        };
        
        
        public GUIColorScope(Color color, EGUIColorScope scopeType) : this()
        {
            this.colorScopeType = scopeType;
            this.originalColor = this.GetColor(scopeType);
            this.SetColor(scopeType, color);
        }
        
        
        public Color originalColor;
        
        public EGUIColorScope colorScopeType;



        private Color GetColor(EGUIColorScope scopeType)
        {
            switch (scopeType)
            {
                case EGUIColorScope.Background: return GUI.backgroundColor;
                
                case EGUIColorScope.Content: return GUI.contentColor;
            }
            
            Debug.LogError("Could not find scope type: " + scopeType);
            return Color.magenta;
        }

        
        private void SetColor(EGUIColorScope scopeType, Color color)
        {
            switch (scopeType)
            {
                case EGUIColorScope.Background: GUI.backgroundColor = color; break;

                case EGUIColorScope.Content: GUI.contentColor = color; break;
            }
        }
        

        public void Dispose()
        {
            this.SetColor(this.colorScopeType, this.originalColor);
        }
    }
}