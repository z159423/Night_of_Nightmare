using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine;
using System.Linq;
using UnityEditor.Localization.Plugins.XLIFF.V20;

public class StructureSlot : UI_Base
{
    enum Images
    {
        DefaultIcon,
        BedIcon1,
        BedIcon2,
        BedIcon3,
        TurretIcon1,
        TurretIcon2,
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

    Transform BedIcons, TurretIcons;

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

    public void Setting(StructureData data, Action onPurcahse, bool upgrade = false, Structure selectedStructure = null)
    {
        _data = data;

        if (upgrade)
        {
            var currentStructure = selectedStructure;
            level = currentStructure != null ? currentStructure.level + 1 : 0;
        }
        else
            level = 0;

        GetTextMesh(Texts.NameText).text = GetName();
        GetTextMesh(Texts.DescText).text = GetDesc();
        SetIcon();

        GetTextMesh(Texts.CoinText).text = data.upgradeCoin.Length > 0 ? data.upgradeCoin[level].ToString() : Managers.Localize.GetText("global.str_free");

        this.onPurcahse = onPurcahse;

        GetButton(Buttons.Button).AddButtonEvent(() =>
        {
            if (CheckIsReqired() && _data.upgradeCoin[level] <= Managers.Game.playerData.coin && _data.upgradeEnergy[level] <= Managers.Game.playerData.energy)
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
            GetTextMesh(Texts.CoinText).text = _data.upgradeCoin[level].ToString();
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

        if (CheckIsReqired() && (_data.upgradeCoin.Length > 0 ? (Managers.Game.playerData.coin >= _data.upgradeCoin[level]) : true)
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

        BedIcons = gameObject.FindRecursive("BedIcons").transform;
        TurretIcons = gameObject.FindRecursive("TurretIcon").transform;
    }

    void OnDisable()
    {
        this.RemoveListener(GameObserverType.Game.OnChangeCoinCount);
        this.RemoveListener(GameObserverType.Game.OnChangeEnergyCount);
    }

    public string GetName()
    {
        if (_data.structureType == Define.StructureType.Turret || _data.structureType == Define.StructureType.Bed || _data.structureType == Define.StructureType.Door)
            return Managers.Localize.GetText(_data.nameKey + "_" + (level + 1));
        else
            return Managers.Localize.GetText(_data.nameKey);
    }

    public string GetDesc()
    {
        string desc = "";
        switch (_data.structureType)
        {
            case Define.StructureType.Turret:
            case Define.StructureType.Bed:
            case Define.StructureType.Door:
            case Define.StructureType.Generator:
                desc = Managers.Localize.GetDynamicText(_data.descriptionKey, _data.argment1[level].ToString());
                break;
            default:
                desc = Managers.Localize.GetText(_data.descriptionKey);
                break;
        }

        if (_data.requireStructures != null &&_data.requireStructures.Length > 0 && _data.requireStructures.Length >= level
          && _data.requireStructures[level].type != _data.structureType)
        {
            var require = Managers.Game.GetStructureData(_data.requireStructures[level].type);
            var currentRequire = Managers.Game.playerData.GetStructure(_data.requireStructures[level].type);

            bool isMet = currentRequire != null && currentRequire.level >= _data.requireStructures[level].level;

            desc += "\n" + (isMet ? "<color=green>" : "<color=red>") + Managers.Localize.GetDynamicText("global.str_desc_need_object", Managers.Localize.GetText(require.nameKey + "_" + (_data.requireStructures[level].level + 1))) + "</color>";
        }
        return desc;
    }

    public void SetIcon()
    {
        GetImage(Images.DefaultIcon).gameObject.SetActive(false);
        BedIcons.gameObject.SetActive(false);
        TurretIcons.gameObject.SetActive(false);

        if (_data.structureType == Define.StructureType.Turret)
        {
            TurretIcons.gameObject.SetActive(true);
            GetImage(Images.TurretIcon1).sprite = _data.sprite1[level];
            GetImage(Images.TurretIcon2).sprite = _data.sprite2[level];
        }
        else if (_data.structureType == Define.StructureType.Bed)
        {
            BedIcons.gameObject.SetActive(true);
            GetImage(Images.BedIcon1).sprite = _data.sprite2[level];
            GetImage(Images.BedIcon2).sprite = Managers.Resource.GetCharactorImage((int)Managers.Game.playerCharactor.charactorType + 1);
            GetImage(Images.BedIcon3).sprite = _data.sprite1[level];
        }
        else if (_data.structureType == Define.StructureType.Door)
        {
            GetImage(Images.DefaultIcon).gameObject.SetActive(true);
            GetImage(Images.DefaultIcon).sprite = _data.sprite1[level];
            GetImage(Images.DefaultIcon).SetNativeSize();

            GetImage(Images.DefaultIcon).transform.localScale = Vector3.one * 1.35f;
        }
        else if (_data.structureType == Define.StructureType.Generator)
        {
            GetImage(Images.DefaultIcon).gameObject.SetActive(true);
            GetImage(Images.DefaultIcon).sprite = _data.sprite1[level];
            GetImage(Images.DefaultIcon).SetNativeSize();

            GetImage(Images.DefaultIcon).transform.localScale = Vector3.one * 1.8f;
        }
        else
        {
            GetImage(Images.DefaultIcon).gameObject.SetActive(true);
            GetImage(Images.DefaultIcon).sprite = _data.icon;
            GetImage(Images.DefaultIcon).SetNativeSize();

            GetImage(Images.DefaultIcon).transform.localScale = Vector3.one * 1.8f;
        }
    }

    public bool CheckIsReqired()
    {
        bool isMet = true;
        if (_data.requireStructures != null && _data.requireStructures.Length > 0 && _data.requireStructures.Length >= level
          && _data.requireStructures[level].type != _data.structureType)
        {
            var currentRequire = Managers.Game.playerData.GetStructure(_data.requireStructures[level].type);

            isMet = currentRequire != null && currentRequire.level >= _data.requireStructures[level].level;
        }

        return isMet;
    }
}
