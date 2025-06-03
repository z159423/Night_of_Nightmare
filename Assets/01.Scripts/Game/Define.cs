using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum CharactorType
    {
        Farmer,
        ReapireMan,
        LampGirl,
        Miner,
        Scientist,
        Chef
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

    public enum GameMode
    {
        None,
        Home,
        Map
    }
}
