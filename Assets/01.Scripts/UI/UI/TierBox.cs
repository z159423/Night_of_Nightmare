using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TierBox : UI_Base
{
    enum Images
    {
        Icon
    }

    enum Texts
    {
        NameText,
        ScoreText
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

    public void Setting(Define.Tier tier)
    {
        GetImage(Images.Icon).sprite = Managers.Resource.Load<Sprite>($"Tier/{tier.ToString()}");
        GetImage(Images.Icon).SetNativeSize();
        GetTextMesh(Texts.NameText).text = tier.ToString();
        GetTextMesh(Texts.ScoreText).text = Define.TierToScore[tier].ToString();
    }
}
