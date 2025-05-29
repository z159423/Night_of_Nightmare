using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBoxUI : UI_Base
{
    enum Images
    {
        LoadingIcon,
        PlayerIcon
    }

    enum Texts
    {
        NameText
    }

    bool _init = false;

    public override void Init()
    {
        if (!_init)
        {
            FirstSetting();
        }

    }

    public void FirstSetting()
    {
        _init = true;

        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    public void Setting()
    {

    }
}
