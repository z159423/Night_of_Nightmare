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

    BoostData data;

    [SerializeField] Sprite[] btnSprites;

    public override void Init()
    {
        if (!init)
        {
            FirstSetting();
            init = true;
        }

        this.SetListener(GameObserverType.Game.OnChangeBoostItemCount, () =>
        {
            UpdateUI();
        });

        this.SetListener(GameObserverType.Game.OnChangeGemCount, () =>
        {
            UpdateUI();
        });


    }

    public void FirstSetting()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    public void Setting(Action onClickPurchase, BoostData data)
    {
        this.data = data;
        GetTextMesh(Texts.NameText).text = Managers.Localize.GetText(data.nameKey);
        GetTextMesh(Texts.DescText).text = GetDesc();
        GetTextMesh(Texts.PriceText).text = data.price.ToString();
        GetImage(Images.Icon).sprite = data.icon;

        GetButton(Buttons.PurchaseBtn).AddButtonEvent(() =>
        {
            if (Managers.LocalData.PlayerGemCount >= data.price)
                onClickPurchase?.Invoke();
        });

        UpdateUI();
    }

    void UpdateUI()
    {
        GetButton(Buttons.PurchaseBtn).GetComponent<Image>().sprite = Managers.LocalData.PlayerGemCount >= data.price ? btnSprites[0] : btnSprites[1];
        GetTextMesh(Texts.CountText).text = Managers.LocalData.GetBoostItemCount(data.type).ToString();
    }

    public string GetDesc()
    {
        switch (data.type)
        {
            case Define.BoostType.HolyProtection:
                return Managers.Localize.GetDynamicText(data.descriptionKey, data.argment1[0].ToString());
            case Define.BoostType.HammerThrow:
                return Managers.Localize.GetDynamicText(data.descriptionKey, data.argment1[0].ToString(), data.argment1[1].ToString());
            case Define.BoostType.Overheat:
                return Managers.Localize.GetDynamicText(data.descriptionKey, data.argment1[0].ToString());

            default:
                return Managers.Localize.GetText(data.descriptionKey);
        }
    }
}
