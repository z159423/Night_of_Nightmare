using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class TextOutliner : MonoBehaviour
{
    [SerializeField] private Color outlineColor = Color.red;
    [SerializeField] private float outlineWidth = 0.2f;
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


        // 아웃라인 색상 설정
        instanceMaterial.SetColor("_OutlineColor", outlineColor);
        instanceMaterial.SetFloat("_OutlineWidth", outlineWidth);
    }

    public void SetOutlineColor(Color color)
    {
        instanceMaterial.SetColor("_OutlineColor", color);
    }
}