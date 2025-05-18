using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;

public class TextSetter : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public string KeyString;
    public bool IsPassRTL = false;

    private void OnValidate()
    {
        if (Text == null)
            Text = GetComponent<TextMeshProUGUI>();
    }

    private void Awake()
    {
        Text.text = Managers.Localize.GetText(KeyString);
    }
}
