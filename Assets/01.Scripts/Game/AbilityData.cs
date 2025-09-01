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

        // 데이터 배열 (type, value, cost, tier) - 표에 맞춰서 조건 수동 입력
        var abilityData = new (int type, float value, int cost, Define.Tier tier)[]
        {   
            // 기본 능력들
            (1, 3f, 5, Define.Tier.Iron3),
            (2, 5f, 5, Define.Tier.Iron3),        // Iron3 사용 (이전 조건)
            (3, 5f, 10, Define.Tier.Iron2),
            (4, 0.5f, 10, Define.Tier.Iron2),     // Iron2 사용 (이전 조건)
            (1, 3f, 10, Define.Tier.Iron1),
            (5, 5f, 15, Define.Tier.Iron1),       // Iron1 사용 (이전 조건)
            (2, 5f, 15, Define.Tier.Bronze4),
            (1, 3f, 20, Define.Tier.Bronze4),     // Bronze4 사용 (이전 조건)
            (4, 0.5f, 20, Define.Tier.Bronze3),
            (5, 10f, 20, Define.Tier.Bronze3),    // Bronze3 사용 (이전 조건)
            (1, 3f, 25, Define.Tier.Bronze2),
            (2, 5f, 25, Define.Tier.Bronze2),     // Bronze2 사용 (이전 조건)
            (3, 5f, 25, Define.Tier.Bronze1),
            (5, 10f, 25, Define.Tier.Bronze1),    // Bronze1 사용 (이전 조건)
            (1, 3f, 30, Define.Tier.Silver4),
            (2, 5f, 30, Define.Tier.Silver4),     // Silver4 사용 (이전 조건)
            (4, 2.5f, 30, Define.Tier.Silver3),
            (5, 10f, 30, Define.Tier.Silver3),    // Silver3 사용 (이전 조건)
            (1, 3f, 30, Define.Tier.Silver2),
            (6, 5f, 35, Define.Tier.Silver2),     // Silver2 사용 (이전 조건)
            (2, 5f, 35, Define.Tier.Silver1),
            (1, 3f, 35, Define.Tier.Silver1),     // Silver1 사용 (이전 조건)
            (3, 5f, 35, Define.Tier.Gold4),
            (1, 3f, 40, Define.Tier.Gold4),       // Gold4 사용 (이전 조건)
            (2, 5f, 40, Define.Tier.Gold3),
            (4, 0.5f, 40, Define.Tier.Gold3),     // Gold3 사용 (이전 조건)
            (5, 10f, 40, Define.Tier.Gold2),
            (6, 5f, 40, Define.Tier.Gold2),       // Gold2 사용 (이전 조건)
            (1, 3f, 40, Define.Tier.Gold1),
            (2, 5f, 40, Define.Tier.Gold1),       // Gold1 사용 (이전 조건)
            (3, 10f, 40, Define.Tier.Platinum4),
            (6, 5f, 50, Define.Tier.Platinum4),   // Platinum4 사용 (이전 조건)
            (1, 3f, 50, Define.Tier.Platinum3),
            (2, 5f, 50, Define.Tier.Platinum3),   // Platinum3 사용 (이전 조건)
            (1, 3f, 50, Define.Tier.Platinum2),
            (2, 5f, 50, Define.Tier.Platinum2),   // Platinum2 사용 (이전 조건)
            (3, 5f, 50, Define.Tier.Platinum1),
            (1, 3f, 50, Define.Tier.Platinum1),   // Platinum1 사용 (이전 조건)
            (2, 5f, 50, Define.Tier.Emerald4),
            (6, 5f, 60, Define.Tier.Emerald4),    // Emerald4 사용 (이전 조건)
            (1, 3f, 70, Define.Tier.Emerald3),
            (2, 5f, 80, Define.Tier.Emerald3),    // Emerald3 사용 (이전 조건)
            (1, 3f, 90, Define.Tier.Emerald2),
            (2, 5f, 100, Define.Tier.Emerald2),   // Emerald2 사용 (이전 조건)
            (3, 5f, 100, Define.Tier.Emerald1),
            (1, 3f, 100, Define.Tier.Emerald1),   // Emerald1 사용 (이전 조건)
            (2, 5f, 100, Define.Tier.Emerald1),   // Emerald1 사용 (이전 조건)
            (4, 1f, 100, Define.Tier.Diamond4),
        };

        foreach (var data in abilityData)
        {
            abilities.Add(new Ability
            {
                type = (AbilityType)(data.type), // 1-11 그대로 사용 (enum 값 수정됨)
                value = data.value,
                cost = data.cost,
                needTier = data.tier
            });
        }

        Debug.Log($"Generated {abilities.Count} abilities");

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public Ability GetAdditionalAbility(Define.Tier tier)
    {
        return additionalAbilities.Find(a => a.needTier == tier);
    }
}

[System.Serializable]
public class Ability
{
    public AbilityType type;
    public float value;
    public int cost;
    public Define.Tier needTier;
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