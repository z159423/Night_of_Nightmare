using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    public static Tier GetPlayerCurrentTier()
    {
        return GetTierByScore(Managers.LocalData.PlayerRankingPoint);
    }



    public static Tier GetTierByScore(int score)
    {
        // TierToScore를 점수 오름차순으로 정렬
        var ordered = TierToScore.OrderBy(kv => kv.Value).ToList();

        Tier result = ordered[0].Key; // 기본값은 가장 낮은 Tier

        for (int i = 0; i < ordered.Count; i++)
        {
            if (score < ordered[i].Value)
                break;
            result = ordered[i].Key;
        }
        return result;
    }

    public static Dictionary<Tier, int> TierDiffValue = new Dictionary<Tier, int>()
    {
        {Tier.Iron4, -30},
        {Tier.Iron3, -20},
        {Tier.Iron2, -15},
        {Tier.Iron1, -10},
        {Tier.Bronze4, -5},
        {Tier.Bronze3, 0},
        {Tier.Bronze2, 5},
        {Tier.Bronze1, 8},
        {Tier.Silver4, 11},
        {Tier.Silver3, 14},
        {Tier.Silver2, 17},
        {Tier.Silver1, 20},
        {Tier.Gold4, 24},
        {Tier.Gold3, 28},
        {Tier.Gold2, 32},
        {Tier.Gold1, 36},
        {Tier.Platinum4, 40},
        {Tier.Platinum3, 44},
        {Tier.Platinum2, 48},
        {Tier.Platinum1, 52},
        {Tier.Emerald4, 56},
        {Tier.Emerald3, 60},
        {Tier.Emerald2, 65},
        {Tier.Emerald1, 70},
        {Tier.Diamond4, 75},
        {Tier.Diamond3, 80},
        {Tier.Diamond2, 85},
        {Tier.Diamond1, 90},
        {Tier.Master4, 100},
        {Tier.Master3, 110},
        {Tier.Master2, 120},
        {Tier.Master1, 130},
        {Tier.GrandMaster, 150},
        {Tier.Challenger, 170}
    };

    public static int GetCurrentStageDiffValue()
    {
        if (Managers.Game.isChallengeMode)
        {
            return ChallenModeDifficultyValue[Managers.Game.challengeLevel];
        }
        else
        {
            return TierDiffValue[GetPlayerCurrentTier()];
        }
    }

    public static Dictionary<Tier, (int, int)> TierWinGetPoint = new Dictionary<Tier, (int, int)>()
    {
        {Tier.Iron4, (200, 250)},
        {Tier.Iron3, (175, 225)},
        {Tier.Iron2, (150, 200)},
        {Tier.Iron1, (125, 175)},
        {Tier.Bronze4, (100, 150)},
        {Tier.Bronze3, (75, 125)},
        {Tier.Bronze2, (70, 120)},
        {Tier.Bronze1, (65, 115)},
        {Tier.Silver4, (60, 110)},
        {Tier.Silver3, (55, 110)},
        {Tier.Silver2, (50, 110)},
        {Tier.Silver1, (50, 110)},
        {Tier.Gold4, (50, 110)},
        {Tier.Gold3, (50, 110)},
        {Tier.Gold2, (50, 110)},
        {Tier.Gold1, (50, 110)},
        {Tier.Platinum4, (50, 110)},
        {Tier.Platinum3, (50, 110)},
        {Tier.Platinum2, (50, 110)},
        {Tier.Platinum1, (50, 110)},
        {Tier.Emerald4, (50, 110)},
        {Tier.Emerald3, (50, 110)},
        {Tier.Emerald2, (50, 110)},
        {Tier.Emerald1, (50, 110)},
        {Tier.Diamond4, (50, 100)},
        {Tier.Diamond3, (50, 90)},
        {Tier.Diamond2, (50, 80)},
        {Tier.Diamond1, (50, 70)},
        {Tier.Master4, (30, 60)},
        {Tier.Master3, (30, 60)},
        {Tier.Master2, (30, 60)},
        {Tier.Master1, (30, 60)},
        {Tier.GrandMaster, (25, 55)},
        {Tier.Challenger, (25, 51)}
    };

    public static Dictionary<Tier, float> TierLossRatio = new Dictionary<Tier, float>()
    {
        {Tier.Iron4, 0.25f},
        {Tier.Iron3, 0.25f},
        {Tier.Iron2, 0.3f},
        {Tier.Iron1, 0.3f},
        {Tier.Bronze4, 0.3f},
        {Tier.Bronze3, 0.4f},
        {Tier.Bronze2, 0.5f},
        {Tier.Bronze1, 0.5f},
        {Tier.Silver4, 0.5f},
        {Tier.Silver3, 0.5f},
        {Tier.Silver2, 0.5f},
        {Tier.Silver1, 0.5f},
        {Tier.Gold4, 0.6f},
        {Tier.Gold3, 0.6f},
        {Tier.Gold2, 0.6f},
        {Tier.Gold1, 0.6f},
        {Tier.Platinum4, 0.7f},
        {Tier.Platinum3, 0.7f},
        {Tier.Platinum2, 0.7f},
        {Tier.Platinum1, 0.7f},
        {Tier.Emerald4, 0.8f},
        {Tier.Emerald3, 0.8f},
        {Tier.Emerald2, 0.8f},
        {Tier.Emerald1, 0.8f},
        {Tier.Diamond4, 0.8f},
        {Tier.Diamond3, 0.85f},
        {Tier.Diamond2, 0.9f},
        {Tier.Diamond1, 0.95f},
        {Tier.Master4, 1.0f},
        {Tier.Master3, 1.1f},
        {Tier.Master2, 1.2f},
        {Tier.Master1, 1.3f},
        {Tier.GrandMaster, 1.4f},
        {Tier.Challenger, 1.5f}
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

    public static int GetResultGemCount(bool isWin, bool isChallengeMode)
    {
        int count = 0;
        if (isChallengeMode)
            count = (int)(Random.Range(2, 4) + (GetCurrentStageDiffValue() * 0.015f));
        else
            count = (int)(Random.Range(3, 7) + (GetCurrentStageDiffValue() * 0.015f));

        if (!isWin)
        {
            // playTime에 따라 timeMultiplierDict에서 곱할 값 결정
            float multiplier = 0.1f;
            foreach (var kv in timeMultiplierDict.OrderBy(kv => kv.Key))
            {
                if (Managers.Game.playTime < kv.Key)
                {
                    multiplier = kv.Value;
                    break;
                }
            }
            count = Mathf.RoundToInt(count * multiplier);
        }

        count = Mathf.Clamp(count, 10, int.MaxValue);

        return count;
    }

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
        Door = 101,

        None = 9999
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

    public static int GetEnemyMaxHp(EnemyType type, int level)
    {
        int hp = 0;

        switch (type)
        {
            case EnemyType.TungTungTung:
                hp = Mathf.RoundToInt(enemyHp[level] * 0.8f);
                break;
            case EnemyType.Tralalero:
                hp = Mathf.RoundToInt(enemyHp[level] * 0.9f);
                break;
            default:
                hp = enemyHp[level];
                break;
        }

        return Mathf.RoundToInt(hp * ((100 + GetCurrentStageDiffValue()) * 0.01f));
    }

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

    public static float GetEnemyDamage(EnemyType type, int level)
    {
        float damage = 0;
        switch (type)
        {
            case EnemyType.SlanderMan:
                damage = enemyDamage[level] * 0.6f;
                break;
            case EnemyType.TungTungTung:
                damage = enemyDamage[level] * 1.2f;
                break;
            case EnemyType.Tralalero:
                damage = enemyDamage[level] * 1.1f;
                break;
            default:
                damage = enemyDamage[level];
                break;
        }

        return damage * ((100 + GetCurrentStageDiffValue()) * 0.01f);
    }

    public static float GetEnemyAttackSpeed(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.SlanderMan:
                return 0.5f;
            default:
                return 0.75f;
        }
    }

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

    public static int GetEnemyExp(EnemyType type, int level)
    {
        int needyExp = 0;
        switch (type)
        {
            case EnemyType.SlanderMan:
                needyExp = Mathf.RoundToInt(enemyExp[level] * 1.5f);
                break;

            default:
                needyExp = enemyExp[level];
                break;
        }

        return Mathf.RoundToInt(needyExp + (-GetCurrentStageDiffValue() * 0.1f));
    }

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

    public static bool IsFreeStructure(PlayerData playerData, StructureType structureType)
    {
        if (playerData.type == CharactorType.Farmer && structureType == StructureType.Turret && playerData.freeTurretCount < 2)
        {
            return true;
        }
        else if (playerData.type == CharactorType.ReapireMan && structureType == StructureType.RepairStation && playerData.freeRepaireStationCount < 1)
        {
            return true;
        }
        else if (playerData.type == CharactorType.LampGirl && structureType == StructureType.Lamp && playerData.freeLampCount < 1)
        {
            return true;
        }
        else
            return false;
    }

    public static BoostData GetBoostData(BoostType type)
    {
        return Managers.Resource.LoadAll<BoostData>("BoostData/").First(n => n.type == type);
    }

    public static Dictionary<int, float> ChallengeModeDiff = new Dictionary<int, float>()
    {
        { 1, 96.97f },
        { 2, 94.41f },
        { 3, 92.23f },
        { 4, 86.87f },
        { 5, 74.05f },
        { 6, 73.85f },
        { 7, 64.41f },
        { 8, 53.13f },
        { 9, 45.90f },
        { 10, 38.15f },
        { 11, 32.44f },
        { 12, 26.65f },
        { 13, 21.23f },
        { 14, 17.82f },
        { 15, 14.02f },
        { 16, 11.51f },
        { 17, 9.24f },
        { 18, 7.88f },
        { 19, 6.09f },
        { 20, 5.65f },
    };

    public static Dictionary<int, Color32> ChallengeModeColor = new Dictionary<int, Color32>()
    {
    { 1,  new Color32(178, 178, 178, 255) }, // B2B2B2
    { 2,  new Color32(229, 229, 229, 255) }, // E5E5E5
    { 3,  new Color32(253, 255, 178, 255) }, // FDFFB2
    { 4,  new Color32(148, 255, 127, 255) }, // 94FF7F
    { 5,  new Color32(255, 255, 76, 255) },  // FFFF4C
    { 6,  new Color32(212, 255, 127, 255) }, // D4FF7F
    { 7,  new Color32(164, 255, 76, 255) },  // A4FF4C
    { 8,  new Color32(138, 255, 25, 255) },  // 8AFF19
    { 9,  new Color32(178, 223, 255, 255) }, // B2DFFF
    { 10, new Color32(101, 191, 255, 255) }, // 65BFFF
    { 11, new Color32(50, 170, 255, 255) },  // 32AAFF
    { 12, new Color32(43, 94, 255, 255) },   // 2B5EFF
    { 13, new Color32(198, 178, 255, 255) }, // C6B2FF
    { 14, new Color32(179, 153, 255, 255) }, // B399FF
    { 15, new Color32(140, 101, 255, 255) }, // 8C65FF
    { 16, new Color32(255, 218, 124, 255) }, // FFDA7C
    { 17, new Color32(255, 204, 76, 255) },  // FFCC4C
    { 18, new Color32(255, 144, 75, 255) },  // FF904B
    { 19, new Color32(255, 84, 75, 255) },   // FF544B
    { 20, new Color32(255, 25, 52, 255) },   // FF1934
    };

    public static Dictionary<int, int> ChallenModeDifficultyValue = new Dictionary<int, int>()
    {
    { 1, 10 },
    { 2, 20 },
    { 3, 30 },
    { 4, 40 },
    { 5, 50 },
    { 6, 60 },
    { 7, 70 },
    { 8, 80 },
    { 9, 90 },
    { 10, 100 },
    { 11, 110 },
    { 12, 120 },
    { 13, 130 },
    { 14, 140 },
    { 15, 150 },
    { 16, 160 },
    { 17, 170 },
    { 18, 180 },
    { 19, 190 },
    { 20, 200 },
};

    public static Dictionary<int, float> timeMultiplierDict = new Dictionary<int, float>
    {
    { 180, 0.1f },
    { 240, 0.15f },
    { 360, 0.2f },
    { 480, 0.25f },
    { 600, 0.3f },
    { 720, 0.4f },
    { 840, 0.5f },
    { 960, 0.6f },
    { 1080, 0.7f },
    { 1200, 0.8f },
    { int.MaxValue, 0.9f } // 1200초 이상 처리용
};

    public static Dictionary<string, SoundData> soundDatas = new Dictionary<string, SoundData>()
{
    { "bgm_base", new SoundData { soundKey = "bgm_base", baseVolume = 0.34f, pitch = 1.1f, maxVolumeRange = -1f, minRangeVolumeMul = -1f } },
    { "snd_autoguard", new SoundData { soundKey = "snd_autoguard", baseVolume = 1f, pitch = 1f, maxVolumeRange = 432f, minRangeVolumeMul = 0.4f } },
    { "snd_boss_roar", new SoundData { soundKey = "snd_boss_roar", baseVolume = 0.6f, pitch = 1f, maxVolumeRange = -1f, minRangeVolumeMul = -1f } },
    { "snd_broken_object", new SoundData { soundKey = "snd_broken_object", baseVolume = 0.4f, pitch = 1f, maxVolumeRange = 432f, minRangeVolumeMul = 0.4f } },
    { "snd_click", new SoundData { soundKey = "snd_click", baseVolume = 1f, pitch = 1f, maxVolumeRange = -1f, minRangeVolumeMul = -1f } },
    { "snd_coin", new SoundData { soundKey = "snd_coin", baseVolume = 1f, pitch = 1f, maxVolumeRange = 432f, minRangeVolumeMul = 0.4f } },
    { "snd_cutter", new SoundData { soundKey = "snd_cutter", baseVolume = 1f, pitch = 1f, maxVolumeRange = 432f, minRangeVolumeMul = 0.4f } },
    { "snd_enemy_anger", new SoundData { soundKey = "snd_enemy_anger", baseVolume = 0.5f, pitch = 1f, maxVolumeRange = -1f, minRangeVolumeMul = -1f } },
    { "snd_enemy_hit2", new SoundData { soundKey = "snd_enemy_hit2", baseVolume = 0.5f, pitch = 1f, maxVolumeRange = 432f, minRangeVolumeMul = -1f } },
    { "snd_enemy_laugh", new SoundData { soundKey = "snd_enemy_laugh", baseVolume = 0.4f, pitch = 1f, maxVolumeRange = -1f, minRangeVolumeMul = -1f } },
    { "snd_enemy_level_up", new SoundData { soundKey = "snd_enemy_level_up", baseVolume = 0.4f, pitch = 1f, maxVolumeRange = -1f, minRangeVolumeMul = -1f } },
    { "snd_enemy_lol", new SoundData { soundKey = "snd_enemy_lol", baseVolume = 0.6f, pitch = 1.0f, maxVolumeRange = -1f, minRangeVolumeMul = -1f } },
    { "snd_enemy_power_hit", new SoundData { soundKey = "snd_enemy_power_hit", baseVolume = 0.18f, pitch = 1f, maxVolumeRange = 432f, minRangeVolumeMul = 0.4f } },
    { "snd_get", new SoundData { soundKey = "snd_get", baseVolume = 0.75f, pitch = 1f, maxVolumeRange = 432f, minRangeVolumeMul = -1f } },
    { "snd_get_item", new SoundData { soundKey = "snd_get_item", baseVolume = 1f, pitch = 1f, maxVolumeRange = -1f, minRangeVolumeMul = -1f } },
    { "snd_girl_die", new SoundData { soundKey = "snd_girl_die", baseVolume = 0.4f, pitch = 1f, maxVolumeRange = -1f, minRangeVolumeMul = -1f } },
    { "snd_girl_scream_1", new SoundData { soundKey = "snd_girl_scream_1", baseVolume = 0.8f, pitch = 0.88f, maxVolumeRange = 600f, minRangeVolumeMul = -1f } },
    { "snd_girl_scream_2", new SoundData { soundKey = "snd_girl_scream_2", baseVolume = 0.8f, pitch = 0.88f, maxVolumeRange = 600f, minRangeVolumeMul = -1f } },
    { "snd_girl_scream_3", new SoundData { soundKey = "snd_girl_scream_3", baseVolume = 0.8f, pitch = 0.88f, maxVolumeRange = 600f, minRangeVolumeMul = -1f } },
    { "snd_loading", new SoundData { soundKey = "snd_loading", baseVolume = 1f, pitch = 1f, maxVolumeRange = -1f, minRangeVolumeMul = -1f } },
    { "snd_scream_1", new SoundData { soundKey = "snd_scream_1", baseVolume = 0.8f, pitch = 0.88f, maxVolumeRange = 600f, minRangeVolumeMul = -1f } },
    { "snd_scream_2", new SoundData { soundKey = "snd_scream_2", baseVolume = 0.8f, pitch = 0.88f, maxVolumeRange = 600f, minRangeVolumeMul = -1f } },
    { "snd_scream_3", new SoundData { soundKey = "snd_scream_3", baseVolume = 0.8f, pitch = 0.88f, maxVolumeRange = 600f, minRangeVolumeMul = -1f } },
    { "snd_skill_spawn_effect", new SoundData { soundKey = "snd_skill_spawn_effect", baseVolume = 0.35f, pitch = 1f, maxVolumeRange = -1f, minRangeVolumeMul = -1f } },
    { "snd_spell", new SoundData { soundKey = "snd_spell", baseVolume = 1f, pitch = 1f, maxVolumeRange = 432f, minRangeVolumeMul = -1f } },
    { "snd_stage_unlock", new SoundData { soundKey = "snd_stage_unlock", baseVolume = 0.3f, pitch = 1f, maxVolumeRange = 432f, minRangeVolumeMul = -1f } },
    { "snd_sword_swing", new SoundData { soundKey = "snd_sword_swing", baseVolume = 1f, pitch = 0.8f, maxVolumeRange = -1f, minRangeVolumeMul = -1f } },
    { "snd_tick", new SoundData { soundKey = "snd_tick", baseVolume = 1f, pitch = 1f, maxVolumeRange = 432f, minRangeVolumeMul = 0.6f } },
    { "snd_tower_hit", new SoundData { soundKey = "snd_tower_hit", baseVolume = 0.4f, pitch = 0.6f, maxVolumeRange = 432f, minRangeVolumeMul = 0.4f } },
    { "snd_wizard_die", new SoundData { soundKey = "snd_wizard_die", baseVolume = 0.4f, pitch = 1f, maxVolumeRange = -1f, minRangeVolumeMul = -1f } },
    { "snd_build", new SoundData { soundKey = "snd_build", baseVolume = 1f, pitch = 1.1f, maxVolumeRange = 432f, minRangeVolumeMul = 0.6f } }
};

    public static readonly string[] SoundKeys = new string[]
    {
        "RvTicket_5",
        "RvTicket_30",
        "RvTicket_100",
        "Gem_1500",
        "Gem_5000",
        "Gem_11000",
        "BoostPack"
    };
}
