using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LowerBtn : UI_Base
{
    enum Images
    {
        Frame,
        Icon
    }

    enum Texts
    {
        Text
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

    public void Select()
    {
        GetImage(Images.Frame).sprite = Managers.Resource.Load<Sprite>("UI/spr_lower tab slected_100");
        GetTextMesh(Texts.Text).gameObject.SetActive(true);

        GetImage(Images.Frame).GetComponent<RectTransform>().DOAnchorPosY(60, 0.23f);

        GetImage(Images.Icon).transform.DOScale(1.3f, 0.23f);

        var frameTransform = GetImage(Images.Frame).transform;
        frameTransform.DOScale(1.2f, 0.13f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => frameTransform.DOScale(1f, 0.13f).SetEase(Ease.InQuad));
    }

    public void UnSelect()
    {
        GetImage(Images.Frame).sprite = Managers.Resource.Load<Sprite>("UI/spr_lower tab_100");
        GetTextMesh(Texts.Text).gameObject.SetActive(false);

        GetImage(Images.Icon).transform.DOScale(1f, 0.23f);

        GetImage(Images.Frame).GetComponent<RectTransform>().DOAnchorPosY(0, 0.23f);
    }
}
