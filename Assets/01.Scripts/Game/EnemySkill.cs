using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public abstract class EnemySkill
{
    public string Name;
    public int MinLevel;
    public float MinCooldown;
    public float MaxCooldown;

    public float Duration;
    public float LastUseTime = -999f;
    public bool IsActive { get; protected set; }

    public abstract bool CanUse(Enemy enemy);
    public abstract void Activate(Enemy enemy);
    public abstract void Deactivate(Enemy enemy);

    public int GetCoolDown()
    {
        // 난이도에 따라 쿨타임을 조정
        return Mathf.RoundToInt(Random.Range(MinCooldown, MaxCooldown) - (Define.TierDiffValue[Define.GetPlayerCurrentTier()] * 0.05f));
    }
}

public class AttackSpeedSkill : EnemySkill
{
    private float speedMultiplier = 2.4f;
    private float moveMultiplier = 2f;

    public AttackSpeedSkill()
    {
        Name = "AttackSpeed";
        MinLevel = 2;
        MinCooldown = 15f; // 기본값, 실제 쿨타임은 난이도에 따라 계산
        MaxCooldown = 15f;
        Duration = 4f;
    }

    public override bool CanUse(Enemy enemy)
    {
        return enemy.level >= MinLevel && !IsActive && Time.time - LastUseTime >= GetCoolDown();
    }

    public override void Activate(Enemy enemy)
    {
        IsActive = true;
        LastUseTime = Time.time;
        enemy.attackSpeed.AddMultiplier(speedMultiplier);
        enemy.moveSpeed.AddMultiplier(moveMultiplier);
        // 기타 효과

        var popup = Managers.Resource.Instantiate("Notification_Popup", Managers.UI.Root.transform);
        popup.GetComponent<Notification_Popup>().Init();
        popup.GetComponent<Notification_Popup>().Setting(Managers.Localize.GetText("global.str_toast_enemy_spd_skill"));

        //근처에 spellBlocker가 있는지 확인

        Managers.Game.charactors
            .Where(n => n.currentActiveRoom != null && n.playerData != null)
            .ToList()
            .ForEach(n =>
            {
                var spellBlocker = n.playerData.structures
                    .FirstOrDefault(s => s.type == Define.StructureType.SpellBlocker && !s.destroyed && Vector2.Distance(s.transform.position, enemy.transform.position) < 5f);
                if (spellBlocker != null && spellBlocker.TryGetComponent<SpellBlocker>(out var sb))
                {
                    sb.TryCastSpellBlock(enemy, () => Deactivate(enemy));
                }
            });
    }

    public override void Deactivate(Enemy enemy)
    {
        IsActive = false;
        enemy.attackSpeed.RemoveMultiplier(speedMultiplier);
        enemy.moveSpeed.RemoveMultiplier(moveMultiplier);
    }
}

public class AttackDamageSkill : EnemySkill
{
    private float damageMultiplier = 1.3f;

    public AttackDamageSkill()
    {
        Name = "AttackDamage";
        MinLevel = 4;
        MinCooldown = 20f; // 기본값, 실제 쿨타임은 난이도에 따라 계산
        MaxCooldown = 20f;
        Duration = 7f;
    }

    public override bool CanUse(Enemy enemy)
    {
        return enemy.level >= MinLevel && !IsActive && Time.time - LastUseTime >= GetCoolDown();
    }

    public override void Activate(Enemy enemy)
    {
        IsActive = true;
        LastUseTime = Time.time;
        enemy.attackPower.AddMultiplier(damageMultiplier);
        // 타격 사운드 변경 등 추가 효과는 필요시 구현

        var popup = Managers.Resource.Instantiate("Notification_Popup", Managers.UI.Root.transform);
        popup.GetComponent<Notification_Popup>().Init();
        popup.GetComponent<Notification_Popup>().Setting(Managers.Localize.GetText("global.str_toast_enemy_dmg_skill"));
    }

    public override void Deactivate(Enemy enemy)
    {
        IsActive = false;
        enemy.attackPower.RemoveMultiplier(damageMultiplier);
    }
}

public class Creepylaughter : EnemySkill
{
    public Creepylaughter()
    {
        Name = "Creepylaughter";
        MinLevel = 3;
        MinCooldown = 40f; // 기본값, 실제 쿨타임은 난이도에 따라 계산
        MaxCooldown = 80f;
        Duration = 6f;
    }

    public override bool CanUse(Enemy enemy)
    {
        return enemy.enemyType == Define.EnemyType.Clown && enemy.level >= MinLevel && !IsActive && Time.time - LastUseTime >= GetCoolDown();
    }

    public override void Activate(Enemy enemy)
    {
        IsActive = true;
        LastUseTime = Time.time;
        // 타격 사운드 변경 등 추가 효과는 필요시 구현

        var popup = Managers.Resource.Instantiate("Notification_Popup", Managers.UI.Root.transform);
        popup.GetComponent<Notification_Popup>().Init();
        popup.GetComponent<Notification_Popup>().Setting(Managers.Localize.GetText("global.str_lol_toast"));

        enemy.creepylaughterParticle.GetComponent<ParticleSystem>().Play();

        foreach (var charactor in Managers.Game.charactors)
        {
            if (!charactor.die)
            {
                foreach (var structure in charactor.playerData.structures)
                {
                    if (!structure.destroyed && Vector2.Distance(structure.transform.position, enemy.transform.position) < 5f)
                    {
                        structure.AddEffect(new CreepylaughterEffect(Duration));
                    }
                }
            }
        }
    }

    public override void Deactivate(Enemy enemy)
    {
        IsActive = false;
    }
}

public class MothPowder : EnemySkill
{
    public MothPowder()
    {
        Name = "MothPowder";
        MinLevel = 3;
        MinCooldown = 40f; // 기본값, 실제 쿨타임은 난이도에 따라 계산
        MaxCooldown = 80f;
        Duration = 4f;
    }

    public override bool CanUse(Enemy enemy)
    {
        return enemy.enemyType == Define.EnemyType.MossMan && enemy.level >= MinLevel && !IsActive && Time.time - LastUseTime >= GetCoolDown();
    }

    public override void Activate(Enemy enemy)
    {
        IsActive = true;
        LastUseTime = Time.time;
        // 타격 사운드 변경 등 추가 효과는 필요시 구현

        var popup = Managers.Resource.Instantiate("Notification_Popup", Managers.UI.Root.transform);
        popup.GetComponent<Notification_Popup>().Init();
        popup.GetComponent<Notification_Popup>().Setting(Managers.Localize.GetText("global.str_moth_skill_toast"));

        enemy.creepylaughterParticle.GetComponent<ParticleSystem>().Play();
        enemy.mossManSkillParticle.GetComponent<ParticleSystem>().Play();

        foreach (var charactor in Managers.Game.charactors)
        {
            if (!charactor.die)
            {
                foreach (var structure in charactor.playerData.structures.Where(n => n is Turret))
                {
                    if (!structure.destroyed && Vector2.Distance(structure.transform.position, enemy.transform.position) < 5f)
                    {
                        structure.AddEffect(new MothPowderStun(Duration));
                    }
                }
            }
        }
    }

    public override void Deactivate(Enemy enemy)
    {
        IsActive = false;
    }
}