using UnityEngine;
using TMPro;

public class TextOutliner : MonoBehaviour
{
    [SerializeField] private Color outlineColor = Color.red;
    [SerializeField] private float outlineWidth = 0.2f;
    private TextMeshProUGUI tmp;
    private Material instanceMaterial;

    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        
        // 텍스트마다 개별 머티리얼 생성
        instanceMaterial = new Material(tmp.fontSharedMaterial);
        tmp.fontMaterial = instanceMaterial;
        
        // 아웃라인 색상 설정
        instanceMaterial.SetColor("_OutlineColor", outlineColor);
        instanceMaterial.SetFloat("_OutlineWidth", outlineWidth);
    }

    public void SetOutlineColor(Color color)
    {
        instanceMaterial.SetColor("_OutlineColor", color);
    }
}