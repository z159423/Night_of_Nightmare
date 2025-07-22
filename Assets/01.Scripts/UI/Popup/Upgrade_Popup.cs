using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

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
        SellCoinSlot,
        SellEnergySlot
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

        GetButton(Buttons.ExitBtn).onClick.AddListener(Exit);
    }

    public void Setting(Structure structure)
    {
        selectedStructure = structure;

        var data = Managers.Game.GetStructureData(structure.type);

        if (data.baseStructure)
        {
            GetButton(Buttons.SellBtn).gameObject.SetActive(false);
            GetImage(Images.SellSlot).gameObject.SetActive(false);
        }
        else
        {
            GetButton(Buttons.SellBtn).gameObject.SetActive(true);
            GetImage(Images.SellSlot).gameObject.SetActive(true);

            int sellCoin = data.GetSellCoin(structure.level);
            GetImage(Images.SellCoinSlot).gameObject.SetActive(sellCoin > 0);

            int sellEnergy = data.GetSellEnergy(structure.level);
            GetImage(Images.SellEnergySlot).gameObject.SetActive(sellEnergy > 0);

            GetTextMesh(Texts.SellCoinCount).text = sellCoin.ToString();
            GetTextMesh(Texts.SellEnergyText).text = sellEnergy.ToString();

            GetButton(Buttons.SellBtn).AddButtonEvent(() => { Managers.Game.playerData.SellStructure(structure); Exit(); });
        }

        int level = structure.level + 1;

        var upgradeSlot = gameObject.FindRecursive("UpgradeSlot").GetComponent<StructureSlot>();

        upgradeSlot.Init();
        upgradeSlot.Setting(data, () =>
        {
            if (data.CanUpgrade(Managers.Game.playerData, level, out string reason))
            {
                Managers.Game.playerData.UseResource(data.GetPurchaseCoin(level, Managers.Game.playerData), data.GetPurchaseEnergy(level, Managers.Game.playerData));
                selectedStructure.Upgrade();

                GameObserver.Call(GameObserverType.Game.OnChangeStructure);
                Exit();
            }
        }, level, true, thisStructure: structure, Exit);

        if (data.canRvUpgrade)
        {
            var rvUpgradeSlot = gameObject.FindRecursive("RVUpgradeSlot").GetComponent<StructureSlot>();

            rvUpgradeSlot.gameObject.SetActive(true);

            rvUpgradeSlot.Init();
            rvUpgradeSlot.RVUpgradeSetting(data, () =>
            {
                // //TODO: 광고 
                // if (data.RVUpgradeTo != Define.StructureType.None)
                // {
                //     var structure = Managers.Game.BuildStructure(Managers.Game.playerData, data.RVUpgradeTo, Managers.Game.selectedTile);

                //     structure.UpgradeTo(level);

                //     if (structure != null)
                //         structure.DestroyStructure();
                // }
                // else
                //     selectedStructure.Upgrade();

                // GameObserver.Call(GameObserverType.Game.OnChangeStructure);

                Exit();
            }, level, thisStructure: structure);
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
