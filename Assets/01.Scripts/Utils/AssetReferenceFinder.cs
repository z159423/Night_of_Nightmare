#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.IO;

public class AssetReferenceFinder
{
    [MenuItem("Tools/Find References To Selected Asset")]
    public static void FindReferences()
    {
        Object selected = Selection.activeObject;

        if (selected == null)
        {
            Debug.LogWarning("에셋을 먼저 선택해주세요.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(selected);
        string guid = AssetDatabase.AssetPathToGUID(path);

        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

        foreach (string assetPath in allAssetPaths)
        {
            // 메타, 라이브러리 등은 제외
            if (!assetPath.StartsWith("Assets/") || assetPath == path)
                continue;

            string content = File.ReadAllText(assetPath);
            if (content.Contains(guid))
            {
                Debug.Log($"🔗 {assetPath} 에서 참조됨", AssetDatabase.LoadAssetAtPath<Object>(assetPath));
            }
        }
    }
}

#endif