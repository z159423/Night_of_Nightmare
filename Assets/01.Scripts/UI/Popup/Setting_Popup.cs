using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Define;
using LongriverSDKNS;

public class Setting_Popup : UI_Popup
{
    enum Buttons
    {
        BG,
        RestorePurchaseBtn,
        VolumeBtn,
        LanguageBtn
    }

    enum Images
    {
        TouchGuard
    }

    public override void Init()
    {
        base.Init();

        OpenAnimation(() =>
        {
            GetImage(Images.TouchGuard).gameObject.SetActive(false);
        });

        GetButton(Buttons.VolumeBtn).GetComponent<Image>().sprite = Managers.LocalData.Volume ? Managers.Resource.Load<Sprite>("UI/On_Btn") : Managers.Resource.Load<Sprite>("UI/Off_Btn");
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));

        GetButton(Buttons.BG).AddButtonEvent(() =>
        {
            GetImage(Images.TouchGuard).gameObject.SetActive(true);
            ClosePop(gameObject.FindRecursive("Panel").transform);
        }, false);

        GetButton(Buttons.VolumeBtn).AddButtonEvent(() =>
        {
            Managers.LocalData.Volume = !Managers.LocalData.Volume;

            GetButton(Buttons.VolumeBtn).GetComponent<Image>().sprite = Managers.LocalData.Volume ? Managers.Resource.Load<Sprite>("UI/On_Btn") : Managers.Resource.Load<Sprite>("UI/Off_Btn");
        });

        GetButton(Buttons.RestorePurchaseBtn).AddButtonEvent(() =>
        {
            LongriverSDKUserPayment.instance.restorePurchases();
        });

        GetButton(Buttons.LanguageBtn).AddButtonEvent(() =>
        {
            Managers.UI.ShowPopupUI<Language_Popup>();
        });
    }
}
