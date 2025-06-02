using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tier_Popup : UI_Popup
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

    GameObject layout;

    public override void Init()
    {
        base.Init();

        foreach (var tier in Define.TierToScore.Keys)
        {
            var tierBox = Managers.Resource.Instantiate("TierBox").GetComponent<TierBox>();
            tierBox.Init();
            tierBox.Setting(tier);
            tierBox.transform.SetParent(layout.transform, false);
        }
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        layout = gameObject.FindRecursive("Layout");

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
