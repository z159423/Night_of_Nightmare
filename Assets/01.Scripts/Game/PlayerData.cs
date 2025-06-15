using System;
using System.Collections.Generic;
using System.Linq;
using static Define;

[System.Serializable]
public class PlayerData
{
    public CharactorType type;

    public int coin;
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
        return structures.First(s => s.type == type);
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
