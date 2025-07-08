using UnityEditor;
using UnityEngine;
using TMPro;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
public class FontFixerUtility
{
    [MenuItem("Assets/fontSetter")]
    public static void ApplyFontFixerToPrefabs()
    {
        // 선택한 경로 가져오기
        string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError("선택된 경로가 폴더가 아닙니다.");
            return;
        }

        // 폴더 내 모든 프리팹 경로 찾기
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        int modifiedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            bool isModified = false;

            // 프리팹 내 모든 TMP 검사
            var tmpComponents = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in tmpComponents)
            {
                if (tmp.GetComponent<FontFixer>() != null) continue;         // 이미 있음
                if (tmp.GetComponent<TextOutliner>() != null) continue;     // 제외 대상

                Undo.RecordObject(tmp.gameObject, "Add FontFixer");
                tmp.gameObject.AddComponent<FontFixer>();
                isModified = true;
            }

            if (isModified)
            {
                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
                modifiedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[FontFixerUtility] 처리 완료: {modifiedCount}개 프리팹 수정됨.");
    }
}
#endif