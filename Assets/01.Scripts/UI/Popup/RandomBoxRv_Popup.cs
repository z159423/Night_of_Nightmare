using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RandomBoxRv_Popup : UI_Popup
{
    enum Buttons
    {
        BG,
        RvBtn,
        CloseBtn
    }

    enum Images
    {
        TouchGuard,
        Ticket,
        Rv
    }

    enum Texts
    {
        CanShowCountText,
        ResetText
    }

    public Action onShowRv;

    private Coroutine updateCoroutine;

    [SerializeField] private Sprite[] btnSprites;

    public override void Init()
    {
        base.Init();

        OpenAnimation(() =>
        {
            GetImage(Images.TouchGuard).gameObject.SetActive(false);
        });

        this.SetListener(GameObserverType.Game.OnChangeTicketCount, () =>
        {
            int ticketCount = Managers.LocalData.PlayerRvTicketCount;
            GetImage(Images.Ticket).gameObject.SetActive(ticketCount > 0);
            GetImage(Images.Rv).gameObject.SetActive(ticketCount <= 0);
        });
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        Managers.LocalData.IsOpenRandomBoxRv = true;

        GetButton(Buttons.BG).AddButtonEvent(() =>
        {
            GetImage(Images.TouchGuard).gameObject.SetActive(true);
            ClosePop(gameObject.FindRecursive("Panel").transform);
        }, false);

        GetButton(Buttons.CloseBtn).AddButtonEvent(() =>
        {
            GetImage(Images.TouchGuard).gameObject.SetActive(true);
            ClosePop(gameObject.FindRecursive("Panel").transform);
        }, false);

        GetButton(Buttons.RvBtn).AddButtonEvent(() =>
        {
            if (Managers.LocalData.RandomBoxRvCount >= Define.RandomBoxRvCount)
                return;

            Managers.Ad.ShowRewardAd(() =>
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
                if (currentServiceDate.Day != lastServiceDate.Day)
                {
                    Managers.LocalData.RandomBoxRvCount = 1;
                }
                else
                {
                    if (Managers.LocalData.RandomBoxRvCount >= Define.RandomBoxRvCount)
                        return;

                    Managers.LocalData.RandomBoxRvCount++;
                }

                onShowRv?.Invoke();

                Managers.LocalData.PlayerGemCount += 50;
                Managers.UI.GenerateUIParticle(Managers.UI._currentScene.transform, uiParticleMarkerType.GemIcon, Define.ItemType.Gem);

                switch (UnityEngine.Random.Range(0, 4))
                {
                    case 0:
                        Managers.LocalData.AddBoostItem(Define.BoostType.Lamp, 1);
                        Managers.UI.GenerateUIParticle(Managers.UI._currentScene.transform, uiParticleMarkerType.BoostBtn, Define.ItemType.Boost_Ramp);
                        break;
                    case 1:
                        Managers.LocalData.AddBoostItem(Define.BoostType.HammerThrow, 1);
                        Managers.UI.GenerateUIParticle(Managers.UI._currentScene.transform, uiParticleMarkerType.BoostBtn, Define.ItemType.Boost_Hammer);
                        break;
                    case 2:
                        Managers.LocalData.AddBoostItem(Define.BoostType.HolyProtection, 1);
                        Managers.UI.GenerateUIParticle(Managers.UI._currentScene.transform, uiParticleMarkerType.BoostBtn, Define.ItemType.Boost_Shield);
                        break;
                    case 3:
                        Managers.LocalData.AddBoostItem(Define.BoostType.Overheat, 1);
                        Managers.UI.GenerateUIParticle(Managers.UI._currentScene.transform, uiParticleMarkerType.BoostBtn, Define.ItemType.Boost_Fire);
                        break;
                }

                // 현재 시간을 Unix timestamp로 저장
                Managers.LocalData.RandomBoxRvShowDate = System.DateTimeOffset.Now.ToUnixTimeSeconds();

                GameObserver.Call(GameObserverType.Game.OnShowRandomReward);

                Managers.Audio.PlaySound("snd_get_item");

                // ClosePop(gameObject.FindRecursive("Panel").transform);

                UpdateUI();
            });
        }, false);

        UpdateUI();

        // 실시간 업데이트 시작 (5회 다 사용했을 때만)
        if (Managers.LocalData.RandomBoxRvCount >= 5)
        {
            updateCoroutine = StartCoroutine(UpdateTimeCoroutine());
        }
    }

    private IEnumerator UpdateTimeCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // 1초마다 업데이트

            // 5회 다 사용했을 때만 UI 업데이트
            if (Managers.LocalData.RandomBoxRvCount >= 5)
            {
                UpdateUI();
            }
            else
            {
                break; // 코루틴 종료
            }
        }
    }

    public override void Reset()
    {
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }
    }

    void UpdateUI()
    {
        var now = System.DateTime.Now;
        int currentCount = Managers.LocalData.RandomBoxRvCount;

        // 첫 실행이 아닌 경우에만 날짜 비교
        if (Managers.LocalData.RandomBoxRvShowDate != 0)
        {
            // 로컬 시간대로 변환하여 불러오기
            var lastShowTime = System.DateTimeOffset.FromUnixTimeSeconds(Managers.LocalData.RandomBoxRvShowDate).ToLocalTime().DateTime;

            var currentServiceDate = now.Hour < 9 ? now.Date.AddDays(-1) : now.Date;
            var lastServiceDate = lastShowTime.Hour < 9 ? lastShowTime.Date.AddDays(-1) : lastShowTime.Date;

            if (currentServiceDate != lastServiceDate)
            {
                currentCount = 0;
            }
        }
        else
        {
            currentCount = 0;
        }

        GetTextMesh(Texts.CanShowCountText).text = Managers.Localize.GetDynamicText("enable_show_count", (5 - currentCount).ToString());

        if (currentCount >= 5)
        {
            GetTextMesh(Texts.ResetText).gameObject.SetActive(true);
            var nextResetTime = GetNextResetTime(now);
            var timeRemaining = nextResetTime - now;
            GetTextMesh(Texts.ResetText).text = Managers.Localize.GetDynamicText("session_reward_reset", Util.FormatTimeRemaining(timeRemaining));
            GetButton(Buttons.RvBtn).image.sprite = btnSprites[1];
        }
        else
        {
            GetTextMesh(Texts.ResetText).gameObject.SetActive(false);
            GetButton(Buttons.RvBtn).image.sprite = btnSprites[0];
        }

        int ticketCount = Managers.LocalData.PlayerRvTicketCount;
        GetImage(Images.Ticket).gameObject.SetActive(ticketCount > 0);
        GetImage(Images.Rv).gameObject.SetActive(ticketCount <= 0);
    }

    // 다음 9시 리셋 시간 계산
    private System.DateTime GetNextResetTime(System.DateTime currentTime)
    {
        var todayReset = new System.DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 9, 0, 0);

        // 현재 시간이 오늘 9시 이전이면 오늘 9시가 다음 리셋
        // 현재 시간이 오늘 9시 이후면 내일 9시가 다음 리셋
        if (currentTime < todayReset)
        {
            return todayReset;
        }
        else
        {
            return todayReset.AddDays(1);
        }
    }
}
