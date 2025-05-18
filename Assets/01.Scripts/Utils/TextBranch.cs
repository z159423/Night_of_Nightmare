#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.Google;
using UnityEditor.Localization.Reporting;

public class TextBranch
{
    static GoogleSheets GetSheet()
    {
        var provider = AssetDatabase.LoadAssetAtPath<SheetsServiceProvider>("Assets/8.Localize/Google Sheets Service.asset");
        var google = new GoogleSheets(provider);
        google.SpreadSheetId = "18pyeTydLxRmR36YZh5uvLH6_f7qhwbWzFQsUulgLXsA";
        return google;
    }

    static StringTableCollection GetStringTable()
    {
        return AssetDatabase.LoadAssetAtPath<StringTableCollection>("Assets/8.Localize/Table/TextScript.asset");
    }

    [MenuItem("TextBranch/main")]
    public static void ReleaseBranch()
    {
        var google = GetSheet();
        var target = GetStringTable().Extensions[0] as GoogleSheetsExtension;
        google.PullIntoStringTableCollection(0, target.TargetCollection as StringTableCollection, target.Columns, true, new ProgressReporter(), true);
    }
}
#endif