using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Define;

public class Exit_Popup : UI_Popup
{
    enum Buttons
    {
        ExitBtn,
        ConfirmBtn
    }

    enum Images
    {
        TouchGuard
    }

    enum Texts
    {

    }

    public override void Init()
    {
        base.Init();
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        GetButton(Buttons.ExitBtn).onClick.AddListener(Exit);
        GetButton(Buttons.ConfirmBtn).onClick.AddListener(() =>
        {
            Managers.Game.GoHome();
            Exit();
        });
    }

    public override void Reset()
    {

    }

    private void Exit()
    {
        ClosePopupUI();
    }
}
