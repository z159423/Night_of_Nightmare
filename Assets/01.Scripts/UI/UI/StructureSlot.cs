using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine;
using System.Linq;

public class StructureSlot : UI_Base
{
    enum Images
    {
        Icon,
        CoinSlot,
        EnergySlot
    }

    enum Texts
    {
        NameText,
        DescText,
        FreeText,
        CoinText,
        EnergyText
    }

    enum Buttons
    {
        Button
    }

    bool _init = false;

    private StructureData _data;

    Action onPurcahse = null;

    [SerializeField] Sprite[] btnSprites;

    private int level = 0;

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

    public void Setting(StructureData data, Action onPurcahse, bool upgrade = false)
    {
        _data = data;

        if (upgrade)
        {
            var currentStructure = Managers.Game.playerData.GetStructure(data.structureType);
            level = currentStructure != null ? currentStructure.level : 0;
        }
        else
            level = 0;

        GetImage(Images.Icon).sprite = data.icon;
        GetImage(Images.Icon).SetNativeSize();

        GetTextMesh(Texts.NameText).text = $"[{Managers.Localize.GetText(data.nameKey)}]";
        GetTextMesh(Texts.DescText).text = Managers.Localize.GetText(data.descriptionKey);
        GetTextMesh(Texts.CoinText).text = data.upgradeCoin.Length > 0 ? data.upgradeCoin[level].ToString() : Managers.Localize.GetText("global.str_free");

        this.onPurcahse = onPurcahse;

        GetButton(Buttons.Button).AddButtonEvent(() =>
        {
            if (_data.upgradeCoin[level] <= Managers.Game.playerData.coin && _data.upgradeEnergy[level] <= Managers.Game.playerData.energy)
            {
                onPurcahse?.Invoke();
                return;
            }
        });

        UpdateUI();
    }

    void UpdateUI()
    {
        if (_data.upgradeCoin.Length > 0 && _data.upgradeCoin[level] > 0)
        {
            GetImage(Images.CoinSlot).gameObject.SetActive(true);
            GetTextMesh(Texts.CoinText).text = _data.upgradeCoin[0].ToString();
        }
        else
            GetImage(Images.CoinSlot).gameObject.SetActive(false);


        if (_data.upgradeEnergy.Length > 0 && _data.upgradeEnergy[level] > 0)
        {
            GetImage(Images.EnergySlot).gameObject.SetActive(true);
            GetTextMesh(Texts.EnergyText).text = _data.upgradeEnergy[level].ToString();
        }
        else
            GetImage(Images.EnergySlot).gameObject.SetActive(false);

        if ((_data.upgradeCoin.Length > 0 ? (Managers.Game.playerData.coin >= _data.upgradeCoin[level]) : true)
         && (_data.upgradeEnergy.Length > 0 ? (Managers.Game.playerData.energy >= _data.upgradeEnergy[level]) : true))
        {
            GetButton(Buttons.Button).GetComponent<Image>().sprite = btnSprites[0]; // 활성화된 버튼 이미지
        }
        else
        {
            GetButton(Buttons.Button).GetComponent<Image>().sprite = btnSprites[1]; // 비활성화된 버튼 이미지
        }

        // if (Managers.Game.coin <= _data.upgradeCoin)
        //     GetButton(Buttons.Button)
    }

    public void FirstSetting()
    {
        _init = true;

        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Button>(typeof(Buttons));
    }

    void OnDisable()
    {
        this.RemoveListener(GameObserverType.Game.OnChangeCoinCount);
        this.RemoveListener(GameObserverType.Game.OnChangeEnergyCount);
    }
}
