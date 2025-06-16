using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
using DG.Tweening;
using Unity.VisualScripting;

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
        ChallengeModeBtn,
        CenterCharactorBtn
    }

    enum Texts
    {
        GemText,
        TicketCount
    }

    enum Images
    {
        TouchGuard
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

    private GameObject lowerMenu;
    public LowerBtn selectedLowerBtn;
    public LowerBtnTypes selectedLowerBtnType = LowerBtnTypes.HomeBtn;

    private IapShop_Popup iapShopPopup;
    private BoostShop_Popup boostShopPopup;

    private UI_Popup currentPopup;


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
        Bind<Image>(typeof(Images));

        lowerMenu = gameObject.FindRecursive("LowerMenu");

        GetButton(Buttons.ShopBtn).GetComponent<LowerBtn>().Init();
        GetButton(Buttons.CharacterBtn).GetComponent<LowerBtn>().Init();
        GetButton(Buttons.HomeBtn).GetComponent<LowerBtn>().Init();
        GetButton(Buttons.BoostBtn).GetComponent<LowerBtn>().Init();
        GetButton(Buttons.QeustBtn).GetComponent<LowerBtn>().Init();

        GetButton(Buttons.ShopBtn).AddButtonEvent(() =>
        {
            var lowerbtn = GetButton(Buttons.ShopBtn).GetComponent<LowerBtn>();

            SelectLowerBtn(LowerBtnTypes.ShopBtn, lowerbtn);
        });

        GetButton(Buttons.CharacterBtn).onClick.AddListener(() =>
        {
            var lowerbtn = GetButton(Buttons.CharacterBtn).GetComponent<LowerBtn>();

            SelectLowerBtn(LowerBtnTypes.CharacterBtn, lowerbtn);
        });

        GetButton(Buttons.HomeBtn).AddButtonEvent(() =>
        {
            var lowerbtn = GetButton(Buttons.HomeBtn).GetComponent<LowerBtn>();

            SelectLowerBtn(LowerBtnTypes.HomeBtn, lowerbtn);

            GameObserver.Call(GameObserverType.Game.OnChangeHomeLowerBtn);

        });

        GetButton(Buttons.BoostBtn).AddButtonEvent(() =>
        {
            var lowerbtn = GetButton(Buttons.BoostBtn).GetComponent<LowerBtn>();

            SelectLowerBtn(LowerBtnTypes.BoostBtn, lowerbtn);
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

        GetButton(Buttons.CenterCharactorBtn).onClick.AddListener(() =>
        {
            var lowerbtn = GetButton(Buttons.CharacterBtn).GetComponent<LowerBtn>();

            SelectLowerBtn(LowerBtnTypes.CharacterBtn, lowerbtn);
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

    public void SelectLowerBtn(LowerBtnTypes type, LowerBtn btn)
    {
        if (selectedLowerBtn == btn)
            return;

        btn.Select();

        selectedLowerBtn.UnSelect();
        selectedLowerBtn = btn;

        GetImage(Images.TouchGuard).gameObject.SetActive(true);

        if (currentPopup != null)
        {
            float moveValue = selectedLowerBtnType == LowerBtnTypes.BoostBtn ? 1500f : -1500f;

            currentPopup.transform.DOLocalMoveX(moveValue, 0.5f).SetEase(Ease.InCubic).OnComplete(() =>
            {
                currentPopup.ClosePopupUI();
                currentPopup = null;

                if (type != LowerBtnTypes.CharacterBtn)
                    SetLowerBtn();

            });

            if (type == LowerBtnTypes.CharacterBtn)
                SetLowerBtn();
        }
        else
        {
            SetLowerBtn();
        }

        void SetLowerBtn()
        {
            if (type == LowerBtnTypes.HomeBtn)
            {
                if (selectedLowerBtnType == LowerBtnTypes.CharacterBtn)
                {
                    GetButton(Buttons.RankModeBtn).transform.DOLocalMoveX(0f, 0).SetEase(Ease.InCubic).OnStart(() => GetButton(Buttons.RankModeBtn).transform.localScale = Vector3.zero).OnComplete(() =>
                    {
                        GetButton(Buttons.RankModeBtn).transform.DOScale(1, 0.3f);
                    });
                    GetButton(Buttons.ChallengeModeBtn).transform.DOLocalMoveX(0f, 0).SetEase(Ease.InCubic).OnStart(() => GetButton(Buttons.ChallengeModeBtn).transform.localScale = Vector3.zero).OnComplete(() =>
                    {
                        GetButton(Buttons.ChallengeModeBtn).transform.DOScale(1, 0.3f);
                    });

                    lowerMenu.transform.DOLocalMoveY(400, 0.5f).SetRelative().OnComplete(() =>
                    {
                        GetImage(Images.TouchGuard).gameObject.SetActive(false);
                    });

                    Managers.Camera.ChangeCameraLensOrthoSize(7, 0.5f);
                }
                else if (selectedLowerBtnType == LowerBtnTypes.BoostBtn ||
                         selectedLowerBtnType == LowerBtnTypes.ShopBtn)
                {
                    GetButton(Buttons.RankModeBtn).transform.DOLocalMoveX(0f, 0).SetEase(Ease.InCubic).OnStart(() => GetButton(Buttons.RankModeBtn).transform.localScale = Vector3.zero).OnComplete(() =>
                    {
                        GetButton(Buttons.RankModeBtn).transform.DOScale(1, 0.3f);
                    });
                    GetButton(Buttons.ChallengeModeBtn).transform.DOLocalMoveX(0f, 0).SetEase(Ease.InCubic).OnStart(() => GetButton(Buttons.ChallengeModeBtn).transform.localScale = Vector3.zero).OnComplete(() =>
                    {
                        GetButton(Buttons.ChallengeModeBtn).transform.DOScale(1, 0.3f);
                        GetImage(Images.TouchGuard).gameObject.SetActive(false);
                    });
                }
            }
            else
            {
                if (type == LowerBtnTypes.CharacterBtn)
                {
                    if (selectedLowerBtnType != LowerBtnTypes.BoostBtn && selectedLowerBtnType != LowerBtnTypes.ShopBtn)
                    {
                        GetButton(Buttons.RankModeBtn).transform.DOLocalMoveX(1500f, 0.5f).SetEase(Ease.InCubic);
                        GetButton(Buttons.ChallengeModeBtn).transform.DOLocalMoveX(1500f, 0.5f).SetEase(Ease.InCubic);
                    }

                    lowerMenu.transform.DOLocalMoveY(-400, 0.5f).SetRelative();

                    Managers.Camera.ChangeCameraLensOrthoSize(5, 0.5f);

                    StartCoroutine(wait());

                    IEnumerator wait()
                    {
                        yield return new WaitForSeconds(0.5f);

                        var popup = Managers.UI.ShowPopupUI<CharactorSelect_Popup>();
                        popup.onExit = () =>
                        {
                            SelectLowerBtn(LowerBtnTypes.HomeBtn, GetButton(Buttons.HomeBtn).GetComponent<LowerBtn>());
                        };

                        GetImage(Images.TouchGuard).gameObject.SetActive(false);
                    }
                }
                else if (type == LowerBtnTypes.BoostBtn)
                {
                    if (selectedLowerBtnType == LowerBtnTypes.HomeBtn)
                    {
                        GetButton(Buttons.RankModeBtn).transform.DOLocalMoveX(-1500, 0.5f).SetEase(Ease.InCubic);
                        GetButton(Buttons.ChallengeModeBtn).transform.DOLocalMoveX(-1500, 0.5f).SetEase(Ease.InCubic);
                    }

                    StartCoroutine(wait());

                    IEnumerator wait()
                    {
                        if (selectedLowerBtnType != LowerBtnTypes.ShopBtn)
                            yield return new WaitForSeconds(0.5f);

                        var popup = Managers.UI.ShowPopupUI<BoostShop_Popup>();

                        currentPopup = popup;
                        GetImage(Images.TouchGuard).gameObject.SetActive(false);
                    }
                }
                else if (type == LowerBtnTypes.ShopBtn)
                {
                    if (selectedLowerBtnType == LowerBtnTypes.HomeBtn)
                    {
                        GetButton(Buttons.RankModeBtn).transform.DOLocalMoveX(1500, 0.5f).SetEase(Ease.InCubic);
                        GetButton(Buttons.ChallengeModeBtn).transform.DOLocalMoveX(1500, 0.5f).SetEase(Ease.InCubic);
                    }

                    StartCoroutine(wait());

                    IEnumerator wait()
                    {
                        if (selectedLowerBtnType != LowerBtnTypes.BoostBtn)
                            yield return new WaitForSeconds(0.5f);
                        var popup = Managers.UI.ShowPopupUI<IapShop_Popup>();

                        currentPopup = popup;
                        GetImage(Images.TouchGuard).gameObject.SetActive(false);
                    }
                }
            }

            selectedLowerBtnType = type;

            GameObserver.Call(GameObserverType.Game.OnChangeHomeLowerBtn);
        }
    }
}
