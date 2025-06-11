using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

public class Upgrade_Popup : UI_Popup
{

    enum Buttons
    {
        ExitBtn
    }

    enum Images
    {
        TouchGuard
    }

    enum Texts
    {
       
    }

    private VerticalLayoutGroup layout;
    private Structure selectedStructure;

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

        layout = gameObject.FindRecursive("Layout").GetComponent<VerticalLayoutGroup>();

        GetButton(Buttons.ExitBtn).AddButtonEvent(Exit);
    }

    public void Setting(Structure structure)
    {
        selectedStructure = structure;
    }

    public override void Reset()
    {

    }

    private void Exit()
    {
        ClosePopupUI();
    }
}
