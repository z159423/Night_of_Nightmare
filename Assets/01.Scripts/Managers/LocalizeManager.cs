using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Text.RegularExpressions;

/// <summary>
/// 이 클래스는 현지화를 도와주는 기능을 포함하고 있습니다.
/// </summary>
public class LocalizeManager : MonoBehaviour
{
    public Regex _regex;

    private void Start()
    {
        _regex = new Regex(@"[\u0600-\u06FF]+");
    }

    /// <summary>
    /// 테이블 이름과, 키값을 통해 텍스트를 현지화된 텍스트로 변경합니다.
    /// </summary>
    public void CallLocalizedString(TextMeshProUGUI text, string tableRef, string key)
    {
        CoroutineHelper.StartCoroutine(LoadString());

        IEnumerator LoadString()
        {

            var localizedString = new LocalizedString(tableRef, key);

            var stringOperation = localizedString.GetLocalizedStringAsync();

            while (true)
            {
                if (stringOperation.IsDone && stringOperation.Status == AsyncOperationStatus.Succeeded)
                {
                    string str = stringOperation.Result;
                    text.text = str;

                    break;
                }
                yield return null;
            }
        }
    }

    public string GetText(string type)
    {
        LocalizedString localizeString = new LocalizedString() { TableReference = "TextScript", TableEntryReference = type };
        var stringOperation = localizeString.GetLocalizedStringAsync();

        if (stringOperation.IsDone && stringOperation.Status == AsyncOperationStatus.Succeeded)
        {
            return stringOperation.Result;
        }
        else
        {
            return null;
        }
    }

    public string GetDynamicText(string type, params string[] contents)
    {
        var str = GetText(type);
        for (int i = 1; i <= contents.Length; i++)
        {
            str = str.Replace($"@{i}", contents[i - 1]);
        }
        return str;
    }
}
