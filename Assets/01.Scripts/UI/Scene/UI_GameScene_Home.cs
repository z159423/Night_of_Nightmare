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

    public enum LowerBtnTypes
    {
        ShopBtn,
        CharacterBtn,
        HomeBtn,
        BoostBtn,
        QeustBtn
    }

    bool _init = false;

    public LowerBtn selectedLowerBtn;
    public LowerBtnTypes selectedLowerBtnType;

    private IapShop_Popup iapShopPopup;
    private BoostShop_Popup boostShopPopup;


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

        this.SetListener(GameObserverType.Game.OnChangeHomeLowerBtn, () =>
        {
            if (iapShopPopup != null && selectedLowerBtnType != LowerBtnTypes.ShopBtn)
            {
                iapShopPopup.ClosePopupUI();
                iapShopPopup = null;
            }

            if (boostShopPopup != null && selectedLowerBtnType != LowerBtnTypes.BoostBtn)
            {
                boostShopPopup.ClosePopupUI();
                boostShopPopup = null;
            }
        });

        GetTextMesh(Texts.GemText).text = Managers.Game.gem.ToString();
        GetTextMesh(Texts.TicketCount).text = Managers.Game.ticket.ToString();
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

            iapShopPopup = Managers.UI.ShowPopupUI<IapShop_Popup>();
            selectedLowerBtnType = LowerBtnTypes.ShopBtn;

            GameObserver.Call(GameObserverType.Game.OnChangeHomeLowerBtn);
        });

        GetButton(Buttons.CharacterBtn).onClick.AddListener(() =>
        {
            var lowerbtn = GetButton(Buttons.CharacterBtn).GetComponent<LowerBtn>();

            if (selectedLowerBtn == lowerbtn)
                return;

            lowerbtn.Select();

            selectedLowerBtn.UnSelect();
            selectedLowerBtn = lowerbtn;

            selectedLowerBtnType = LowerBtnTypes.CharacterBtn;

            GameObserver.Call(GameObserverType.Game.OnChangeHomeLowerBtn);

        });

        GetButton(Buttons.HomeBtn).AddButtonEvent(() =>
        {
            var lowerbtn = GetButton(Buttons.HomeBtn).GetComponent<LowerBtn>();

            if (selectedLowerBtn == lowerbtn)
                return;

            lowerbtn.Select();

            selectedLowerBtn.UnSelect();
            selectedLowerBtn = lowerbtn;

            selectedLowerBtnType = LowerBtnTypes.HomeBtn;

            GameObserver.Call(GameObserverType.Game.OnChangeHomeLowerBtn);

        });

        GetButton(Buttons.BoostBtn).AddButtonEvent(() =>
        {
            var lowerbtn = GetButton(Buttons.BoostBtn).GetComponent<LowerBtn>();

            if (selectedLowerBtn == lowerbtn)
                return;

            lowerbtn.Select();

            selectedLowerBtn.UnSelect();
            selectedLowerBtn = lowerbtn;

            selectedLowerBtnType = LowerBtnTypes.BoostBtn;

            GameObserver.Call(GameObserverType.Game.OnChangeHomeLowerBtn);

            boostShopPopup = Managers.UI.ShowPopupUI<BoostShop_Popup>();
        });

        GetButton(Buttons.QeustBtn).AddButtonEvent(() =>
        {

        });

        GetButton(Buttons.RankModeBtn).onClick.AddListener(() =>
        {
            var popup = Managers.UI.ShowPopupUI<Match_Making_Popup>();

            
        });

        GetButton(Buttons.ChallengeModeBtn).onClick.AddListener(() =>
        {
            Managers.UI.ShowPopupUI<ChallengeMode_Popup>();
        });

        this.SetListener(GameObserverType.Game.OnChangeGemCount, () =>
        {
            GetTextMesh(Texts.GemText).text = Managers.Game.gem.ToString();
        });

        this.SetListener(GameObserverType.Game.OnChangeTicketCount, () =>
        {
            GetTextMesh(Texts.TicketCount).text = Managers.Game.ticket.ToString();
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
