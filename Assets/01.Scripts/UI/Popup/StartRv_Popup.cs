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
        RvBtn
    }

    enum Images
    {
        TouchGuard
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

        GetButton(Buttons.RvBtn).AddButtonEvent(() =>
        {
            onShowRv?.Invoke();
            ClosePop(gameObject.FindRecursive("Panel").transform);

        }, false);
    }
}
