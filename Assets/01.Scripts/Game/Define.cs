using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum CharactorType
    {
        Farmer = 0,
        Miner = 1,
        LampGirl = 2,
        ReapireMan = 3,
        Scientist = 4,
        Chef = 5
    }

    public enum Tier
    {
        Iron4,
        Iron3,
        Iron2,
        Iron1,
        Bronze4,
        Bronze3,
        Bronze2,
        Bronze1,
        Silver4,
        Silver3,
        Silver2,
        Silver1,
        Gold4,
        Gold3,
        Gold2,
        Gold1,
        Platinum4,
        Platinum3,
        Platinum2,
        Platinum1,
        Emerald4,
        Emerald3,
        Emerald2,
        Emerald1,
        Diamond4,
        Diamond3,
        Diamond2,
        Diamond1,
        Master4,
        Master3,
        Master2,
        Master1,
        GrandMaster,
        Challenger
    }

    public static Dictionary<Tier, int> TierToScore = new Dictionary<Tier, int>()
    {
        {Tier.Iron4, 0},
        {Tier.Iron3, 100},
        {Tier.Iron2, 200},
        {Tier.Iron1, 300},
        {Tier.Bronze4, 400},
        {Tier.Bronze3, 500},
        {Tier.Bronze2, 600},
        {Tier.Bronze1, 700},
        {Tier.Silver4, 800},
        {Tier.Silver3, 900},
        {Tier.Silver2, 1000},
        {Tier.Silver1, 1100},
        {Tier.Gold4, 1200},
        {Tier.Gold3, 1300},
        {Tier.Gold2, 1400},
        {Tier.Gold1, 1500},
        {Tier.Platinum4, 1600},
        {Tier.Platinum3, 1700},
        {Tier.Platinum2, 1800},
        {Tier.Platinum1, 1900},
        {Tier.Emerald4, 2000},
        {Tier.Emerald3, 2100},
        {Tier.Emerald2, 2200},
        {Tier.Emerald1, 2300},
        {Tier.Diamond4, 2400},
        {Tier.Diamond3, 2500},
        {Tier.Diamond2, 2600},
        {Tier.Diamond1, 2700},
        {Tier.Master4, 2800},
        {Tier.Master3, 3000},
        {Tier.Master2, 3200},
        {Tier.Master1, 3400},
        {Tier.GrandMaster, 3800},
        {Tier.Challenger, 4200}
    };

    public static Dictionary<Tier, Color32> TierColor = new Dictionary<Tier, Color32>()
    {
        {Tier.Iron4, new Color32(178, 178, 178, 255)},
        {Tier.Iron3, new Color32(229, 229, 229, 255)},
        {Tier.Iron2, new Color32(253, 253, 180, 255)},
        {Tier.Iron1, new Color32(148, 253, 127, 255)},
        {Tier.Bronze4, new Color32(247, 245, 103, 255)},
        {Tier.Bronze3, new Color32(213, 255, 127, 255)},
        {Tier.Bronze2, new Color32(164, 255, 75, 255)},
        {Tier.Bronze1, new Color32(138, 255, 26, 255)},
        {Tier.Silver4, new Color32(178, 223, 255, 255)},
        {Tier.Silver3, new Color32(100, 191, 254, 255)},
        {Tier.Silver2, new Color32(229, 229, 229, 255)},
        {Tier.Silver1, new Color32(50, 170, 255, 255)},
        {Tier.Gold4, new Color32(199, 178, 255, 255)},
        {Tier.Gold3, new Color32(178, 151, 254, 255)},
        {Tier.Gold2, new Color32(140, 101, 254, 255)},
        {Tier.Gold1, new Color32(254, 218, 124, 255)},
        {Tier.Platinum4, new Color32(255, 211, 102, 255)},
        {Tier.Platinum3, new Color32(255, 204, 76, 255)},
        {Tier.Platinum2, new Color32(154, 196, 50, 255)},
        {Tier.Platinum1, new Color32(255, 181, 0, 255)},
        {Tier.Emerald4, new Color32(255, 161, 100, 255)},
        {Tier.Emerald3, new Color32(255, 144, 75, 255)},
        {Tier.Emerald2, new Color32(222, 125, 10, 255)},
        {Tier.Emerald1, new Color32(255, 153, 164, 255)},
        {Tier.Diamond4, new Color32(255, 127, 140, 255)},
        {Tier.Diamond3, new Color32(254, 101, 120, 255)},
        {Tier.Diamond2, new Color32(255, 50, 73, 255)},
        {Tier.Diamond1, new Color32(159, 44, 56, 255)},
        {Tier.Master4, new Color32(255, 50, 73, 255)},
        {Tier.Master3, new Color32(254, 24, 53, 255)},
        {Tier.Master2, new Color32(255, 25, 53, 255)},
        {Tier.Master1, new Color32(255, 25, 53, 255)},
        {Tier.GrandMaster, new Color32(255, 25, 53, 255)},
        {Tier.Challenger, new Color32(228, 22, 219, 255)}
    };

    public enum GameMode
    {
        None,
        Home,
        Map
    }

    public enum EnemyType
    {
        ScareCrow,
        Clown,
        MossMan,
        SlanderMan,
        TungTungTung,
        Tralalero
    }

    public enum StructureCategory
    {
        Basic,
        Ore,
        Guard,
        Trap,
        Buff,
        Lamp
    }

    public enum StructureType
    {
        Turret,
        Generator,
        GoldenChest,
        Lamp,
        CopperOre,
        SilverOre,
        GoldOre,
        DiamondOre,
        SpellBlocker,
        EnergyShield,
        RepairStation,
        Cooler,
        Trap,
        ThornBush,
        Guillotine,
        Telescope,
        SatelliteAntenna,
        TurretBooster,

        Sheep,
        Grave,
        Frog,
        MovingFrog,
        PoisonFrog,
        GoldenFrog,
        SilverMirror,
        GoldenMirror,
        Axe,
        GoldenAxe,
        MoneySack,
        Battery,
        FlowerPot,
        LushFlowerPot,
        AutoTurret,
        GoldenTurret,

        Bed = 100,
        Door = 101
    }



    public enum BasicStructureType
    {
        Turret,
        Generator,
        GoldenChest,
        Lamp
    }

    public enum OreStructureType
    {
        CopperOre,
        SilverOre,
        GoldOre,
        DiamondOre,
    }

    public enum GuardStructureType
    {
        SpellBlocker,
        EnergyShield,
        RepairStation,
        Cooler
    }

    public enum TrapStructureType
    {
        Trap,
        ThornBush,
        Guillotine
    }

    public enum BuffStructureType
    {
        Telescope,
        SatelliteAntenna,
        TurretBooster
    }

    public static float turretRange = 12f;

    public static int[] enemyHp = new int[]
    {
        350,
        500,
        800,
        1600,
        4000,
        8000,
        12000,
        21600,
        43200,
        85400,
        172800,
        344000,
        7500000
    };

    public static int[] enemyDamage = new int[]
    {
        2,
        3,
        5,
        10,
        22,
        40,
        60,
        150,
        320,
        600,
        1200,
        1800,
        10000
    };

    public static int[] enemyExp = new int[]
    {
        25,
        30,
        45,
        60,
        75,
        85,
        100,
        100,
        100,
        125,
        130,
        140,
        9999999
    };

    public enum CharactorPurchaseType
    {
        Basic,
        Gem,
        Iap
    }

    public static Dictionary<CharactorType, CharactorPurchaseType> CharactorPurchaseData = new Dictionary<CharactorType, CharactorPurchaseType>()
    {
        { CharactorType.Farmer, CharactorPurchaseType.Basic },
        { CharactorType.ReapireMan, CharactorPurchaseType.Gem },
        { CharactorType.LampGirl, CharactorPurchaseType.Iap },
        { CharactorType.Miner, CharactorPurchaseType.Gem },
        { CharactorType.Scientist, CharactorPurchaseType.Iap },
        { CharactorType.Chef, CharactorPurchaseType.Gem}
    };

    public enum BoostType
    {
        Lamp,
        HammerThrow,
        HolyProtection,
        Overheat
    }
}
