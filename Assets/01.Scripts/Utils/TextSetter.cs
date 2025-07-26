using UnityEngine;
using TMPro;
using System.Threading.Tasks;

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

    private async void Start()
    {
        await SetText();

        this.SetListener(GameObserverType.Game.OnChangeLocolization, async () =>
        {
            await SetText();
        });
    }

    private async Task SetText()
    {
        string localizedText = await Managers.Localize.GetTextAsync(KeyString);
        Text.text = localizedText;
    }

    void OnDestroy()
    {
        this.RemoveListener(GameObserverType.Game.OnChangeLocolization);
    }
}
