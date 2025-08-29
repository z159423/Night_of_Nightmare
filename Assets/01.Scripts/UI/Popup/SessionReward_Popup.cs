using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SessionReward_Popup : UI_Popup
{
    enum Buttons
    {
        BGExit,
        CloseBtn
    }

    enum Images
    {
        TouchGuard
    }

    enum Texts
    {
        OnlineTimeText,
        ResetTimeText
    }

    SessionRewardSlot[] sessionSlotUI;

    public override void Init()
    {
        base.Init();

        OpenAnimation();

        this.SetListener(GameObserverType.Game.Timer, UpdateTime);
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        sessionSlotUI = GetComponentsInChildren<SessionRewardSlot>();

        for (int i = 0; i < sessionSlotUI.Length; i++)
        {
            sessionSlotUI[i].Init();
        }

        GetButton(Buttons.BGExit).AddButtonEvent(() =>
        {
            ClosePopupUI();
        });

        GetButton(Buttons.CloseBtn).AddButtonEvent(() =>
        {
            ClosePopupUI();
        });

        UpdateTime();
    }

    public override void Reset()
    {

    }

    void UpdateTime()
    {
        GetTextMesh(Texts.ResetTimeText).text = Managers.Localize.GetDynamicText("session_reward_reset", Managers.SessionReward.SecondsToDailyReset());
        GetTextMesh(Texts.OnlineTimeText).text = Managers.SessionReward.SecondsToNextReward() == -1 ? Managers.Localize.GetText("session_reward_complete") : Managers.Localize.GetDynamicText("session_reward_next", SessionRewardManager.FormatHMS(Managers.SessionReward.SecondsToNextReward()));

        for (int i = 0; i < sessionSlotUI.Length; i++)
        {
            sessionSlotUI[i].UpdateUI();
        }
    }

    private void Exit()
    {
        ClosePopupUI();
    }
}
