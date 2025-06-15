using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class BoostShop_Popup : UI_Popup
{
    enum Buttons
    {
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

        OpenAnimation();
    }

    public override void FirstSetting()
    {
        base.FirstSetting();
    }

    public override void Reset()
    {

    }

    private void Exit()
    {
        ClosePopupUI();
    }
}
