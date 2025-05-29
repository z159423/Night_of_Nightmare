using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;

public class UI_GameScene_Home : UI_Scene
{

    enum Buttons
    {
        ShopBtn,
        CharacterBtn,
        HomeBtn,
        BoostBtn,
        QeustBtn,
        RankModeBtn,
        ChallengeModeBtn
    }

    enum Texts
    {
        GemText,
        TicketCount
    }

    bool _init = false;

    public LowerBtn selectedLowerBtn;

    public override void Init()
    {
        base.Init();
        // Managers.UI.SetCanvas(gameObject, true);

        if (!_init)
        {
            FirstSetting();
        }

        GetButton(Buttons.HomeBtn).GetComponent<LowerBtn>().Select();
        selectedLowerBtn = GetButton(Buttons.HomeBtn).GetComponent<LowerBtn>();
    }

    public void FirstSetting()
    {
        _init = true;

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));

        GetButton(Buttons.ShopBtn).GetComponent<LowerBtn>().Init();
        GetButton(Buttons.CharacterBtn).GetComponent<LowerBtn>().Init();
        GetButton(Buttons.HomeBtn).GetComponent<LowerBtn>().Init();
        GetButton(Buttons.BoostBtn).GetComponent<LowerBtn>().Init();
        GetButton(Buttons.QeustBtn).GetComponent<LowerBtn>().Init();

        GetButton(Buttons.ShopBtn).AddButtonEvent(() =>
        {
            var lowerbtn = GetButton(Buttons.ShopBtn).GetComponent<LowerBtn>();

            if (selectedLowerBtn == lowerbtn)
                return;

            lowerbtn.Select();

            selectedLowerBtn.UnSelect();
            selectedLowerBtn = lowerbtn;
        });

        GetButton(Buttons.CharacterBtn).onClick.AddListener(() =>
        {
            var lowerbtn = GetButton(Buttons.CharacterBtn).GetComponent<LowerBtn>();

            if (selectedLowerBtn == lowerbtn)
                return;

            lowerbtn.Select();

            selectedLowerBtn.UnSelect();
            selectedLowerBtn = lowerbtn;
        });

        GetButton(Buttons.HomeBtn).AddButtonEvent(() =>
        {
            var lowerbtn = GetButton(Buttons.HomeBtn).GetComponent<LowerBtn>();

            if (selectedLowerBtn == lowerbtn)
                return;

            lowerbtn.Select();

            selectedLowerBtn.UnSelect();
            selectedLowerBtn = lowerbtn;
        });

        GetButton(Buttons.BoostBtn).AddButtonEvent(() =>
        {
            var lowerbtn = GetButton(Buttons.BoostBtn).GetComponent<LowerBtn>();

            if (selectedLowerBtn == lowerbtn)
                return;

            lowerbtn.Select();

            selectedLowerBtn.UnSelect();
            selectedLowerBtn = lowerbtn;
        });

        GetButton(Buttons.QeustBtn).AddButtonEvent(() =>
        {

        });

        GetButton(Buttons.RankModeBtn).onClick.AddListener(() =>
        {
            Managers.UI.ShowPopupUI<Match_Making_Popup>();
        });
    }

    public override void Show()
    {
        gameObject.SetActive(true);
    }

    public override void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SelectLowerBtn()
    {

    }
}
