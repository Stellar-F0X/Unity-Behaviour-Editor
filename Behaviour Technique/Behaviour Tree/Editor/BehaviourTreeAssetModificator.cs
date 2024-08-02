using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public class BehaviourTreeAssetModificator : AssetModificationProcessor
    {
        private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            if (EditorWindow.HasOpenInstances<BehaviourTreeEditorWindow>())
            {
                BehaviourTreeEditorWindow wnd = EditorWindow.GetWindow<BehaviourTreeEditorWindow>();
                DeleteNodeEvent(wnd.Tree.nodeList, wnd.Actor);
                wnd.View.ClearEditorViwer();
            }
            else if (BehaviourTreeEditorWindow.Instance != null && BehaviourTreeEditorWindow.Instance.Actor != null)
            {
                BehaviourTree behaviourTree = AssetDatabase.LoadAssetAtPath<BehaviourTree>(assetPath);

                if (behaviourTree.nodeList != null)
                {
                    DeleteNodeEvent(behaviourTree.nodeList, BehaviourTreeEditorWindow.Instance.Actor);
                }
            }

            return AssetDeleteResult.DidNotDelete;
        }


        private static void DeleteNodeEvent(List<Node> nodes, BehaviourActor actor)
        {
            for (int i = 0; i < nodes.Count; ++i)
            {
                Type nodeType = nodes[i].GetType();

                foreach (FieldInfo fieldInfo in nodeType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (fieldInfo.FieldType == typeof(BehaviourTreeEvent) && fieldInfo.GetValue(nodes[i]) is BehaviourTreeEvent eventValue)
                    {
                        //Debug.Log($"노드에 등록된 이벤트 삭제 : {eventValue.key}");
                        actor?.RemoveBehaviourEvent(eventValue.key);
                    }
                }
            }
        }
    }
}
