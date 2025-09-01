using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
using DG.Tweening;
using Unity.VisualScripting;
using LongriverSDKNS;

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
        CenterCharactorBtn,
        ChallengeLockBtn,
        GemBtn,
        AdTicket,
        RankPointUp,
        RankPointDown,
        SettingBtn,
        AttendanceBtn,
        SessionRewardBtn
    }

    enum Texts
    {
        GemText,
        TicketCount,
        SessionRewardText
    }

    enum Images
    {
        TouchGuard,
        RankImage
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

    private GameObject challengeLock;

    private Transform cheat;


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

        GetTextMesh(Texts.GemText).text = Managers.LocalData.PlayerGemCount.ToString();
        GetTextMesh(Texts.TicketCount).text = Managers.LocalData.PlayerRvTicketCount.ToString();

        challengeLock.SetActive(Managers.LocalData.PlayerWinCount < 1);

        this.SetListener(GameObserverType.Game.Timer, () =>
        {
            UpdateUI();
        });

        GetImage(Images.RankImage).sprite = Managers.Resource.Load<Sprite>($"Tier/{Define.GetPlayerCurrentTier().ToString()}");
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

        challengeLock = gameObject.FindRecursive("ChallengeLock");

        cheat = gameObject.FindRecursive("Cheat").transform;

        GetButton(Buttons.ShopBtn).AddButtonEvent(() =>
        {
            if (!GameManager.sdkLogin)
            {
                Managers.UI.ShowNotificationPopup("shop_loading", 2);
                return;
            }

            var lowerbtn = GetButton(Buttons.ShopBtn).GetComponent<LowerBtn>();

            SelectLowerBtn(LowerBtnTypes.ShopBtn, lowerbtn);
        });

        GetButton(Buttons.CharacterBtn).AddButtonEvent(() =>
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
            var btn = GetButton(Buttons.QeustBtn);
            btn.transform.DOScale(1.15f, 0.12f).SetEase(Ease.Linear).OnComplete(() =>
            {
                btn.transform.DOScale(1f, 0.12f).SetEase(Ease.Linear);
            });

            // Managers.UI.ShowNotificationPopup("global.str_update_coming_soon", 2);

            Managers.UI.ShowPopupUI<Ability_Popup>();
        });

        GetButton(Buttons.RankModeBtn).AddButtonEvent(() =>
        {
            var popup = Managers.UI.ShowPopupUI<Match_Making_Popup>();
            popup.Setting(false, 0);
        }, true);

        GetButton(Buttons.ChallengeModeBtn).AddButtonEvent(() =>
        {
            Managers.UI.ShowPopupUI<ChallengeMode_Popup>();
        });

        GetButton(Buttons.CenterCharactorBtn).AddButtonEvent(() =>
        {
            var lowerbtn = GetButton(Buttons.CharacterBtn).GetComponent<LowerBtn>();

            SelectLowerBtn(LowerBtnTypes.CharacterBtn, lowerbtn);
        });

        GetButton(Buttons.ChallengeLockBtn).AddButtonEvent(() =>
        {
            var btn = GetButton(Buttons.ChallengeLockBtn);
            btn.transform.DOScale(1.15f, 0.12f).SetEase(Ease.Linear).OnComplete(() =>
            {
                btn.transform.DOScale(1f, 0.12f).SetEase(Ease.Linear);
            });

            Managers.UI.ShowNotificationPopup("global.str_not_enough_rank", 2);

            Managers.Audio.PlaySound("snd_stage_unlock");
        }, false);

        this.SetListener(GameObserverType.Game.OnChangeGemCount, () =>
        {
            GetTextMesh(Texts.GemText).text = Managers.LocalData.PlayerGemCount.ToString();
        });

        this.SetListener(GameObserverType.Game.OnChangeTicketCount, () =>
        {
            GetTextMesh(Texts.TicketCount).text = Managers.LocalData.PlayerRvTicketCount.ToString();
        });

        GetButton(Buttons.GemBtn).AddButtonEvent(() =>
        {
            Managers.LocalData.PlayerGemCount += 1000;
        });

        GetButton(Buttons.AdTicket).AddButtonEvent(() =>
        {
            Managers.LocalData.CheatMode = Managers.LocalData.CheatMode == 0 ? 1 : 0;
            GameObserver.Call(GameObserverType.Game.OnCheatModeOn);
        });

        this.SetListener(GameObserverType.Game.OnCheatModeOn, () =>
        {
            cheat.gameObject.SetActive(Managers.LocalData.CheatMode == 1);
        });

        cheat.gameObject.SetActive(Managers.LocalData.CheatMode == 1);

        GetButton(Buttons.RankPointUp).AddButtonEvent(() =>
        {
            Managers.LocalData.PlayerRankingPoint += 100;
        });

        GetButton(Buttons.RankPointDown).AddButtonEvent(() =>
        {
            Managers.LocalData.PlayerRankingPoint -= 100;
        });

        GetButton(Buttons.SettingBtn).AddButtonEvent(() =>
        {
            Managers.UI.ShowPopupUI<Setting_Popup>();
        });

        GetButton(Buttons.AttendanceBtn).AddButtonEvent(() =>
        {
            Managers.UI.ShowPopupUI<Attendance_Popup>();
        });

        GetButton(Buttons.SessionRewardBtn).AddButtonEvent(() =>
        {
            Managers.UI.ShowPopupUI<SessionReward_Popup>();
        });
    }

    void UpdateUI()
    {
        GetButton(Buttons.AttendanceBtn).gameObject.FindRecursive("Reddot").SetActive(Managers.Attendance.CanClaimToday());

        GetButton(Buttons.SessionRewardBtn).gameObject.FindRecursive("Reddot").SetActive(Managers.SessionReward.IsNextRewardClaimable());
        GetTextMesh(Texts.SessionRewardText).text = Managers.SessionReward.SecondsToNextReward() == -1 ? "" : SessionRewardManager.FormatHMS(Managers.SessionReward.SecondsToNextReward());
    }

    public override void Show()
    {
        gameObject.SetActive(true);

        if (challengeLock == null)
            challengeLock = gameObject.FindRecursive("ChallengeLock");

        challengeLock.SetActive(Managers.LocalData.PlayerWinCount < 1);
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

                    Managers.Camera.ChangeCameraLensOrthoSizeAndPosition(7, new Vector3(0, 0f, -10), 0.5f);

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

                    Managers.Camera.ChangeCameraLensOrthoSizeAndPosition(5, new Vector3(0, -1.5f, -10), 0.5f);

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
