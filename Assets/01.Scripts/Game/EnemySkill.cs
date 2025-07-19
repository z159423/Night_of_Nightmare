using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

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
    public abstract void DeactiveBySpellBlocker();

    public int GetCoolDown()
    {
        // 난이도에 따라 쿨타임을 조정
        return Mathf.RoundToInt(Random.Range(MinCooldown, MaxCooldown) - (Define.GetCurrentStageDiffValue() * 0.05f));
    }
}

public class AttackSpeedSkill : EnemySkill
{
    private float speedMultiplier = 0.416f;
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
        // enemy.moveSpeed.AddMultiplier(moveMultiplier);
        // 기타 효과
        enemy.attackSpeedSkillParticle.GetComponent<ParticleSystem>().Play();

        Managers.UI.ShowNotificationPopup("global.str_toast_enemy_spd_skill");

        //근처에 spellBlocker가 있는지 확인

        Managers.Audio.PlaySound("snd_enemy_laugh", minRangeVolumeMul: -1f);

        Managers.Game.charactors
            .Where(n => n.currentActiveRoom != null && n.playerData != null)
            .ToList()
            .ForEach(n =>
            {
                var spellBlocker = n.playerData.structures
                    .FirstOrDefault(s => s.type == Define.StructureType.SpellBlocker && !s.destroyed && Vector2.Distance(s.transform.position, enemy.transform.position) < 7f);
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
        // enemy.moveSpeed.RemoveMultiplier(moveMultiplier);

        enemy.attackSpeedSkillParticle.GetComponent<ParticleSystem>().Stop();
    }

    public override void DeactiveBySpellBlocker()
    {
        // 이 스킬은 SpellBlocker에 의해 비활성화되지 않음
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

        Managers.UI.ShowNotificationPopup("global.str_toast_enemy_dmg_skill");

        Managers.Audio.PlaySound("snd_enemy_anger", minRangeVolumeMul: -1f);

        enemy.attackDamageSkillParticle.GetComponent<ParticleSystem>().Play();

        Managers.Game.charactors
            .Where(n => n.currentActiveRoom != null && n.playerData != null)
            .ToList()
            .ForEach(n =>
            {
                var spellBlocker = n.playerData.structures
                    .FirstOrDefault(s => s.type == Define.StructureType.SpellBlocker && !s.destroyed && Vector2.Distance(s.transform.position, enemy.transform.position) < 7f);
                if (spellBlocker != null && spellBlocker.TryGetComponent<SpellBlocker>(out var sb))
                {
                    sb.TryCastSpellBlock(enemy, () => Deactivate(enemy));
                }
            });
    }

    public override void Deactivate(Enemy enemy)
    {
        IsActive = false;
        enemy.attackPower.RemoveMultiplier(damageMultiplier);

        enemy.attackDamageSkillParticle.GetComponent<ParticleSystem>().Stop();
    }

    public override void DeactiveBySpellBlocker()
    {
        // 이 스킬은 SpellBlocker에 의해 비활성화되지 않음
    }
}

public class Creepylaughter : EnemySkill
{
    private List<(StructureEffect, Structure)> effects = new List<(StructureEffect, Structure)>();

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

        Managers.UI.ShowNotificationPopup("global.str_lol_toast");

        enemy.creepylaughterParticle.GetComponent<ParticleSystem>().Play();

        foreach (var charactor in Managers.Game.charactors)
        {
            if (!charactor.die)
            {
                foreach (var structure in charactor.playerData.structures)
                {
                    if (!structure.destroyed && Vector2.Distance(structure.transform.position, enemy.transform.position) < 5f)
                    {
                        var effect = new CreepylaughterEffect(Duration);
                        structure.AddEffect(effect);
                        effects.Add((effect, structure));
                    }
                }
            }
        }

        Managers.Game.charactors
            .Where(n => n.currentActiveRoom != null && n.playerData != null)
            .ToList()
            .ForEach(n =>
            {
                var spellBlocker = n.playerData.structures
                    .FirstOrDefault(s => s.type == Define.StructureType.SpellBlocker && !s.destroyed && Vector2.Distance(s.transform.position, enemy.transform.position) < 7f);
                if (spellBlocker != null && spellBlocker.TryGetComponent<SpellBlocker>(out var sb))
                {
                    sb.TryCastSpellBlock(enemy, () =>
                    {
                        Deactivate(enemy);
                        DeactiveBySpellBlocker();
                    });
                }
            });

        Managers.Audio.PlaySound("snd_enemy_lol", minRangeVolumeMul: -1f, pitch: Random.Range(1.0f, 1.3f));
    }

    public override void DeactiveBySpellBlocker()
    {
        foreach (var effect in effects)
        {
            if (effect.Item2 != null && !effect.Item2.destroyed && effect.Item1.IsActive)
            {
                effect.Item1.Remove(effect.Item2);
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
    private List<(StructureEffect, Structure)> effects = new List<(StructureEffect, Structure)>();

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

        Managers.UI.ShowNotificationPopup("global.str_moth_skill_toast");

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

        Managers.Game.charactors
            .Where(n => n.currentActiveRoom != null && n.playerData != null)
            .ToList()
            .ForEach(n =>
            {
                var spellBlocker = n.playerData.structures
                    .FirstOrDefault(s => s.type == Define.StructureType.SpellBlocker && !s.destroyed && Vector2.Distance(s.transform.position, enemy.transform.position) < 7f);
                if (spellBlocker != null && spellBlocker.TryGetComponent<SpellBlocker>(out var sb))
                {
                    sb.TryCastSpellBlock(enemy, () =>
                    {
                        Deactivate(enemy);
                        DeactiveBySpellBlocker();
                    });
                }
            });

        Managers.Audio.PlaySound("snd_boss_roar", minRangeVolumeMul: -1f);
    }

    public override void Deactivate(Enemy enemy)
    {
        IsActive = false;
    }

    public override void DeactiveBySpellBlocker()
    {
        foreach (var effect in effects)
        {
            if (effect.Item2 != null && !effect.Item2.destroyed && effect.Item1.IsActive)
            {
                effect.Item1.Remove(effect.Item2);
            }
        }
    }
}