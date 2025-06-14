using UnityEditor;
using UnityEngine;

public class ScriptUsageFinder
{
    [MenuItem("Tools/Find Prefabs Using Script")]
    public static void FindPrefabsWithScript()
    {
        string scriptName = "TextOutliner"; // 찾고자 하는 스크립트 이름

        string[] allPrefabGUIDs = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in allPrefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
                continue;

            Component[] components = prefab.GetComponentsInChildren<Component>(true);

            foreach (Component comp in components)
            {
                if (comp == null)
                    continue;

                if (comp.GetType().Name == scriptName)
                {
                    Debug.Log($"Found in: {path}", prefab);
                    break;
                }
            }
        }
    }
}
