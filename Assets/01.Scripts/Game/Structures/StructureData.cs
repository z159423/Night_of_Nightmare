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
}

[System.Serializable]
public class RequireStructure
{
    public StructureType type;
    public int level;
}