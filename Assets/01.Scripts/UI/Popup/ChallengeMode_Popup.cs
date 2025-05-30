using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChallengeMode_Popup : UI_Popup
{
    enum Buttons
    {
        ExitBtn,
    }

    enum Images
    {
        TouchGuard
    }

    enum Texts
    {

    }

    VerticalLayoutGroup layout;

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

        layout = GetComponentInChildren<VerticalLayoutGroup>();

        GetButton(Buttons.ExitBtn).AddButtonEvent(Exit);
    }

    public override void Reset()
    {

    }

    private void Exit()
    {
        ClosePopupUI();
    }
}
