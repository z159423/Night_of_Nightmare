using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BoostBoxUI : UI_Base
{
    enum Buttons
    {
        PurchaseBtn
    }

    enum Images
    {
        Icon
    }

    enum Texts
    {
        CountText,
        NameText,
        DescText,
        PriceText
    }

    bool init = false;

    public override void Init()
    {
        if (!init)
        {
            FirstSetting();
            init = true;
        }
    }

    public void FirstSetting()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    public void Setting(Action onClickPurchase, BoostData data)
    {
        GetTextMesh(Texts.NameText).text = Managers.Localize.GetText(data.nameKey);
        GetTextMesh(Texts.DescText).text = Managers.Localize.GetText(data.descriptionKey);
        GetTextMesh(Texts.PriceText).text = data.price.ToString();

        GetImage(Images.Icon).sprite = data.icon;

        GetButton(Buttons.PurchaseBtn).AddButtonEvent(() =>
        {
            onClickPurchase?.Invoke();
        });
    }
}
