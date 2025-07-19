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
                    string gameObjectPath = GetFullPath(comp.gameObject, prefab);
                    Debug.Log($"✅ Found '{scriptName}' on GameObject '{gameObjectPath}' in Prefab: {path}", prefab);
                    break;
                }
            }
        }
    }

    // 프리팹 내에서의 상대 GameObject 경로 계산
    private static string GetFullPath(GameObject obj, GameObject root)
    {
        string path = obj.name;
        Transform current = obj.transform;

        while (current.parent != null && current.parent != root.transform)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }

        return path;
    }
}
