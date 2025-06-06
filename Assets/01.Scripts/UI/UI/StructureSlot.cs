using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StructureSlot : UI_Base
{
    enum Images
    {
        Icon,
        CoinIcon,
        EnergyIcon
    }

    enum Texts
    {
        NameText,
        DescText,
        FreeText,
        ValueText
    }

    enum Buttons
    {
        Button
    }

    bool _init = false;

    public override void Init()
    {
        if (!_init)
        {
            FirstSetting();
        }
    }

    public void Setting()
    {
        
    }

    public void FirstSetting()
    {
        _init = true;

        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }
}
