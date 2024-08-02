using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


public static class EditorUtility
{
    public static string GetAssetFolderPath(string searchFilter, string folderPath = "")
    {
        foreach (var guid in AssetDatabase.FindAssets(searchFilter) ?? Enumerable.Empty<string>())
        {
            string parentPath = AssetDatabase.GUIDToAssetPath(guid);
            string resultPath = $"{parentPath}{folderPath}";

            if (Directory.Exists(resultPath))
            {
                return resultPath;
            }
        }

        return string.Empty;
    }


    public static T FindAsset<T>(string searchFilter, string assetPath) where T : Object
    {
        string[] paths = assetPath.Split('/', '\\');
        string folderPath = Path.Combine("/", string.Join("/", paths[0..(paths.Length - 1)]));
        string combinedPath = Path.Combine(GetAssetFolderPath(searchFilter, folderPath), paths.Last());

        T findAsset = AssetDatabase.LoadAssetAtPath<T>(combinedPath);

        if (findAsset == null)
        {
            throw new FileNotFoundException($"Asset not found at path: {combinedPath}");
        }

        return findAsset;
    }


    public static T FindAssetByName<T>(string searchFilter) where T : Object
    {
        foreach (var guid in AssetDatabase.FindAssets(searchFilter) ?? Enumerable.Empty<string>())
        {
            string parentPath = AssetDatabase.GUIDToAssetPath(guid);

            if (File.Exists(parentPath))
            {
                return AssetDatabase.LoadAssetAtPath<T>(parentPath);
            }
        }

        throw new FileNotFoundException($"Asset not found at filter: {searchFilter}");
    }
}
