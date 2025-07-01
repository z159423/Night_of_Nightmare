using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

public class Structure_Popup : UI_Popup
{
    public enum TapTypes
    {
        Basic,
        Ore,
        Guard,
        Trap,
        Buff,
        Lamp
    }

    enum Buttons
    {
        BasicBtn,
        OreBtn,
        GuardBtn,
        TrapBtn,
        BuffBtn,
        ExitBtn,
        LampBtn
    }

    enum Images
    {
        TouchGuard
    }

    enum Texts
    {
        BasicText,
        OreText,
        GuardText,
        TrapText,
        BuffText,
        LampText
    }

    private VerticalLayoutGroup layout;

    private TapTypes currentTapType = TapTypes.Buff;

    [SerializeField]
    private Color[] tapColors;

    private Tile selectedTile;


    public override void Init()
    {
        base.Init();

        SelectTap(TapTypes.Basic);

        OpenAnimation();
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        layout = gameObject.FindRecursive("Layout").GetComponent<VerticalLayoutGroup>();

        GetButton(Buttons.BasicBtn).AddButtonEvent(() => SelectTap(TapTypes.Basic));
        GetButton(Buttons.OreBtn).AddButtonEvent(() => SelectTap(TapTypes.Ore));
        GetButton(Buttons.GuardBtn).AddButtonEvent(() => SelectTap(TapTypes.Guard));
        GetButton(Buttons.TrapBtn).AddButtonEvent(() => SelectTap(TapTypes.Trap));
        GetButton(Buttons.BuffBtn).AddButtonEvent(() => SelectTap(TapTypes.Buff));
        GetButton(Buttons.LampBtn).AddButtonEvent(() => SelectTap(TapTypes.Lamp));

        GetButton(Buttons.ExitBtn).onClick.AddListener(Exit);
    }

    public void SelectTap(TapTypes tapType)
    {
        if (currentTapType == tapType)
            return;

        // 이전 탭 텍스트 스타일 초기화
        GetTextMesh((Texts)(int)currentTapType).fontSizeMax = 60;
        GetTextMesh((Texts)(int)currentTapType).color = tapColors[1];

        currentTapType = tapType;

        // 선택된 탭 텍스트 스타일 적용
        GetTextMesh((Texts)(int)currentTapType).fontSizeMax = 72;
        GetTextMesh((Texts)(int)currentTapType).color = tapColors[0];

        foreach (Transform child in layout.transform)
        {
            Destroy(child.gameObject);
        }

        var finds = Managers.Resource.LoadAll<StructureData>($"StructureData/{(TapTypes)(int)currentTapType}").Where(n => !n.baseStructure);

        // Enum 순서대로 정렬
        var sortedFinds = finds.OrderBy(data => (int)data.structureType).ToList();

        foreach (var data in sortedFinds)
        {
            var slot = Managers.Resource.Instantiate("StructureSlot", layout.transform).GetComponent<StructureSlot>();
            slot.Init();
            slot.Setting(data, () =>
            {
                Managers.Game.BuildStructure(Managers.Game.playerData, data.structureType, Managers.Game.selectedTile);

                GameObserver.Call(GameObserverType.Game.OnChangeStructure);
                Exit();
            }, 0, upgrade: false);
        }
    }

    public void Setting(Tile tile)
    {
        selectedTile = tile;
    }

    public override void Reset()
    {

    }

    private void Exit()
    {
        ClosePopupUI();
    }
}
