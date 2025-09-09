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
        AbilityBtn,
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
        SessionRewardBtn,
        RankModeRvBtn,
        RandomBoxRvBtn
    }

    enum Texts
    {
        GemText,
        TicketCount,
        SessionRewardText,
        RandomBoxRvCountText
    }

    enum Images
    {
        TouchGuard,
        RankImage,
        RandomBoxReddot
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

    private Ability_Popup abilityPopup;


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

        challengeLock.SetActive(Managers.LocalData.PlayerWinCount < 3);

        this.SetListener(GameObserverType.Game.Timer, () =>
        {
            UpdateUI();
            SetRandomRvText();

        });

        GetImage(Images.RankImage).sprite = Managers.Resource.Load<Sprite>($"Tier/{Define.GetPlayerCurrentTier().ToString()}");
        GetImage(Images.RankImage).SetNativeSize();

        GetButton(Buttons.RankModeRvBtn).gameObject.SetActive(Managers.LocalData.PlayerGameCount >= 1 && (!Managers.Game.goldRvBonus && !Managers.Game.energyRvBonus));

        this.SetListener(GameObserverType.Game.OnShowRandomReward, () =>
        {
            SetRandomRvText();
        });

        SetRandomRvText();

        void SetRandomRvText()
        {
            // 현재 로컬 시간
            var now = System.DateTime.Now;

            // 마지막 시청 날짜 불러오기 (Unix timestamp를 DateTime으로 변환)
            var lastShowTime = System.DateTimeOffset.FromUnixTimeSeconds(Managers.LocalData.RandomBoxRvShowDate).ToLocalTime().DateTime;

            // 서비스 날짜 기준으로 날짜가 바뀌었는지 확인
            // 현재 시간이 9시 이전이면 전날로 간주, 9시 이후면 당일로 간주
            var currentServiceDate = now.Hour < 9 ? now.Date.AddDays(-1) : now.Date;
            var lastServiceDate = lastShowTime.Hour < 9 ? lastShowTime.Date.AddDays(-1) : lastShowTime.Date;

            // 서비스 날짜가 다르면 카운트 리셋
            if (currentServiceDate != lastServiceDate)
            {
                GetTextMesh(Texts.RandomBoxRvCountText).text = $"0 / {Define.RandomBoxRvCount}";
            }
            else
            {
                if (Managers.LocalData.RandomBoxRvCount >= Define.RandomBoxRvCount)
                {
                    GetTextMesh(Texts.RandomBoxRvCountText).text = $"리셋 가능";
                }
                else
                {
                    var nextResetTime = lastShowTime.AddDays(1).Date.AddHours(9); // 다음 리셋 시간 (다음 날 9시)
                    var timeRemaining = nextResetTime - now; // 남은 시간 계산

                    if (timeRemaining.TotalSeconds < 0)
                    {
                        // 이미 리셋 시간이 지났다면 카운트를 리셋
                        Managers.LocalData.RandomBoxRvCount = 0;
                        GetTextMesh(Texts.RandomBoxRvCountText).text = $"0 / {Define.RandomBoxRvCount}";
                    }
                    else
                    {
                        GetTextMesh(Texts.RandomBoxRvCountText).text = Util.FormatTimeRemaining(timeRemaining);
                    }
                }

                GetTextMesh(Texts.RandomBoxRvCountText).text = $"{Managers.LocalData.RandomBoxRvCount} / {Define.RandomBoxRvCount}";
            }
        }

        GetImage(Images.RandomBoxReddot).gameObject.SetActive(!Managers.LocalData.IsOpenRandomBoxRv);

        Managers.Tutorial.StartTutorial(GetButton(Buttons.RankModeBtn), PlayerTutorialStep.StartRankGame);
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
        GetButton(Buttons.AbilityBtn).GetComponent<LowerBtn>().Init();

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

        GetButton(Buttons.AbilityBtn).AddButtonEvent(() =>
        {
            var lowerbtn = GetButton(Buttons.AbilityBtn).GetComponent<LowerBtn>();

            SelectLowerBtn(LowerBtnTypes.QeustBtn, lowerbtn);
        });

        // GetButton(Buttons.QeustBtn).AddButtonEvent(() =>
        // {
        //     var btn = GetButton(Buttons.QeustBtn);
        //     btn.transform.DOScale(1.15f, 0.12f).SetEase(Ease.Linear).OnComplete(() =>
        //     {
        //         btn.transform.DOScale(1f, 0.12f).SetEase(Ease.Linear);
        //     });

        //     // Managers.UI.ShowNotificationPopup("global.str_update_coming_soon", 2);

        //     ShowAbilityPopup();
        // });

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

        GetButton(Buttons.RankModeRvBtn).AddButtonEvent(() =>
        {
            var popup = Managers.UI.ShowPopupUI<StartRv_Popup>();

            popup.onShowRv = () =>
            {
                Managers.Game.goldRvBonus = false;
                Managers.Game.energyRvBonus = false;

                if (UnityEngine.Random.Range(0, 2) == 0)
                    Managers.Game.energyRvBonus = true;
                else
                    Managers.Game.goldRvBonus = true;

                GetButton(Buttons.RankModeRvBtn).gameObject.SetActive(false);
            };
        });

        GetButton(Buttons.RandomBoxRvBtn).gameObject.SetActive(ShouldShowRandomBoxRvBtn());

        GetButton(Buttons.RandomBoxRvBtn).AddButtonEvent(() =>
        {
            var popup = Managers.UI.ShowPopupUI<RandomBoxRv_Popup>();

            GetImage(Images.RandomBoxReddot).gameObject.SetActive(false);
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

        challengeLock.SetActive(Managers.LocalData.PlayerWinCount < 3);
    }

    public override void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SelectLowerBtn(LowerBtnTypes type, LowerBtn btn)
    {
        if (selectedLowerBtn == btn) return;

        btn.Select();
        selectedLowerBtn.UnSelect();
        selectedLowerBtn = btn;

        GetImage(Images.TouchGuard).gameObject.SetActive(true);

        // 현재 팝업 처리
        if (currentPopup != null)
        {
            HandleCurrentPopup(type);
        }
        else
        {
            TransitionToNewState(type);
        }
    }

    private void HandleCurrentPopup(LowerBtnTypes newType)
    {
        if (currentPopup is Ability_Popup)
        {
            // Ability 팝업은 바로 닫기
            // currentPopup.ClosePopupUI();
            currentPopup.gameObject.SetActive(false);
            currentPopup = null;
            TransitionToNewState(newType);
        }
        else
        {
            // 다른 팝업들은 슬라이드 아웃 애니메이션
            float moveValue = selectedLowerBtnType == LowerBtnTypes.BoostBtn ? 1500f : -1500f;
            currentPopup.transform.DOLocalMoveX(moveValue, 0.5f).SetEase(Ease.InCubic).OnComplete(() =>
            {
                currentPopup.ClosePopupUI();
                currentPopup = null;
                TransitionToNewState(newType);
            });
        }
    }

    private void TransitionToNewState(LowerBtnTypes newType)
    {
        var transition = GetTransitionPlan(selectedLowerBtnType, newType);
        ExecuteTransition(transition, newType);
    }

    private TransitionPlan GetTransitionPlan(LowerBtnTypes from, LowerBtnTypes to)
    {
        return new TransitionPlan
        {
            needsGameButtonAnimation = NeedsGameButtonAnimation(from, to),
            needsLowerMenuMove = NeedsLowerMenuMove(from, to),
            needsCameraChange = NeedsCameraChange(from, to),
            waitTime = GetWaitTime(from, to),
            finalAction = GetFinalAction(to)
        };
    }

    private bool NeedsGameButtonAnimation(LowerBtnTypes from, LowerBtnTypes to)
    {
        // HomeBtn으로 돌아가거나, HomeBtn에서 나가는 경우
        return to == LowerBtnTypes.HomeBtn ||
               (from == LowerBtnTypes.HomeBtn && to != LowerBtnTypes.HomeBtn);
    }

    private bool NeedsLowerMenuMove(LowerBtnTypes from, LowerBtnTypes to)
    {
        // CharacterBtn으로 가거나 CharacterBtn에서 나오는 모든 경우
        return to == LowerBtnTypes.CharacterBtn || from == LowerBtnTypes.CharacterBtn;
    }

    private bool NeedsCameraChange(LowerBtnTypes from, LowerBtnTypes to)
    {
        return NeedsLowerMenuMove(from, to);
    }

    private float GetWaitTime(LowerBtnTypes from, LowerBtnTypes to)
    {
        if (to == LowerBtnTypes.CharacterBtn) return 0.5f;
        if (to == LowerBtnTypes.BoostBtn && from != LowerBtnTypes.ShopBtn) return 0.5f;
        if (to == LowerBtnTypes.ShopBtn && from != LowerBtnTypes.BoostBtn) return 0.5f;
        if (to == LowerBtnTypes.QeustBtn) return 0.3f;
        return 0f;
    }

    private System.Action GetFinalAction(LowerBtnTypes to)
    {
        return to switch
        {
            LowerBtnTypes.CharacterBtn => ShowCharacterPopup,
            LowerBtnTypes.BoostBtn => ShowBoostPopup,
            LowerBtnTypes.ShopBtn => ShowShopPopup,
            LowerBtnTypes.QeustBtn => ShowAbilityPopup,
            _ => () => GetImage(Images.TouchGuard).gameObject.SetActive(false)
        };
    }

    private void ExecuteTransition(TransitionPlan plan, LowerBtnTypes newType)
    {
        // 게임 버튼 애니메이션
        if (plan.needsGameButtonAnimation)
        {
            AnimateGameButtons(selectedLowerBtnType, newType);
        }

        // 하단 메뉴 이동
        if (plan.needsLowerMenuMove)
        {
            AnimateLowerMenu(selectedLowerBtnType, newType);
        }

        // 카메라 변경
        if (plan.needsCameraChange)
        {
            AnimateCamera(selectedLowerBtnType, newType);
        }

        // 최종 액션 실행
        StartCoroutine(ExecuteFinalAction(plan.waitTime, plan.finalAction, newType));
    }

    private void AnimateGameButtons(LowerBtnTypes from, LowerBtnTypes to)
    {
        if (to == LowerBtnTypes.HomeBtn)
        {
            // 게임 버튼들을 화면으로 복귀
            AnimateGameButtonsIn();
        }
        else if (from == LowerBtnTypes.HomeBtn)
        {
            // 게임 버튼들을 화면 밖으로
            float moveX = (to == LowerBtnTypes.ShopBtn) ? 1500f : -1500f;
            AnimateGameButtonsOut(moveX);
        }
    }

    private void AnimateGameButtonsIn()
    {
        GetButton(Buttons.RankModeBtn).transform.DOLocalMoveX(0f, 0).SetEase(Ease.InCubic)
            .OnStart(() => GetButton(Buttons.RankModeBtn).transform.localScale = Vector3.zero)
            .OnComplete(() => GetButton(Buttons.RankModeBtn).transform.DOScale(1, 0.3f));

        GetButton(Buttons.ChallengeModeBtn).transform.DOLocalMoveX(0f, 0).SetEase(Ease.InCubic)
            .OnStart(() => GetButton(Buttons.ChallengeModeBtn).transform.localScale = Vector3.zero)
            .OnComplete(() => GetButton(Buttons.ChallengeModeBtn).transform.DOScale(1, 0.3f));
    }

    private void AnimateGameButtonsOut(float moveX)
    {
        GetButton(Buttons.RankModeBtn).transform.DOLocalMoveX(moveX, 0.5f).SetEase(Ease.InCubic);
        GetButton(Buttons.ChallengeModeBtn).transform.DOLocalMoveX(moveX, 0.5f).SetEase(Ease.InCubic);
    }

    private void AnimateLowerMenu(LowerBtnTypes from, LowerBtnTypes to)
    {
        if (to == LowerBtnTypes.CharacterBtn)
        {
            // 어떤 버튼에서든 CharacterBtn으로 갈 때 - 메뉴 내리기
            lowerMenu.transform.DOLocalMoveY(-400, 0.5f).SetRelative();
        }
        else if (from == LowerBtnTypes.CharacterBtn)
        {
            // CharacterBtn에서 다른 버튼으로 갈 때 - 메뉴 올리기
            lowerMenu.transform.DOLocalMoveY(400, 0.5f).SetRelative()
                .OnComplete(() => GetImage(Images.TouchGuard).gameObject.SetActive(false));
        }
    }

    private void AnimateCamera(LowerBtnTypes from, LowerBtnTypes to)
    {
        if (to == LowerBtnTypes.CharacterBtn)
        {
            // 어떤 버튼에서든 CharacterBtn으로 갈 때 - 카메라 줌인
            Managers.Camera.ChangeCameraLensOrthoSizeAndPosition(5, new Vector3(0, -1.5f, -10), 0.5f);
        }
        else if (from == LowerBtnTypes.CharacterBtn)
        {
            // CharacterBtn에서 다른 버튼으로 갈 때 - 카메라 줌아웃
            Managers.Camera.ChangeCameraLensOrthoSizeAndPosition(7, new Vector3(0, 0f, -10), 0.5f);
        }
    }

    private IEnumerator ExecuteFinalAction(float waitTime, System.Action action, LowerBtnTypes newType)
    {
        if (waitTime > 0)
            yield return new WaitForSeconds(waitTime);

        action?.Invoke();

        selectedLowerBtnType = newType;
        GameObserver.Call(GameObserverType.Game.OnChangeHomeLowerBtn);
    }

    // 팝업 표시 메서드들
    private void ShowCharacterPopup()
    {
        var popup = Managers.UI.ShowPopupUI<CharactorSelect_Popup>();
        popup.onExit = () => SelectLowerBtn(LowerBtnTypes.HomeBtn, GetButton(Buttons.HomeBtn).GetComponent<LowerBtn>());
        GetImage(Images.TouchGuard).gameObject.SetActive(false);
    }

    private void ShowBoostPopup()
    {
        var popup = Managers.UI.ShowPopupUI<BoostShop_Popup>();
        currentPopup = popup;
        GetImage(Images.TouchGuard).gameObject.SetActive(false);
    }

    private void ShowShopPopup()
    {
        var popup = Managers.UI.ShowPopupUI<IapShop_Popup>();
        currentPopup = popup;
        GetImage(Images.TouchGuard).gameObject.SetActive(false);
    }

    private void ShowAbilityPopup()
    {
        if (abilityPopup == null)
        {
            var popup = Managers.UI.ShowPopupUI<Ability_Popup>();
            popup.transform.SetParent(lowerMenu.transform.parent);
            popup.transform.SetSiblingIndex(lowerMenu.transform.GetSiblingIndex());
            currentPopup = popup;
            abilityPopup = popup;
        }
        else
        {
            abilityPopup.gameObject.SetActive(true);
            GameObserver.Call(GameObserverType.Game.OnAbilityChanged);
            currentPopup = abilityPopup;
        }

        GetImage(Images.TouchGuard).gameObject.SetActive(false);
    }

    // 헬퍼 클래스
    private class TransitionPlan
    {
        public bool needsGameButtonAnimation;
        public bool needsLowerMenuMove;
        public bool needsCameraChange;
        public float waitTime;
        public System.Action finalAction;
    }

    bool ShouldShowRandomBoxRvBtn()
    {
        // 현재 로컬 시간
        var now = System.DateTime.Now;

        // 마지막 시청 날짜 불러오기 (Unix timestamp를 DateTime으로 변환)
        var lastShowTime = System.DateTimeOffset.FromUnixTimeSeconds(Managers.LocalData.RandomBoxRvShowDate).DateTime;

        // 서비스 날짜 기준으로 날짜가 바뀌었는지 확인
        var currentServiceDate = now.Hour < 9 ? now.Date.AddDays(-1) : now.Date;
        var lastServiceDate = lastShowTime.Hour < 9 ? lastShowTime.Date.AddDays(-1) : lastShowTime.Date;

        // 서비스 날짜가 다르거나, 같은 날인데 5회 미만이면 버튼 표시
        return currentServiceDate != lastServiceDate || Managers.LocalData.RandomBoxRvCount < Define.RandomBoxRvCount;
    }
}
