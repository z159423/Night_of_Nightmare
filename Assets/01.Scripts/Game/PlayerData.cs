using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

[System.Serializable]
public class PlayerData
{
    public CharactorType type;

    public int coin = 10;
    public int energy;

    public Room room;
    public List<Structure> structures = new List<Structure>();

    public int freeTurretCount = 0;
    public int freeRepaireStationCount = 0;
    public int freeLampCount = 0;

    public int buyLampCount = 0;

    public bool canDoorRepair = true;

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

        var deadPlayerCount = Managers.Game.charactors.Count(n => n.die);

        foreach (var sheep in structures.Where(n => n.type == Define.StructureType.Sheep).ToList())
        {
            sheep.GetComponent<Sheep>().ResourceGetParticle((int)Managers.Game.GetStructureData(StructureType.Sheep).argment1[deadPlayerCount]);
            coin += (int)Managers.Game.GetStructureData(StructureType.Sheep).argment1[deadPlayerCount];
        }

        foreach (var sheep in structures.Where(n => n.type == Define.StructureType.Grave).ToList())
        {
            sheep.GetComponent<Grave>().ResourceGetParticle((int)Managers.Game.GetStructureData(StructureType.Grave).argment1[0]);
            coin += (int)Managers.Game.GetStructureData(StructureType.Grave).argment1[0];
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
        coin += Managers.Game.GetStructureData(structure.type).GetSellCoin(structure.level);
        energy += Managers.Game.GetStructureData(structure.type).GetSellEnergy(structure.level);
    }

    public void AddFreeCount(StructureType type)
    {
        switch (type)
        {
            case StructureType.Turret:
                freeTurretCount++;
                break;
            case StructureType.RepairStation:
                freeRepaireStationCount++;
                break;
            case StructureType.Lamp:
                freeLampCount++;
                break;
        }
    }

    public void SelfRepairDoor()
    {
        Managers.Game.StartCoroutine(coolTime());

        IEnumerator coolTime()
        {
            if (canDoorRepair)
            {
                canDoorRepair = false;
                room.door.AddEffect(new SelfDoorRepair(5));

                yield return new WaitForSeconds(20);
                canDoorRepair = true;
            }
        }
    }
}
