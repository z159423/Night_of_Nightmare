using UnityEngine;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TextMeshProUGUI))]
public class FontFixer : MonoBehaviour
{
    void Awake()
    {
        ApplyFontFix();
    }

    void OnEnable()
    {
        ApplyFontFix();
    }

    private void ApplyFontFix()
    {
        var tmp = GetComponent<TextMeshProUGUI>();
        if (tmp == null || tmp.fontSharedMaterial == null)
            return;

        // 인스턴스 머티리얼 적용
        var instanceMat = new Material(tmp.fontSharedMaterial);
        tmp.fontMaterial = instanceMat;
    }
}
