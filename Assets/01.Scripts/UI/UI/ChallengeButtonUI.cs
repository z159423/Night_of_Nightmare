using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeButtonUI : UI_Base
{
    enum Images
    {

    }

    enum Texts
    {
        StageText,
        PercentText
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
