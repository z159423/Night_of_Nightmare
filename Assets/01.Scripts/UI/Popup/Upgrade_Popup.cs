using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
using UnityEditor.Localization.Plugins.XLIFF.V20;

public class Upgrade_Popup : UI_Popup
{

    enum Buttons
    {
        ExitBtn,
        SellBtn
    }

    enum Images
    {
        SellSlot,
        TouchGuard,
        CoinSlot,
        EnergySlot
    }

    enum Texts
    {
        SellCoinCount,
        SellEnergyText
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

        var data = Managers.Game.GetStructureData(structure.type);
        var currentStructure = Managers.Game.playerData.GetStructure(structure.type);

        if (data.baseStructure)
        {
            GetButton(Buttons.SellBtn).gameObject.SetActive(false);
            GetImage(Images.SellSlot).gameObject.SetActive(false);
        }
        else
        {
            GetButton(Buttons.SellBtn).gameObject.SetActive(true);
            GetImage(Images.SellSlot).gameObject.SetActive(true);

            int sellCoin = data.GetSellCoin(currentStructure.level);
            GetImage(Images.CoinSlot).gameObject.SetActive(sellCoin > 0);

            int sellEnergy = data.GetSellEnergy(currentStructure.level);
            GetImage(Images.EnergySlot).gameObject.SetActive(sellEnergy > 0);

            GetTextMesh(Texts.SellCoinCount).text = sellCoin.ToString();
            GetTextMesh(Texts.SellEnergyText).text = sellEnergy.ToString();

            GetButton(Buttons.SellBtn).AddButtonEvent(() => { Managers.Game.playerData.SellStructure(structure); Exit(); });
        }

        var upgradeSlot = GetComponentInChildren<StructureSlot>();

        // 업그레이드가 가능한지 확인: 현재 레벨 + 1이 배열 범위 내에 있는지 체크
        bool canUpgrade =
            data.upgradeCoin != null && data.upgradeEnergy != null &&
            (structure.level + 1) < data.upgradeCoin.Length &&
            (structure.level + 1) < data.upgradeEnergy.Length;

        if (!canUpgrade)
        {
            // upgradeSlot.gameObject.SetActive(false);

            upgradeSlot.Init();
            upgradeSlot.Setting(data, null, structure.level, structure);
        }
        else
        {
            upgradeSlot.Init();
            upgradeSlot.Setting(data, () =>
            {
                Managers.Game.playerData.UseResource(data.upgradeCoin[structure.level + 1], data.upgradeEnergy[structure.level + 1]);
                selectedStructure.Upgrade();

                GameObserver.Call(GameObserverType.Game.OnChangeStructure);

                Exit();
            }, structure.level + 1, structure);
        }

        OpenAnimation();
    }

    public override void Reset()
    {

    }

    private void Exit()
    {
        ClosePopupUI();
    }
}
