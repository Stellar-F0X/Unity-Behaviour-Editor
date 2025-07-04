using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace BehaviourSystemEditor.BT
{
    public static class EditorHelper
    {
        public static T FindAssetByName<T>(string searchFilter) where T : Object
        {
            string[] guids = AssetDatabase.FindAssets(searchFilter);

            if (guids is null || guids.Length == 0)
            {
                return null;
            }

            foreach (var guid in guids)
            {
                string parentPath = AssetDatabase.GUIDToAssetPath(guid);

                if (File.Exists(parentPath))
                {
                    return AssetDatabase.LoadAssetAtPath<T>(parentPath);
                }
            }

            throw new FileNotFoundException($"Asset not found at filter: {searchFilter}");
        }


        public static string FindAssetPath(string searchFilter)
        {
            string[] guids = AssetDatabase.FindAssets(searchFilter);

            if (guids != null && guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);

                if (File.Exists(path))
                {
                    return path;
                }
            }

            throw new FileNotFoundException($"Asset not found at filter: {searchFilter}");
        }


        public static void ForEach<T>([NotNull] this IEnumerable<T> array, [NotNull] Action<T> action)
        {
            foreach (var element in array)
            {
                action.Invoke(element);
            }
        }


        public static bool EditorErrorAssert(Rect rect, bool condition, string message)
        {
            if (condition)
            {
                GUIContent warningIcon = EditorGUIUtility.IconContent("console.warnicon");
                EditorGUI.LabelField(rect, new GUIContent(message, warningIcon.image));
                return false;
            }

            return true;
        }


        public static Type[] OrderByNameAndFilterAbstracts(this TypeCache.TypeCollection collection)
        {
            Type[] array = collection.Where(t => t.IsAbstract == false).ToArray();

            if (array.Length <= 1)
            {
                return array;
            }

            Array.Sort(array, (a, b) => a.Name[0].CompareTo(b.Name[0]));
            return array;
        }


        public static void RegisterRemovableCallback<T>(this VisualElement element, EventCallback<T> callback) where T : EventBase<T>, new()
        {
            if (element.userData is null)
            {
                element.userData = new Dictionary<Type, List<Delegate>>();
            }

            if (element.userData is Dictionary<Type, List<Delegate>> callbackList)
            {
                Type key = typeof(T);

                if (callbackList.ContainsKey(key))
                {
                    callbackList[key].Add(callback);
                }
                else
                {
                    callbackList.Add(key, new List<Delegate>() { callback });
                }

                element.RegisterCallback(callback);
            }
            else
            {
                Debug.LogWarning("User Data is already in use.");
            }
        }


        public static void RemoveAllCallbacksOfType<T>(this VisualElement element) where T : EventBase<T>, new()
        {
            if (element.userData is Dictionary<Type, List<Delegate>> dictionary)
            {
                if (dictionary.Count == 0)
                {
                    return;
                }

                foreach (var list in dictionary.Values)
                {
                    if (list == null || list.Count == 0 && list[0] is not EventCallback<T>)
                    {
                        continue;
                    }

                    foreach (var callback in list)
                    {
                        if (callback is EventCallback<T> convertedCallback)
                        {
                            element.UnregisterCallback(convertedCallback);
                        }
                    }

                    list.Clear();
                }
            }
        }
    }
}