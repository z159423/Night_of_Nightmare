using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine;
using System.Linq;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using DG.Tweening.Core.Easing;

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
        AutoTurretIcon1,
        AutoTurretIcon2,
        CoinSlot,
        EnergySlot
    }

    enum Texts
    {
        NameText,
        DescText,
        FreeText,
        CoinText,
        EnergyText,
        MaxText
    }

    enum Buttons
    {
        Button
    }

    Transform BedIcons, TurretIcons, AutoTurretIcon;

    bool _init = false;

    private StructureData _data;

    [SerializeField] Sprite[] btnSprites;

    private int level = 0;

    bool upgrade = false;

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

    public void Setting(StructureData data, Action onPurcahse, int level, bool upgrade = false)
    {
        _data = data;
        this.upgrade = upgrade;
        this.level = level;

        GetTextMesh(Texts.NameText).text = $"[{GetName()}]";
        GetTextMesh(Texts.DescText).text = GetDesc();
        SetIcon();

        GetButton(Buttons.Button).AddButtonEvent(() =>
        {
            onPurcahse?.Invoke();
            return;
        });

        UpdateUI();
    }

    void UpdateUI()
    {
        if (_data.upgradeCoin.Length <= level && _data.upgradeEnergy.Length <= level)
        {
            GetButton(Buttons.Button).gameObject.SetActive(false);
            return;
        }

        if (_data.onlyOnePurcahse && Managers.Game.playerData.GetStructure(_data.structureType) != null)
        {
            GetTextMesh(Texts.MaxText).gameObject.SetActive(true);
            GetTextMesh(Texts.CoinText).gameObject.SetActive(false);
            GetTextMesh(Texts.EnergyText).gameObject.SetActive(false);
            GetTextMesh(Texts.FreeText).gameObject.SetActive(false);
            return;
        }

        if (_data.structureType == Define.StructureType.Lamp && Managers.Game.playerData.buyLampCount >= 4)
        {
            GetTextMesh(Texts.MaxText).gameObject.SetActive(true);
            GetTextMesh(Texts.CoinText).gameObject.SetActive(false);
            GetTextMesh(Texts.EnergyText).gameObject.SetActive(false);
            GetTextMesh(Texts.FreeText).gameObject.SetActive(false);
            return;
        }

        if (_data.upgradeCoin.Length > 0 && _data.GetPurchaseCoin(level, Managers.Game.playerData) > 0)
        {
            GetImage(Images.CoinSlot).gameObject.SetActive(true);
            GetTextMesh(Texts.CoinText).text = _data.GetPurchaseCoin(level, Managers.Game.playerData).ToString();
        }
        else
            GetImage(Images.CoinSlot).gameObject.SetActive(false);


        if (_data.upgradeEnergy.Length > 0 && _data.GetPurchaseEnergy(level, Managers.Game.playerData) > 0)
        {
            GetImage(Images.EnergySlot).gameObject.SetActive(true);
            GetTextMesh(Texts.EnergyText).text = _data.GetPurchaseEnergy(level, Managers.Game.playerData).ToString();
        }
        else
            GetImage(Images.EnergySlot).gameObject.SetActive(false);

        if (_data.CanPurchase(Managers.Game.playerData, level, upgrade))
        {
            GetButton(Buttons.Button).GetComponent<Image>().sprite = btnSprites[0]; // 활성화된 버튼 이미지
        }
        else
        {
            GetButton(Buttons.Button).GetComponent<Image>().sprite = btnSprites[1]; // 비활성화된 버튼 이미지
        }

        if (upgrade ? false : Define.IsFreeStructure(Managers.Game.playerData, _data.structureType))
        {
            GetTextMesh(Texts.FreeText).gameObject.SetActive(true);
            GetImage(Images.CoinSlot).gameObject.SetActive(false);
            GetImage(Images.EnergySlot).gameObject.SetActive(false);
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
        AutoTurretIcon = gameObject.FindRecursive("AutoTurretIcon").transform;
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
        else if (_data.structureType == Define.StructureType.CopperOre || _data.structureType == Define.StructureType.SilverOre || _data.structureType == Define.StructureType.GoldOre || _data.structureType == Define.StructureType.DiamondOre)
        {
            var name = _data.nameKey;
            int index = 1;

            switch (_data.structureType)
            {
                case Define.StructureType.SilverOre:
                    index += 1;
                    break;

                case Define.StructureType.GoldOre:
                    index += 2;
                    break;

                case Define.StructureType.DiamondOre:
                    index += 2;
                    break;
            }

            return Managers.Localize.GetText(name + "_" + (level + index));
        }
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
            case Define.StructureType.CopperOre:
            case Define.StructureType.SilverOre:
            case Define.StructureType.GoldOre:
            case Define.StructureType.DiamondOre:
            case Define.StructureType.FlowerPot:
            case Define.StructureType.LushFlowerPot:
            case Define.StructureType.Sheep:
            case Define.StructureType.GoldenFrog:
            case Define.StructureType.SilverMirror:
            case Define.StructureType.GoldenMirror:
                desc = Managers.Localize.GetDynamicText(_data.descriptionKey, _data.argment1[level].ToString());
                break;

            case Define.StructureType.Axe:
            case Define.StructureType.GoldenAxe:
                desc = Managers.Localize.GetDynamicText(_data.descriptionKey, _data.argment1[level].ToString(), _data.argment2[level].ToString());
                break;

            case Define.StructureType.AutoTurret:
            case Define.StructureType.GoldenTurret:
                desc = Managers.Localize.GetDynamicText(_data.descriptionKey, _data.argment1[level].ToString()) + "<br>" + Managers.Localize.GetDynamicText("global.str_desc_atk_range", "10.5");
                break;

            case Define.StructureType.Lamp:
                desc = Managers.Localize.GetText(_data.descriptionKey) + "<br>" + Managers.Localize.GetDynamicText("global.str_owned", Managers.LocalData.PlayerLampCount.ToString());
                break;

            default:
                desc = Managers.Localize.GetText(_data.descriptionKey);
                break;
        }

        if (_data.requireStructures != null && _data.requireStructures.Length > 0 && _data.requireStructures.Length >= level
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
        AutoTurretIcon.gameObject.SetActive(false);

        if (_data.structureType == Define.StructureType.Turret)
        {
            TurretIcons.gameObject.SetActive(true);
            GetImage(Images.TurretIcon1).sprite = _data.sprite1[level];
            GetImage(Images.TurretIcon1).SetNativeSize();
            GetImage(Images.TurretIcon2).sprite = _data.sprite2[level];
            GetImage(Images.TurretIcon2).SetNativeSize();
        }
        else if (_data.structureType == Define.StructureType.AutoTurret || _data.structureType == Define.StructureType.GoldenTurret)
        {
            AutoTurretIcon.gameObject.SetActive(true);
            GetImage(Images.AutoTurretIcon1).sprite = _data.sprite1[0];
            GetImage(Images.AutoTurretIcon2).sprite = _data.sprite2[0];
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
        else if (_data.structureType == Define.StructureType.Generator || _data.structureType == Define.StructureType.CopperOre || _data.structureType == Define.StructureType.SilverOre || _data.structureType == Define.StructureType.GoldOre)
        {
            GetImage(Images.DefaultIcon).gameObject.SetActive(true);
            GetImage(Images.DefaultIcon).sprite = _data.sprite1[level];
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
