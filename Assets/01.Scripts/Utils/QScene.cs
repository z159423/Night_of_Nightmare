#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class QScene : EditorWindow
{
    [MenuItem("QTool/Scene/Detect")]
    public static void Generate()
    {
        var count = SceneManager.sceneCountInBuildSettings;
        string[] scenes = new string[count];
        for (int i = 0; i < count; i++)
            scenes[i] = System.IO.Path.GetRelativePath("Assets/", SceneUtility.GetScenePathByBuildIndex(i));

        var insertList = new List<string>();

        insertList.Add("    // Auto Generated");

        foreach (var scene in scenes)
        {
            insertList.Add($"    [MenuItem(\"QTool/Scene/{System.IO.Path.GetFileNameWithoutExtension(scene)}\", priority = 1)]");
            insertList.Add($"    public static void To{System.IO.Path.GetFileNameWithoutExtension(scene)}() => QScene.OpenScene(\"Assets/{scene}\");");
        }

        insertList.Add("    // End");

        var currentScript = AssetDatabase.FindAssets($"t:Script {nameof(QScene)}");

        var allLines = File.ReadAllLines(AssetDatabase.GUIDToAssetPath(currentScript[0])).ToList();

        var autoStart = allLines.FindIndex(f => f.Contains("// Auto Generated") && !f.Contains("Add"));
        var autoEnd = allLines.FindIndex(f => f.Contains("// End") && !f.Contains("Add"));

        if (autoStart >= 0)
            allLines.RemoveRange(autoStart, autoEnd - autoStart + 1);

        allLines.InsertRange(allLines.Count - 2, insertList);

        File.WriteAllLines(AssetDatabase.GUIDToAssetPath(currentScript[0]), allLines, System.Text.Encoding.UTF8);
        AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(currentScript[0]));
    }
    public static void OpenScene(string path)
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(path);
    }
    // Auto Generated
    [MenuItem("QTool/Scene/SplashScene", priority = 1)]
    public static void ToSplashScene() => QScene.OpenScene("Assets/00.Scenes/SplashScene.unity");
    [MenuItem("QTool/Scene/Loading", priority = 1)]
    public static void ToLoading() => QScene.OpenScene("Assets/00.Scenes/Loading.unity");
    [MenuItem("QTool/Scene/Game", priority = 1)]
    public static void ToGame() => QScene.OpenScene("Assets/00.Scenes/Game.unity");
    // End
}
#endif
