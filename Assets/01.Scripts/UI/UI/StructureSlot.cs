using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEditor.Localization.Plugins.XLIFF.V20;
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

    private StructureData _data;

    public override void Init()
    {
        if (!_init)
        {
            FirstSetting();
        }

        this.SetListener(GameObserverType.Game.OnChangeCoinCount, () =>
        {
            UpdateUI();
        });

        this.SetListener(GameObserverType.Game.OnChangeEnergyCount, () =>
        {
            UpdateUI();
        });
    }

    public void Setting(StructureData data)
    {
        _data = data;

        GetImage(Images.Icon).sprite = data.icon;
        GetImage(Images.Icon).SetNativeSize();
        
        GetTextMesh(Texts.NameText).text = Managers.Localize.GetText(data.nameKey);
        GetTextMesh(Texts.DescText).text = Managers.Localize.GetText(data.descriptionKey);
        GetTextMesh(Texts.ValueText).text = data.upgradeCoin.Length > 0 ? data.upgradeCoin[0].ToString() : Managers.Localize.GetText("global.str_free");

        UpdateUI();
    }

    void UpdateUI()
    {
        // if (Managers.Game.coin <= _data.upgradeCoin)
        //     GetButton(Buttons.Button)
    }

    public void FirstSetting()
    {
        _init = true;

        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    void OnDisable()
    {
        this.RemoveListener(GameObserverType.Game.OnChangeCoinCount);
        this.RemoveListener(GameObserverType.Game.OnChangeEnergyCount);
    }
}
