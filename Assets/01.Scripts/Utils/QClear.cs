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

    [MenuItem("QTool/Clear/All Local Data", priority = 0)]
    public static void ClearAllLocalData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("<Color=red>PlayerPrefs DeleteAll : All local data cleared!</color>");
    }

    [MenuItem("QTool/Clear/Log All PlayerPrefs", priority = 0)]
    public static void LogAllPlayerPrefs()
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

        Debug.Log("<Color=cyan>===== All PlayerPrefs Values =====</color>");
        foreach (var key in keys)
        {
            if (PlayerPrefs.HasKey(key))
            {
                // 키 타입을 추측해서 적절한 값 읽기
                var value = GetPlayerPrefsValue(key);
                Debug.Log($"<Color=cyan>{key}</color> : {value}");
            }
            else
            {
                Debug.Log($"<Color=yellow>{key}</color> : <Color=gray>Not Set</color>");
            }
        }
        Debug.Log("<Color=cyan>===== End =====</color>");
    }

    private static string GetPlayerPrefsValue(string key)
    {
        // 키 이름으로 타입 추측
        if (key.Contains("Count") || key.Contains("Point") || key.Contains("Level") || key.Contains("Step") || key.Contains("Index"))
        {
            return PlayerPrefs.GetInt(key, 0).ToString();
        }
        else if (key.Contains("Use") || key.Contains("First"))
        {
            return PlayerPrefs.GetInt(key, 0).ToString() + " (bool)";
        }
        else if (key.Contains("Volume"))
        {
            return PlayerPrefs.GetFloat(key, 0f).ToString("F2");
        }
        else
        {
            // 기본적으로 string으로 시도, 없으면 int로 시도
            var stringValue = PlayerPrefs.GetString(key, "");
            if (!string.IsNullOrEmpty(stringValue))
            {
                return $"\"{stringValue}\"";
            }
            else
            {
                return PlayerPrefs.GetInt(key, 0).ToString();
            }
        }
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
            // Clear 메서드 추가
            insertList.Add($"    [MenuItem(\"QTool/Clear/{key}\", priority = 1)]");
            insertList.Add($"    public static void Clear{key}() {{ PlayerPrefs.DeleteKey(\"{key}\"); Debug.Log(\"<Color=red>PlayerPrefs Delete : {key}</color>\"); }}");
            
            // Log 메서드 추가
            insertList.Add($"    [MenuItem(\"QTool/Log/{key}\", priority = 1)]");
            insertList.Add($"    public static void Log{key}() {{ var value = GetPlayerPrefsValue(\"{key}\"); Debug.Log($\"<Color=cyan>{key}</color> : {{value}}\"); }}");
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
        
        Debug.Log($"<Color=green>Generated Clear and Log methods for {keys.Count} PlayerPrefs keys!</color>");
    }

    // Auto Generated
    [MenuItem("QTool/Clear/OwnedCharactorFlags", priority = 1)]
    public static void ClearOwnedCharactorFlags() { PlayerPrefs.DeleteKey("OwnedCharactorFlags"); Debug.Log("<Color=red>PlayerPrefs Delete : OwnedCharactorFlags</color>"); }
    [MenuItem("QTool/Log/OwnedCharactorFlags", priority = 1)]
    public static void LogOwnedCharactorFlags() { var value = GetPlayerPrefsValue("OwnedCharactorFlags"); Debug.Log($"<Color=cyan>OwnedCharactorFlags</color> : {value}"); }
    [MenuItem("QTool/Clear/SelectedCharactor", priority = 1)]
    public static void ClearSelectedCharactor() { PlayerPrefs.DeleteKey("SelectedCharactor"); Debug.Log("<Color=red>PlayerPrefs Delete : SelectedCharactor</color>"); }
    [MenuItem("QTool/Log/SelectedCharactor", priority = 1)]
    public static void LogSelectedCharactor() { var value = GetPlayerPrefsValue("SelectedCharactor"); Debug.Log($"<Color=cyan>SelectedCharactor</color> : {value}"); }
    [MenuItem("QTool/Clear/UseHaptic", priority = 1)]
    public static void ClearUseHaptic() { PlayerPrefs.DeleteKey("UseHaptic"); Debug.Log("<Color=red>PlayerPrefs Delete : UseHaptic</color>"); }
    [MenuItem("QTool/Log/UseHaptic", priority = 1)]
    public static void LogUseHaptic() { var value = GetPlayerPrefsValue("UseHaptic"); Debug.Log($"<Color=cyan>UseHaptic</color> : {value}"); }
    [MenuItem("QTool/Clear/UseSound", priority = 1)]
    public static void ClearUseSound() { PlayerPrefs.DeleteKey("UseSound"); Debug.Log("<Color=red>PlayerPrefs Delete : UseSound</color>"); }
    [MenuItem("QTool/Log/UseSound", priority = 1)]
    public static void LogUseSound() { var value = GetPlayerPrefsValue("UseSound"); Debug.Log($"<Color=cyan>UseSound</color> : {value}"); }
    [MenuItem("QTool/Clear/LanguageIndex", priority = 1)]
    public static void ClearLanguageIndex() { PlayerPrefs.DeleteKey("LanguageIndex"); Debug.Log("<Color=red>PlayerPrefs Delete : LanguageIndex</color>"); }
    [MenuItem("QTool/Log/LanguageIndex", priority = 1)]
    public static void LogLanguageIndex() { var value = GetPlayerPrefsValue("LanguageIndex"); Debug.Log($"<Color=cyan>LanguageIndex</color> : {value}"); }
    [MenuItem("QTool/Clear/PlayerWinCount", priority = 1)]
    public static void ClearPlayerWinCount() { PlayerPrefs.DeleteKey("PlayerWinCount"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerWinCount</color>"); }
    [MenuItem("QTool/Log/PlayerWinCount", priority = 1)]
    public static void LogPlayerWinCount() { var value = GetPlayerPrefsValue("PlayerWinCount"); Debug.Log($"<Color=cyan>PlayerWinCount</color> : {value}"); }
    [MenuItem("QTool/Clear/PlayerRankingPoint", priority = 1)]
    public static void ClearPlayerRankingPoint() { PlayerPrefs.DeleteKey("PlayerRankingPoint"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerRankingPoint</color>"); }
    [MenuItem("QTool/Log/PlayerRankingPoint", priority = 1)]
    public static void LogPlayerRankingPoint() { var value = GetPlayerPrefsValue("PlayerRankingPoint"); Debug.Log($"<Color=cyan>PlayerRankingPoint</color> : {value}"); }
    [MenuItem("QTool/Clear/PlayerHighestTier", priority = 1)]
    public static void ClearPlayerHighestTier() { PlayerPrefs.DeleteKey("PlayerHighestTier"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerHighestTier</color>"); }
    [MenuItem("QTool/Log/PlayerHighestTier", priority = 1)]
    public static void LogPlayerHighestTier() { var value = GetPlayerPrefsValue("PlayerHighestTier"); Debug.Log($"<Color=cyan>PlayerHighestTier</color> : {value}"); }
    [MenuItem("QTool/Clear/PlayerGemCount", priority = 1)]
    public static void ClearPlayerGemCount() { PlayerPrefs.DeleteKey("PlayerGemCount"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerGemCount</color>"); }
    [MenuItem("QTool/Log/PlayerGemCount", priority = 1)]
    public static void LogPlayerGemCount() { var value = GetPlayerPrefsValue("PlayerGemCount"); Debug.Log($"<Color=cyan>PlayerGemCount</color> : {value}"); }
    [MenuItem("QTool/Clear/PlayerRvTicketCount", priority = 1)]
    public static void ClearPlayerRvTicketCount() { PlayerPrefs.DeleteKey("PlayerRvTicketCount"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerRvTicketCount</color>"); }
    [MenuItem("QTool/Log/PlayerRvTicketCount", priority = 1)]
    public static void LogPlayerRvTicketCount() { var value = GetPlayerPrefsValue("PlayerRvTicketCount"); Debug.Log($"<Color=cyan>PlayerRvTicketCount</color> : {value}"); }
    [MenuItem("QTool/Clear/PlayerLampCount", priority = 1)]
    public static void ClearPlayerLampCount() { PlayerPrefs.DeleteKey("PlayerLampCount"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerLampCount</color>"); }
    [MenuItem("QTool/Log/PlayerLampCount", priority = 1)]
    public static void LogPlayerLampCount() { var value = GetPlayerPrefsValue("PlayerLampCount"); Debug.Log($"<Color=cyan>PlayerLampCount</color> : {value}"); }
    [MenuItem("QTool/Clear/PlayerHammerCount", priority = 1)]
    public static void ClearPlayerHammerCount() { PlayerPrefs.DeleteKey("PlayerHammerCount"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerHammerCount</color>"); }
    [MenuItem("QTool/Log/PlayerHammerCount", priority = 1)]
    public static void LogPlayerHammerCount() { var value = GetPlayerPrefsValue("PlayerHammerCount"); Debug.Log($"<Color=cyan>PlayerHammerCount</color> : {value}"); }
    [MenuItem("QTool/Clear/PlayerHolyShieldCount", priority = 1)]
    public static void ClearPlayerHolyShieldCount() { PlayerPrefs.DeleteKey("PlayerHolyShieldCount"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerHolyShieldCount</color>"); }
    [MenuItem("QTool/Log/PlayerHolyShieldCount", priority = 1)]
    public static void LogPlayerHolyShieldCount() { var value = GetPlayerPrefsValue("PlayerHolyShieldCount"); Debug.Log($"<Color=cyan>PlayerHolyShieldCount</color> : {value}"); }
    [MenuItem("QTool/Clear/PlayerOverHeatCount", priority = 1)]
    public static void ClearPlayerOverHeatCount() { PlayerPrefs.DeleteKey("PlayerOverHeatCount"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerOverHeatCount</color>"); }
    [MenuItem("QTool/Log/PlayerOverHeatCount", priority = 1)]
    public static void LogPlayerOverHeatCount() { var value = GetPlayerPrefsValue("PlayerOverHeatCount"); Debug.Log($"<Color=cyan>PlayerOverHeatCount</color> : {value}"); }
    [MenuItem("QTool/Clear/ChallengeModeLevel", priority = 1)]
    public static void ClearChallengeModeLevel() { PlayerPrefs.DeleteKey("ChallengeModeLevel"); Debug.Log("<Color=red>PlayerPrefs Delete : ChallengeModeLevel</color>"); }
    [MenuItem("QTool/Log/ChallengeModeLevel", priority = 1)]
    public static void LogChallengeModeLevel() { var value = GetPlayerPrefsValue("ChallengeModeLevel"); Debug.Log($"<Color=cyan>ChallengeModeLevel</color> : {value}"); }
    [MenuItem("QTool/Clear/PlayerTutorialStep", priority = 1)]
    public static void ClearPlayerTutorialStep() { PlayerPrefs.DeleteKey("PlayerTutorialStep"); Debug.Log("<Color=red>PlayerPrefs Delete : PlayerTutorialStep</color>"); }
    [MenuItem("QTool/Log/PlayerTutorialStep", priority = 1)]
    public static void LogPlayerTutorialStep() { var value = GetPlayerPrefsValue("PlayerTutorialStep"); Debug.Log($"<Color=cyan>PlayerTutorialStep</color> : {value}"); }
    [MenuItem("QTool/Clear/FirstUseLamp", priority = 1)]
    public static void ClearFirstUseLamp() { PlayerPrefs.DeleteKey("FirstUseLamp"); Debug.Log("<Color=red>PlayerPrefs Delete : FirstUseLamp</color>"); }
    [MenuItem("QTool/Log/FirstUseLamp", priority = 1)]
    public static void LogFirstUseLamp() { var value = GetPlayerPrefsValue("FirstUseLamp"); Debug.Log($"<Color=cyan>FirstUseLamp</color> : {value}"); }
    [MenuItem("QTool/Clear/CheatMode", priority = 1)]
    public static void ClearCheatMode() { PlayerPrefs.DeleteKey("CheatMode"); Debug.Log("<Color=red>PlayerPrefs Delete : CheatMode</color>"); }
    [MenuItem("QTool/Log/CheatMode", priority = 1)]
    public static void LogCheatMode() { var value = GetPlayerPrefsValue("CheatMode"); Debug.Log($"<Color=cyan>CheatMode</color> : {value}"); }
    [MenuItem("QTool/Clear/Volume", priority = 1)]
    public static void ClearVolume() { PlayerPrefs.DeleteKey("Volume"); Debug.Log("<Color=red>PlayerPrefs Delete : Volume</color>"); }
    [MenuItem("QTool/Log/Volume", priority = 1)]
    public static void LogVolume() { var value = GetPlayerPrefsValue("Volume"); Debug.Log($"<Color=cyan>Volume</color> : {value}"); }
    // End
}
#endif
