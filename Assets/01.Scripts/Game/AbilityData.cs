using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "AbilityData", menuName = "AbilityData", order = 0)]
public class AbilityData : ScriptableObject
{
    public List<Ability> abilities = new List<Ability>();
    public List<Ability> additionalAbilities = new List<Ability>();


    [ContextMenu("Generate Abilities Data")]
    public void GenerateAbilitiesData()
    {
        abilities.Clear();
        additionalAbilities.Clear();

        // 기본 능력 데이터 배열 (type, value, cost, tier)
        var abilityData = new (int type, float value, int cost, Define.Tier tier)[]
        {   
            // Iron 티어
            (1, 3f, 15, Define.Tier.Iron3),       // 포탑 공격력 3% 증가
            (2, 5f, 15, Define.Tier.Iron3),       // 문 체력 5% 증가
            (3, 5f, 15, Define.Tier.Iron2),       // 포탑 공격 사거리 5% 증가
            (4, 0.5f, 15, Define.Tier.Iron2),     // 포탑 치명타 확률 0.5% 증가
            (1, 3f, 20, Define.Tier.Iron1),       // 포탑 공격력 3% 증가
            (5, 5f, 20, Define.Tier.Iron1),       // 포탑 치명타 피해량 5% 증가
            
            // Bronze 티어
            (2, 5f, 25, Define.Tier.Bronze4),     // 문 체력 5% 증가
            (1, 3f, 30, Define.Tier.Bronze4),     // 포탑 공격력 3% 증가
            (4, 0.5f, 35, Define.Tier.Bronze3),   // 포탑 치명타 확률 0.5% 증가
            (5, 10f, 40, Define.Tier.Bronze3),    // 포탑 치명타 피해량 10% 증가
            (1, 3f, 45, Define.Tier.Bronze2),     // 포탑 공격력 3% 증가
            (2, 5f, 50, Define.Tier.Bronze2),     // 문 체력 5% 증가
            (3, 5f, 60, Define.Tier.Bronze1),     // 포탑 공격 사거리 5% 증가
            (5, 10f, 70, Define.Tier.Bronze1),    // 포탑 치명타 피해량 10% 증가
            
            // Silver 티어
            (1, 3f, 80, Define.Tier.Silver4),     // 포탑 공격력 3% 증가
            (2, 5f, 90, Define.Tier.Silver4),     // 문 체력 5% 증가
            (4, 2.5f, 100, Define.Tier.Silver3),  // 포탑 치명타 확률 2.5% 증가
            (5, 10f, 115, Define.Tier.Silver3),   // 포탑 치명타 피해량 10% 증가
            (1, 3f, 130, Define.Tier.Silver2),    // 포탑 공격력 3% 증가
            (6, 5f, 145, Define.Tier.Silver2),    // 포탑 공격속도 5% 증가
            (2, 5f, 160, Define.Tier.Silver1),    // 문 체력 5% 증가
            (1, 3f, 175, Define.Tier.Silver1),    // 포탑 공격력 3% 증가
            
            // Gold 티어
            (3, 5f, 190, Define.Tier.Gold4),      // 포탑 공격 사거리 5% 증가
            (1, 3f, 210, Define.Tier.Gold4),      // 포탑 공격력 3% 증가
            (2, 5f, 230, Define.Tier.Gold3),      // 문 체력 5% 증가
            (4, 0.5f, 250, Define.Tier.Gold3),    // 포탑 치명타 확률 0.5% 증가
            (5, 10f, 270, Define.Tier.Gold2),     // 포탑 치명타 피해량 10% 증가
            (6, 5f, 290, Define.Tier.Gold2),      // 포탑 공격속도 5% 증가
            (1, 3f, 310, Define.Tier.Gold1),      // 포탑 공격력 3% 증가
            (2, 5f, 330, Define.Tier.Gold1),      // 문 체력 5% 증가
            
            // Platinum 티어
            (3, 10f, 350, Define.Tier.Platinum4), // 포탑 공격 사거리 10% 증가
            (6, 5f, 375, Define.Tier.Platinum4),  // 포탑 공격속도 5% 증가
            (1, 3f, 400, Define.Tier.Platinum3),  // 포탑 공격력 3% 증가
            (2, 5f, 425, Define.Tier.Platinum3),  // 문 체력 5% 증가
            (1, 3f, 450, Define.Tier.Platinum2),  // 포탑 공격력 3% 증가
            (2, 5f, 475, Define.Tier.Platinum2),  // 문 체력 5% 증가
            (3, 5f, 500, Define.Tier.Platinum1),  // 포탑 공격 사거리 5% 증가
            (1, 3f, 500, Define.Tier.Platinum1),  // 포탑 공격력 3% 증가
            
            // Emerald 티어
            (2, 5f, 500, Define.Tier.Emerald4),   // 문 체력 5% 증가
            (6, 5f, 500, Define.Tier.Emerald4),   // 포탑 공격속도 5% 증가
            (1, 3f, 500, Define.Tier.Emerald3),   // 포탑 공격력 3% 증가
            (2, 5f, 500, Define.Tier.Emerald3),   // 문 체력 5% 증가
            (1, 3f, 500, Define.Tier.Emerald2),   // 포탑 공격력 3% 증가
            (2, 5f, 500, Define.Tier.Emerald2),   // 문 체력 5% 증가
            (3, 5f, 500, Define.Tier.Emerald1),   // 포탑 공격 사거리 5% 증가
            (1, 3f, 500, Define.Tier.Emerald1),   // 포탑 공격력 3% 증가
            (2, 5f, 500, Define.Tier.Emerald1),   // 문 체력 5% 증가
            
            // Diamond 티어
            (4, 1f, 500, Define.Tier.Diamond4),   // 포탑 치명타 확률 1% 증가
        };

        // 추가 능력 데이터 배열 (type, value, cost, tier)
        var additionalAbilityData = new (int type, float value, int cost, Define.Tier tier)[]
        {
            (6, 5f, 50, Define.Tier.Bronze4),     // 포탑 공격속도 5% 증가
            (7, 1f, 150, Define.Tier.Silver4),    // 침대 골드 생산량 1 증가
            (8, 3f, 225, Define.Tier.Gold4),      // 문 수리 능력 체력비례 3%
            (9, 1f, 400, Define.Tier.Platinum4),  // 수리대 수리 능력 체력비례 1% 증가
            (10, 1f, 1000, Define.Tier.Emerald4), // 발전기 에너지 생산량 1 증가
            (11, 3f, 1500, Define.Tier.Emerald1), // 타워 더블어택 확률 3%
            (11, 3f, 2000, Define.Tier.Diamond4), // 타워 더블어택 확률 3%
        };

        foreach (var data in abilityData)
        {
            abilities.Add(new Ability
            {
                type = (AbilityType)(data.type),
                value = data.value,
                cost = data.cost,
                needTier = data.tier
            });
        }

        foreach (var data in additionalAbilityData)
        {
            additionalAbilities.Add(new Ability
            {
                type = (AbilityType)(data.type),
                value = data.value,
                cost = data.cost,
                needTier = data.tier
            });
        }

        Debug.Log($"Generated {abilities.Count} abilities and {additionalAbilities.Count} additional abilities");
        

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public Ability GetAdditionalAbility(Define.Tier tier)
    {
        return additionalAbilities.Find(a => a.needTier == tier);
    }

    public int GetAdditionalAbilityIndex(Define.Tier tier)
    {
        return additionalAbilities.FindIndex(a => a.needTier == tier);
    }


}

[System.Serializable]
public class Ability
{
    public AbilityType type;
    public float value;
    public int cost;
    public Define.Tier needTier;

    public string GetAbilityDesc()
    {
        return Managers.Localize.GetDynamicText("ability_desc_" + type, value.ToString());
    }
}

public enum AbilityType
{
    AttackDamage = 1,        // 1: 공격력
    DoorHp = 2,              // 2: 문 체력
    AttackRange = 3,         // 3: 공격 사거리
    CriticalChance = 4,      // 4: 치명타 확률
    CriticalDamage = 5,      // 5: 치명타 피해
    AttackSpeed = 6,         // 6: 공격속도
    BedGoldGain = 7,         // 7: 침대 골드 생산량
    DoorRepairMul = 8,       // 8: 문 수리 능력
    RepairStationMul = 9,    // 9: 수리대 수리 능력
    GeneratorMul = 10,        // 10: 발전기 에너지 생산량
    TurretDoubleAttack = 11   // 11: 타워 더블어택 확률
}