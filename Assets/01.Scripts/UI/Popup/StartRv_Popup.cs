using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StartRv_Popup : UI_Popup
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
        Rv,
        Ticket
    }

    enum Texts
    {
    }

    public Action onShowRv;

    public override void Init()
    {
        base.Init();

        OpenAnimation(() =>
        {
            GetImage(Images.TouchGuard).gameObject.SetActive(false);
        });

        Action updateTicketImages = () =>
        {
            int ticketCount = Managers.LocalData.PlayerRvTicketCount;
            GetImage(Images.Ticket).gameObject.SetActive(ticketCount > 0);
            GetImage(Images.Rv).gameObject.SetActive(ticketCount <= 0);
        };

        this.SetListener(GameObserverType.Game.OnChangeTicketCount, updateTicketImages);
        updateTicketImages();
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

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
            Managers.Ad.ShowRewardAd(() =>
            {
                onShowRv?.Invoke();
                ClosePop(gameObject.FindRecursive("Panel").transform);
            });
        }, false);
    }
}
