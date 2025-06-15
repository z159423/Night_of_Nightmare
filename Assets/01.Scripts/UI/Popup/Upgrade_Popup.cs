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
        TouchGuard
    }

    enum Texts
    {
        SellCoinCount
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

            int sellCoin = data.upgradeCoin[currentStructure.level] / 4;
            GetTextMesh(Texts.SellCoinCount).text = sellCoin.ToString();
            GetButton(Buttons.SellBtn).AddButtonEvent(() => { Managers.Game.playerData.SellStructure(structure); Exit(); });
        }

        var upgradeSlot = GetComponentInChildren<StructureSlot>();

        if (data.upgradeCoin.Length < 2 && data.upgradeEnergy.Length < 2)
        {
            upgradeSlot.gameObject.SetActive(false);
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
            }, true, structure);
        }
    }

    public override void Reset()
    {

    }

    private void Exit()
    {
        ClosePopupUI();
    }
}
