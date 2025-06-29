#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

class QClear : EditorWindow
{
    [MenuItem("QTool/Clear/Bundle Caching Clear")]
    static void BundleClear()
    {
        Caching.ClearCache();
    }

    [MenuItem("QTool/Clear/Detect")]
    public static void Generate()
    {
        var targetPath = Path.Combine(Application.dataPath, "01.Scripts/Managers/LocalDataManager.cs");
        var scriptTexts = File.ReadAllLines(targetPath);

        var keys = new List<string>();
        foreach (var item in scriptTexts)
        {
            if (item.Contains("PlayerPrefs.Set") && !item.Contains("}\""))
            {
                var pattern = @"\""([^\""]*)\""";
                var match = Regex.Match(item, pattern);
                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    if (!string.IsNullOrEmpty(key) && !keys.Contains(key))
                        keys.Add(key);
                }
            }
        }

        var insertList = new List<string>();

        insertList.Add("    // Auto Generated");

        foreach (var key in keys)
        {
            insertList.Add($"    [MenuItem(\"QTool/Clear/{key}\", priority = 1)]");
            insertList.Add($"    public static void Clear{key}() {{ PlayerPrefs.DeleteKey(\"{key}\"); Debug.Log(\"<Color=red>PlayerPrefs Delete : {key}</color>\"); }}");
        }

        insertList.Add("    // End");

        var currentScript = AssetDatabase.FindAssets($"t:Script {nameof(QClear)}");

        var allLines = File.ReadAllLines(AssetDatabase.GUIDToAssetPath(currentScript[0])).ToList();

        var autoStart = allLines.FindIndex(f => f.Contains("// Auto Generated") && !f.Contains("Add"));
        var autoEnd = allLines.FindIndex(f => f.Contains("// End") && !f.Contains("Add"));

        if (autoStart >= 0)
            allLines.RemoveRange(autoStart, autoEnd - autoStart + 1);

        allLines.InsertRange(allLines.Count - 2, insertList);

        File.WriteAllLines(AssetDatabase.GUIDToAssetPath(currentScript[0]), allLines, System.Text.Encoding.UTF8);
        AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(currentScript[0]));
    }

    // Auto Generated
    [MenuItem("QTool/Clear/OwnedCharactorFlags", priority = 1)]
    public static void ClearOwnedCharactorFlags() { PlayerPrefs.DeleteKey("OwnedCharactorFlags"); Debug.Log("<Color=red>PlayerPrefs Delete : OwnedCharactorFlags</color>"); }
    [MenuItem("QTool/Clear/SelectedCharactor", priority = 1)]
    public static void ClearSelectedCharactor() { PlayerPrefs.DeleteKey("SelectedCharactor"); Debug.Log("<Color=red>PlayerPrefs Delete : SelectedCharactor</color>"); }
    [MenuItem("QTool/Clear/UseHaptic", priority = 1)]
    public static void ClearUseHaptic() { PlayerPrefs.DeleteKey("UseHaptic"); Debug.Log("<Color=red>PlayerPrefs Delete : UseHaptic</color>"); }
    [MenuItem("QTool/Clear/UseSound", priority = 1)]
    public static void ClearUseSound() { PlayerPrefs.DeleteKey("UseSound"); Debug.Log("<Color=red>PlayerPrefs Delete : UseSound</color>"); }
    [MenuItem("QTool/Clear/LanguageIndex", priority = 1)]
    public static void ClearLanguageIndex() { PlayerPrefs.DeleteKey("LanguageIndex"); Debug.Log("<Color=red>PlayerPrefs Delete : LanguageIndex</color>"); }
    [MenuItem("QTool/Clear/PlayerWinCount", priority = 1)]
    public static void ClearPlayerWinCount() { PlayerPrefs.DeleteKey("PlayerWinCount"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerWinCount</color>"); }
    [MenuItem("QTool/Clear/PlayerRankingPoint", priority = 1)]
    public static void ClearPlayerRankingPoint() { PlayerPrefs.DeleteKey("PlayerRankingPoint"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerRankingPoint</color>"); }
    [MenuItem("QTool/Clear/PlayerGemCount", priority = 1)]
    public static void ClearPlayerGemCount() { PlayerPrefs.DeleteKey("PlayerGemCount"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerGemCount</color>"); }
    [MenuItem("QTool/Clear/PlayerTicketCount", priority = 1)]
    public static void ClearPlayerTicketCount() { PlayerPrefs.DeleteKey("PlayerTicketCount"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerTicketCount</color>"); }
    [MenuItem("QTool/Clear/PlayerLampCount", priority = 1)]
    public static void ClearPlayerLampCount() { PlayerPrefs.DeleteKey("PlayerLampCount"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerLampCount</color>"); }
    [MenuItem("QTool/Clear/PlayerHammerCount", priority = 1)]
    public static void ClearPlayerHammerCount() { PlayerPrefs.DeleteKey("PlayerHammerCount"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerHammerCount</color>"); }
    [MenuItem("QTool/Clear/PlayerHolyShieldCount", priority = 1)]
    public static void ClearPlayerHolyShieldCount() { PlayerPrefs.DeleteKey("PlayerHolyShieldCount"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerHolyShieldCount</color>"); }
    [MenuItem("QTool/Clear/PlayerOverHeatCount", priority = 1)]
    public static void ClearPlayerOverHeatCount() { PlayerPrefs.DeleteKey("PlayerOverHeatCount"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerOverHeatCount</color>"); }
    [MenuItem("QTool/Clear/ChallengeModeLevel", priority = 1)]
    public static void ClearChallengeModeLevel() { PlayerPrefs.DeleteKey("ChallengeModeLevel"); Debug.Log("<Color=red>PlayerPrefs Delete : ChallengeModeLevel</color>"); }
    [MenuItem("QTool/Clear/PlayerTutorialStep", priority = 1)]
    public static void ClearPlayerTutorialStep() { PlayerPrefs.DeleteKey("PlayerTutorialStep"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerTutorialStep</color>"); }
    // End
}
#endif
