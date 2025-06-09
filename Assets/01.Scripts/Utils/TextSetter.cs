using UnityEngine;
using TMPro;

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

    private void start()
    {
        Text.text = Managers.Localize.GetText(KeyString);
    }
}
