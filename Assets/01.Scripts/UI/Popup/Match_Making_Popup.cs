using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Match_Making_Popup : UI_Popup
{
    enum Buttons
    {
        ExitBtn,
        MatchBtn,
        InfoBtn,
        MatchingBtn
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

        GetButton(Buttons.ExitBtn).AddButtonEvent(Exit);
        GetButton(Buttons.MatchBtn).AddButtonEvent(StartMatching);
        GetButton(Buttons.InfoBtn).AddButtonEvent(() =>
        {

        });
    }

    private void StartMatching()
    {

    }


    public override void Reset()
    {

    }

    private void Exit()
    {
        ClosePopupUI();
    }
}
