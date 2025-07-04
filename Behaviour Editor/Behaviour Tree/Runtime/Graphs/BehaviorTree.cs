using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BehaviourSystem.BT
{
    public class BehaviorTree : Graph
    {
        public TreeInterruptor interrupter
        {
            get;
            internal set;
        }
        


        public override EStatus UpdateGraph()
        {
            if (entry is BehaviorNodeBase behaviourNode)
            {
                return behaviourNode.UpdateNode();
            }
            else
            {
                return EStatus.Failure;
            }
        }


        public override void ResetGraph()
        {
            interrupter.ClearCallStack();
        }


        public override void StopGraph()
        {
            interrupter.AbortSubtree(entry.callStackID);
        }


#if UNITY_EDITOR
        public void AddChild(BehaviorNodeBase parent, BehaviorNodeBase child)
        {
            Undo.RecordObject(parent, "Behaviour Tree (AddChild)");

            switch (parent.nodeType)
            {
                case BehaviorNodeBase.EBehaviourNodeType.Root:
                {
                    ((RootNode)parent).child = child;
                    child.parent = parent;
                    break;
                }

                case BehaviorNodeBase.EBehaviourNodeType.Decorator:
                {
                    ((DecoratorNode)parent).child = child;
                    child.parent = parent;
                    break;
                }

                case BehaviorNodeBase.EBehaviourNodeType.Composite:
                {
                    ((CompositeNode)parent).children.Add(child);
                    child.parent = parent;
                    break;
                }
            }

            EditorUtility.SetDirty(parent);
            EditorUtility.SetDirty(child);
        }


        public void RemoveChild(BehaviorNodeBase parent, BehaviorNodeBase child)
        {
            Undo.RecordObject(parent, "Behaviour Tree (RemoveChild)");

            switch (parent.nodeType)
            {
                case BehaviorNodeBase.EBehaviourNodeType.Root:
                {
                    ((RootNode)parent).child = null;
                    child.parent = null;
                    break;
                }

                case BehaviorNodeBase.EBehaviourNodeType.Decorator:
                {
                    ((DecoratorNode)parent).child = null;
                    child.parent = null;
                    break;
                }

                case BehaviorNodeBase.EBehaviourNodeType.Composite:
                {
                    ((CompositeNode)parent).children.Remove(child);
                    child.parent = null;
                    break;
                }
            }

            EditorUtility.SetDirty(child);
        }
#endif
    }
}