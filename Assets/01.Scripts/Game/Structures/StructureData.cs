using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[CreateAssetMenu(fileName = "StructureData", menuName = "Scriptable Object/StructureData", order = int.MaxValue)]
public class StructureData : ScriptableObject
{
    public StructureCategory category;

    public StructureType structureType;

    public Sprite icon;

    public float lampProp;
    public bool baseStructure = false;
    public bool sellable = true;
    public bool onlyOnePurcahse = false;

    public string nameKey;
    public string descriptionKey;

    public int[] upgradeCoin;
    public int[] upgradeEnergy;
    public int purcahseLimit = -1;

    public float[] argment1;
    public float[] argment2;

    public RequireStructure[] requireStructures;

    public Sprite[] sprite1;
    public Sprite[] sprite2;

    public int GetPurchaseCoin(int level, CharactorType charactorType)
    {
        if (charactorType == CharactorType.Miner && category == StructureCategory.Ore)
        {
            // 광부 캐릭터가 오레 구조물을 구매할 때는 코인을 10% 할인
            return Mathf.RoundToInt(upgradeCoin[level] * 0.9f);
        }
        else if (charactorType == CharactorType.Scientist && structureType == StructureType.Generator)
        {
            return Mathf.RoundToInt(upgradeCoin[level] * 0.9f);
        }


        return upgradeCoin[level];
    }

    public int GetPurchaseEnergy(int level, CharactorType charactorType)
    {
        if (charactorType == CharactorType.Miner && category == StructureCategory.Ore)
        {
            // 광부 캐릭터가 오레 구조물을 구매할 때는 에너지를 10% 할인
            return Mathf.RoundToInt(upgradeEnergy[level] * 0.9f);
        }
        else if (charactorType == CharactorType.Scientist && structureType == StructureType.Generator)
        {
            return Mathf.RoundToInt(upgradeEnergy[level] * 0.9f);
        }

        return upgradeEnergy[level];
    }

    public int GetSellCoin(int level)
    {
        return upgradeCoin[level] / 4;
    }

    public int GetSellEnergy(int level)
    {
        return upgradeEnergy[level] / 4;
    }

    public bool CanPurchase(PlayerData playerData, int level, bool upgrade = false)
    {
        // 무료 구조물은 항상 구매 가능
        if (!upgrade && IsFreeStructure(playerData, structureType))
            return true;

        // 이미 하나만 구매 가능한 구조물이고 이미 소유 중이면 구매 불가
        if (onlyOnePurcahse && playerData.GetStructure(structureType) != null)
            return false;

        // 요구 구조물 조건 체크
        if (requireStructures != null && requireStructures.Length > 0 && requireStructures.Length > level
            && requireStructures[level].type != structureType)
        {
            var currentRequire = playerData.GetStructure(requireStructures[level].type);
            if (currentRequire == null || currentRequire.level < requireStructures[level].level)
                return false;
        }

        // 자원 조건 체크
        bool coinOk = upgrade ? (upgradeCoin.Length > level && playerData.coin >= GetPurchaseCoin(level, playerData.type))
                              : (upgradeCoin.Length > level && playerData.coin >= GetPurchaseCoin(level, playerData.type));
        bool energyOk = upgrade ? (upgradeEnergy.Length > level && playerData.energy >= GetPurchaseEnergy(level, playerData.type))
                                : (upgradeEnergy.Length > level && playerData.energy >= GetPurchaseEnergy(level, playerData.type));

        if (structureType == StructureType.Lamp && Managers.LocalData.PlayerLampCount <= 0)
            return false;

        return coinOk && energyOk;
    }

    public bool CanUpgrade(PlayerData playerData, int level)
    {
        // 업그레이드가 가능한지: 배열 범위 내에 있는지 체크
        if (upgradeCoin == null || upgradeEnergy == null)
            return false;
        if ((level) >= upgradeCoin.Length || (level) >= upgradeEnergy.Length)
            return false;

        // 요구 구조물 조건 체크
        if (requireStructures != null && requireStructures.Length > 0 && requireStructures.Length > level
            && requireStructures[level].type != structureType)
        {
            var currentRequire = playerData.GetStructure(requireStructures[level].type);
            if (currentRequire == null || currentRequire.level < requireStructures[level].level)
                return false;
        }

        // 자원 조건 체크
        bool coinOk = playerData.coin >= GetPurchaseCoin(level, playerData.type);
        bool energyOk = playerData.energy >= GetPurchaseEnergy(level, playerData.type);

        return coinOk && energyOk;
    }
}

[System.Serializable]
public class RequireStructure
{
    public StructureType type;
    public int level;
}