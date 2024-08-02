using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

namespace BehaviourTechnique.UIElements
{
    public class SplitView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits>
        {
        }
    }
}