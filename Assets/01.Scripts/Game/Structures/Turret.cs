using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using Unity.VisualScripting;

public class Turret : Structure
{
    public Enemy target;
    protected float attackCooldown = 1f;
    private float lastAttackTime;

    GameObject body;
    GameObject head;

    float headOffset = -90;

    List<GameObject> bullets = new List<GameObject>();

    public GameObject overHeatParticlel;

    protected override void Start()
    {
        base.Start();
        lastAttackTime = Time.time;

        body = gameObject.FindRecursive("Body");
        head = gameObject.FindRecursive("Head");

        overHeatParticlel = gameObject.FindRecursive("OverHeatParticle");

        upgradePercent = 2.5f;
    }

    protected override void Update()
    {
        base.Update();
        if (target == null && Managers.Game.enemy != null)
        {
            // 타겟이 없으면 120px(=1.2f) 범위 내의 enemy를 찾음
            float distance = Vector3.Distance(transform.position, Managers.Game.enemy.transform.position);
            if (distance <= GetAttackRange()) // Ensure turretRange is static in Define
            {
                target = Managers.Game.enemy;
            }
        }

        if (target != null && Vector3.Distance(transform.position, target.transform.position) <= GetAttackRange())
        {
            if (Time.time >= lastAttackTime + GetAttackSpeed())
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }

    protected virtual void Attack()
    {
        if (target == null || activeEffects.Any(a => a is MothPowderStun)) return;

        // 헤드가 타겟을 바라보도록 회전 (2D 기준)
        Vector3 dir = target.transform.position - head.transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        head.transform.rotation = Quaternion.Euler(0, 0, angle + headOffset);

        // 총알 생성
        var bullet = Managers.Resource.Instantiate("Bullet");
        bullet.transform.position = transform.position;

        bullets.Add(bullet);

        float bulletSpeed = 10f;
        StartCoroutine(BulletFollowTarget(bullet, target, bulletSpeed));

        var goldenChest = playerData.structures.Any(n => n.type == Define.StructureType.GoldenChest);

        int coinValue = 0;

        if (this is GoldenTurret)
            coinValue += (int)Managers.Game.GetStructureData(Define.StructureType.GoldenTurret).argment2[level];

        if (goldenChest)
            coinValue += level + 1;

        if (coinValue > 0)
        {
            playerData.AddCoin(coinValue);
            ResourceGetParticle(coinValue);
        }
    }

    private IEnumerator BulletFollowTarget(GameObject bullet, Enemy target, float speed)
    {
        while (bullet != null && target != null)
        {
            Vector3 dir = (target.transform.position - bullet.transform.position).normalized;
            float distance = Vector3.Distance(bullet.transform.position, target.transform.position);

            // 한 프레임 이동
            float move = speed * Time.deltaTime;
            if (move >= distance)
            {
                bullet.transform.position = target.transform.position;
                Managers.Resource.Destroy(bullet);
                bullets.Remove(bullet);
                target.Hit((int)Managers.Game.GetStructureData(type).argment1[level]);

                Managers.Audio.PlaySound("snd_tower_hit", target.transform, minRangeVolumeMul: -0.4f);

                yield break;
            }
            else
            {
                bullet.transform.position += dir * move;
            }
            yield return null;
        }

        // 타겟이 사라졌거나 총알이 파괴된 경우
        if (bullet != null)
        {
            Managers.Resource.Destroy(bullet);
            bullets.Remove(bullet);
        }
    }

    public override void Upgrade()
    {
        base.Upgrade();
        var data = Managers.Resource.GetStructureData(type);

        if (body == null || head == null)
        {
            body = gameObject.FindRecursive("Body");
            head = gameObject.FindRecursive("Head");
        }

        if (type != Define.StructureType.AutoTurret && type != Define.StructureType.GoldenTurret)
        {
            body.GetComponent<SpriteRenderer>().sprite = data.sprite1[level];
            head.GetComponent<SpriteRenderer>().sprite = data.sprite2[level];
        }
    }

    public void ResourceGetParticle(int value)
    {
        var particle = Managers.Resource.Instantiate("ResourceGetParticle", transform);
        particle.transform.localPosition = Vector3.zero;
        particle.GetComponent<ResourceGetParticle>().Setting(
            "coin",
            value,
            0
        );
    }

    public float GetAttackRange()
    {
        float range = Managers.Game.GetStructureData(type).argment2[level];

        range = (playerData.structures.Find(n => n.type == Define.StructureType.Telescope) != null) ? (range * 1.2f) : range;

        return range;
    }

    public virtual float GetAttackSpeed()
    {
        float coolDown = attackCooldown;

        if (playerData.type == Define.CharactorType.Chef)
        {
            coolDown *= 0.85f;
        }

        // TurretBooster가 있을 때만 거리 기반 쿨다운 보정
        if (playerData.structures.Find(n => n.type == Define.StructureType.TurretBooster) != null && target != null)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            float range = GetAttackRange();

            // 거리가 가까울수록 쿨다운 감소 (최소 0.25배까지)
            float ratio = Mathf.Clamp01(distance / range);
            float minRate = 0.25f;
            float rate = Mathf.Lerp(minRate, 1f, ratio); // 가까울수록 minRate, 멀수록 1
            coolDown *= rate;
        }

        // SatelliteAntenna가 있으면 쿨다운 절반
        if (playerData.structures.Find(n => n.type == Define.StructureType.SatelliteAntenna) != null)
        {
            coolDown *= 0.5f;
        }

        if (activeEffects.Any(a => a is OverHeat))
        {
            coolDown *= 0.5f;
        }

        return coolDown;
    }

    void OnDisable()
    {
        foreach (var bullet in bullets)
        {
            if (bullet != null)
                Managers.Resource.Destroy(bullet);
        }
    }
}
