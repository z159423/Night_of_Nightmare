using UnityEngine;
using TMPro;

public class TextOutliner : MonoBehaviour
{
    [SerializeField] private Color underlayColor = Color.black;
    [SerializeField] private float underlayDilate = 0.2f;
    private TextMeshProUGUI tmp;
    private TextMeshPro textMeshPro;
    private Material instanceMaterial;

    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        textMeshPro = GetComponent<TextMeshPro>();

        // 텍스트마다 개별 머티리얼 생성
        if (tmp != null)
        {
            instanceMaterial = new Material(tmp.fontSharedMaterial);
            tmp.fontMaterial = instanceMaterial;
        }

        if (textMeshPro != null)
        {
            instanceMaterial = new Material(textMeshPro.fontSharedMaterial);
            textMeshPro.fontMaterial = instanceMaterial;
        }

        instanceMaterial.EnableKeyword("UNDERLAY_ON");

        // Underlay 효과 적용
        if (instanceMaterial.HasProperty("_UnderlayColor"))
            instanceMaterial.SetColor("_UnderlayColor", underlayColor);
        if (instanceMaterial.HasProperty("_UnderlayDilate"))
            instanceMaterial.SetFloat("_UnderlayDilate", underlayDilate);
    }

    public void SetUnderlayDilate(float dilate)
    {
        if (instanceMaterial != null && instanceMaterial.HasProperty("_UnderlayDilate"))
            instanceMaterial.SetFloat("_UnderlayDilate", dilate);
    }

    public void SetUnderlayColor(Color color)
    {
        if (instanceMaterial != null && instanceMaterial.HasProperty("_UnderlayColor"))
            instanceMaterial.SetColor("_UnderlayColor", color);
    }
}