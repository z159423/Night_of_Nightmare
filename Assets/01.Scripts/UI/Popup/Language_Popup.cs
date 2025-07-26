using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Language_Popup : UI_Popup
{
    enum Buttons
    {
        BG,
    }

    enum Images
    {
        TouchGuard
    }

    private Transform layout;
    [SerializeField] private Sprite[] languageBtnSprites;
    public override void Init()
    {
        base.Init();

        OpenAnimation(() =>
        {
            GetImage(Images.TouchGuard).gameObject.SetActive(false);
        });

        UpdateUI();
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));

        layout = gameObject.FindRecursive("Content").transform;

        GetButton(Buttons.BG).AddButtonEvent(() =>
        {
            GetImage(Images.TouchGuard).gameObject.SetActive(true);
            ClosePop(gameObject.FindRecursive("Panel").transform);
        }, false);
    }

    void UpdateUI()
    {
        foreach (Transform child in layout)
        {
            Destroy(child.gameObject);
        }

        var availableLanguages = UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.Locales;

        foreach (var locale in availableLanguages)
        {
            // 언어별 UI 생성 코드
            GameObject languageItem = Instantiate(Managers.Resource.Load<GameObject>("LanguageBtn"), layout);
            var button = languageItem.GetComponent<Button>();
            var text = languageItem.GetComponentInChildren<TextMeshProUGUI>();

            text.text = locale.Identifier.CultureInfo.NativeName;

            button.onClick.AddListener(() =>
            {
                // 언어 변경
                UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale = locale;
                UpdateUI();

                GameObserver.Call(GameObserverType.Game.OnChangeLocolization);
            });

            if (locale == UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale)
                // 현재 선택된 언어에 대한 표시
                button.GetComponent<Image>().sprite = languageBtnSprites[0];
            else
                button.GetComponent<Image>().sprite = languageBtnSprites[1];
        }
    }
}
