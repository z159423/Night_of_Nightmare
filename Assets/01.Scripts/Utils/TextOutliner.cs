using UnityEngine;
using TMPro;

[ExecuteAlways]
public class TextOutliner : MonoBehaviour
{
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField] private float outlineThickness = 0.2f;
    [SerializeField] private float faceDilate = 0.2f;
    private TextMeshProUGUI tmp;
    private TextMeshPro textMeshPro;
    private Material instanceMaterial;

    private const float offset = 0.8f;

    void Awake()
    {
        ApplyOutline();
    }

    void Start()
    {
        ApplyOutline();
    }

    void OnValidate()
    {
        ApplyOutline();
    }

    private void ApplyOutline()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        textMeshPro = GetComponent<TextMeshPro>();

        // 텍스트마다 개별 머티리얼 생성
        if (tmp != null && tmp.fontSharedMaterial != null)
        {
            if (instanceMaterial == null || tmp.fontMaterial != instanceMaterial)
            {
                instanceMaterial = new Material(tmp.fontSharedMaterial);
                tmp.fontMaterial = instanceMaterial;
            }
        }

        if (textMeshPro != null && textMeshPro.fontSharedMaterial != null)
        {
            if (instanceMaterial == null || textMeshPro.fontMaterial != instanceMaterial)
            {
                instanceMaterial = new Material(textMeshPro.fontSharedMaterial);
                textMeshPro.fontMaterial = instanceMaterial;
            }
        }

        // Outline 효과 적용
        if (instanceMaterial != null)
        {
            if (instanceMaterial.HasProperty("_OutlineColor"))
                instanceMaterial.SetColor("_OutlineColor", outlineColor);
            if (instanceMaterial.HasProperty("_OutlineWidth"))
                instanceMaterial.SetFloat("_OutlineWidth", outlineThickness * offset);
            if (instanceMaterial.HasProperty("_FaceDilate"))
                instanceMaterial.SetFloat("_FaceDilate", faceDilate * offset);
        }
    }

    public void SetOutlineColor(Color color)
    {
        outlineColor = color;
        ApplyOutline();
    }

    public void SetOutlineThickness(float thickness)
    {
        outlineThickness = thickness;
        ApplyOutline();
    }

    public void SetFaceDilate(float dilate)
    {
        faceDilate = dilate;
        ApplyOutline();
    }
}