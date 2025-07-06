using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class EnemyEffect
{
    public string Name;
    public float Duration;
    public float elapsedTime = 0f;

    public bool IsActive { get; private set; }

    public void Apply(Enemy enemy)
    {
        IsActive = true;
        elapsedTime = 0f;
        OnApply(enemy);
    }

    public void Tick(Enemy enemy, float deltaTime)
    {
        if (!IsActive) return;
        elapsedTime += deltaTime;
        OnTick(enemy, deltaTime);

        if (elapsedTime >= Duration)
        {
            Remove(enemy);
        }
    }

    public void Remove(Enemy enemy)
    {
        if (!IsActive) return;
        IsActive = false;
        OnRemove(enemy);
    }

    // 효과별 세부 구현
    protected abstract void OnApply(Enemy enemy);
    protected abstract void OnTick(Enemy enemy, float deltaTime);
    protected abstract void OnRemove(Enemy enemy);
}

public class BleedEffect : EnemyEffect
{
    private float tickTimer = 0f;

    public BleedEffect(float duration)
    {
        Name = "Bleed";
        Duration = duration;
    }

    protected override void OnApply(Enemy enemy)
    {
        tickTimer = 0f;
        // 출혈 이펙트 등 필요시 추가

        enemy.bleedParticle.GetComponent<ParticleSystem>().Play();
    }

    protected override void OnTick(Enemy enemy, float deltaTime)
    {
        tickTimer += deltaTime;
        // 0.5초마다 최대 체력의 0.5% 데미지
        while (tickTimer >= 0.5f)
        {
            tickTimer -= 0.5f;
            int bleedDamage = Mathf.Max(1, Mathf.RoundToInt(enemy.MaxHp * 0.005f));
            enemy.Hit(bleedDamage, false);
        }
    }

    protected override void OnRemove(Enemy enemy)
    {
        // 출혈 종료 이펙트 등 필요시 추가

        enemy.bleedParticle.GetComponent<ParticleSystem>().Stop();
    }
}

public class FreezeEffect : EnemyEffect
{
    private float attackSpeedMultiplier = 1.25f;

    public FreezeEffect(float duration)
    {
        Name = "Freeze";
        Duration = duration;
    }

    protected override void OnApply(Enemy enemy)
    {
        enemy.attackSpeed.AddMultiplier(attackSpeedMultiplier);
    }

    protected override void OnTick(Enemy enemy, float deltaTime)
    {
        // 별도 지속 효과 없음
    }

    protected override void OnRemove(Enemy enemy)
    {
        enemy.attackSpeed.RemoveMultiplier(attackSpeedMultiplier);
    }
}

public class StunEffect : EnemyEffect
{
    public StunEffect(float duration)
    {
        Name = "Stun";
        Duration = duration;
    }

    protected override void OnApply(Enemy enemy)
    {
        enemy.Agent.isStopped = true;
        enemy.Agent.velocity = Vector3.zero;
        enemy.stunParticle.gameObject.SetActive(true);
    }

    protected override void OnTick(Enemy enemy, float deltaTime) { }

    protected override void OnRemove(Enemy enemy)
    {
        enemy.Agent.isStopped = false;
        enemy.stunParticle.gameObject.SetActive(false);
    }
}

public class PoisonEffect : EnemyEffect
{
    private float tickTimer = 0f;

    public PoisonEffect(float duration = 3f)
    {
        Name = "Poison";
        Duration = duration;
    }

    protected override void OnApply(Enemy enemy)
    {
        tickTimer = 0f;
        // 필요시 이펙트 추가 (예: enemy.poisonParticle.Play();)
        enemy.poisonParticle.GetComponent<ParticleSystem>().Play();
    }

    protected override void OnTick(Enemy enemy, float deltaTime)
    {
        tickTimer += deltaTime;
        // 0.5초마다 2의 데미지
        while (tickTimer >= 0.5f)
        {
            tickTimer -= 0.5f;
            enemy.Hit(2, false);

            Managers.Audio.PlaySound("snd_enemy_hit2", enemy.transform, minRangeVolumeMul: -6f);
        }
    }

    protected override void OnRemove(Enemy enemy)
    {
        // 필요시 이펙트 종료 (예: enemy.poisonParticle.Stop();)
        enemy.poisonParticle.GetComponent<ParticleSystem>().Stop();
    }
}