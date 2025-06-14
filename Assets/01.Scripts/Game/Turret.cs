using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Turret : Structure
{
    public Enemy target;
    public float attackRange = 5f;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    GameObject body;
    GameObject head;

    StructureData turretData;

    float headOffset = -90;

    protected override void Start()
    {
        base.Start();
        lastAttackTime = Time.time;

        body = gameObject.FindRecursive("Body");
        head = gameObject.FindRecursive("Head");

        turretData = Managers.Resource.GetStructureData(type);
    }

    private void Update()
    {
        if (target == null && Managers.Game.enemy != null)
        {
            // 타겟이 없으면 120px(=1.2f) 범위 내의 enemy를 찾음
            float distance = Vector3.Distance(transform.position, Managers.Game.enemy.transform.position);
            if (distance <= Define.turretRange) // Ensure turretRange is static in Define
            {
                target = Managers.Game.enemy;
            }
        }

        if (target != null && Vector3.Distance(transform.position, target.transform.position) <= attackRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }

    private void Attack()
    {
        if (target == null) return;

        // 헤드가 타겟을 바라보도록 회전 (2D 기준)
        Vector3 dir = target.transform.position - head.transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        head.transform.rotation = Quaternion.Euler(0, 0, angle + headOffset);

        // 총알 생성
        var bullet = Managers.Resource.Instantiate("Bullet");
        bullet.transform.position = transform.position;

        float bulletSpeed = 10f;
        StartCoroutine(BulletFollowTarget(bullet, target, bulletSpeed));
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
                target.Hit((int)turretData.argment1[level]);
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
            Managers.Resource.Destroy(bullet);
    }

    public override void Upgrade()
    {
        base.Upgrade();
        var data = Managers.Resource.GetStructureData(type);

        body.GetComponent<SpriteRenderer>().sprite = data.sprite1[level];
        head.GetComponent<SpriteRenderer>().sprite = data.sprite2[level];
    }
}
