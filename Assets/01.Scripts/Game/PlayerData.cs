using System;
using System.Collections.Generic;
using System.Linq;
using static Define;

[System.Serializable]
public class PlayerData
{
    public CharactorType type;

    public int coin = 10;
    public int energy;

    public Room room;
    public List<Structure> structures = new List<Structure>();

    public PlayerData(CharactorType type)
    {
        this.type = type;
    }

    public void BuildStructure(Structure structure)
    {
        if (structures.Contains(structure))
            return;

        structures.Add(structure);
        structure.playerData = this;
    }

    public void UpgradeStructure(Structure structure)
    {
        if (!structures.Contains(structure))
            return;

        structure.Upgrade();
    }

    public void GetResources()
    {
        int coinValue = room == null ? (int)Managers.Game.GetStructureData(Define.StructureType.Bed).argment1[0] : (int)Managers.Game.GetStructureData(Define.StructureType.Bed).argment1[room.bed.level];

        if (room != null && room.bed != null)
            room.bed.ResourceGetParticle(coinValue);

        foreach (var generator in structures.Where(s => s.type == Define.StructureType.Generator))
        {
            generator.GetComponent<Generator>().ResourceGetParticle((int)Managers.Game.GetStructureData(Define.StructureType.Generator).argment1[generator.level]);
        }

        var energy = structures.Where(s => s.type == Define.StructureType.Generator).Count();
        var coin = coinValue;

        var ores = structures.Where(s =>
            s.type == Define.StructureType.CopperOre ||
            s.type == Define.StructureType.SilverOre ||
            s.type == Define.StructureType.GoldOre ||
            s.type == Define.StructureType.DiamondOre
        ).ToList();

        foreach (var ore in ores)
        {
            if (ore.type == Define.StructureType.CopperOre)
            {
                coin += (int)Managers.Game.GetStructureData(Define.StructureType.CopperOre).argment1[ore.level];
            }
            else if (ore.type == Define.StructureType.SilverOre)
            {
                coin += (int)Managers.Game.GetStructureData(Define.StructureType.SilverOre).argment1[ore.level];

            }
            else if (ore.type == Define.StructureType.GoldOre)
            {
                coin += (int)Managers.Game.GetStructureData(Define.StructureType.GoldOre).argment1[ore.level];

            }
            else if (ore.type == Define.StructureType.DiamondOre)
            {
                coin += (int)Managers.Game.GetStructureData(Define.StructureType.DiamondOre).argment1[ore.level];
            }

            ore.GetComponent<Ore>().ResourceGetParticle((int)Managers.Game.GetStructureData(ore.type).argment1[ore.level]);
        }

        AddCoin(coin);
        AddEnergy(energy);
    }

    public void UseResource(int coin, int energy)
    {
        this.coin -= coin;
        this.energy -= energy;

        GameObserver.Call(GameObserverType.Game.OnChangeCoinCount);
        GameObserver.Call(GameObserverType.Game.OnChangeEnergyCount);
    }

    public void AddCoin(int coin)
    {
        this.coin += coin;

        GameObserver.Call(GameObserverType.Game.OnChangeCoinCount);
    }

    public void AddEnergy(int energy)
    {
        this.energy += energy;

        GameObserver.Call(GameObserverType.Game.OnChangeEnergyCount);
    }

    public Structure GetStructure(Define.StructureType type)
    {
        return structures.FirstOrDefault(s => s.type == type);
    }

    public void SellStructure(Structure structure)
    {
        if (!structures.Contains(structure))
            return;

        structure.GetComponentInParent<Tile>().currentStructure = null;
        structure.DestroyStructure();
        structures.Remove(structure);
        coin += structure.GetSellValue();
    }
}
