using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : Structure
{
    private float cooldown = 180f;
    private float lastUsedTime = -999f;
    private Coroutine axeRoutine;

    protected override void Start()
    {
        base.Start();
        axeRoutine = StartCoroutine(AxeAttackRoutine());
    }

    private IEnumerator AxeAttackRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            var enemy = Managers.Game.enemy;
            if (enemy == null) continue;

            // 적의 체력이 30% 이하일 때만 발동
            if (Time.time - lastUsedTime >= cooldown && enemy.hp <= enemy.MaxHp * 0.3f)
            {
                lastUsedTime = Time.time;
                StartCoroutine(FireAxeBullet(enemy));
            }
        }
    }

    private IEnumerator FireAxeBullet(Enemy target)
    {
        GameObject axeBullet = Managers.Resource.Instantiate("AxeBullet", transform);
        axeBullet.transform.position = transform.position;

        float speed = 5f;
        float minDistance = 0.1f;
        float rotateSpeed = 600f; // 1초에 2바퀴

        while (true)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                Managers.Resource.Destroy(axeBullet);
                yield break;
            }

            // 방향 및 이동
            Vector3 dir = (target.transform.position - axeBullet.transform.position);
            float distance = dir.magnitude;

            if (distance < minDistance)
            {
                // 도착 시 데미지
                int damage = Mathf.RoundToInt(target.MaxHp * 0.15f);
                target.Hit(damage, false);

                Managers.Audio.PlaySound("snd_cutter", target.transform);

                Managers.Resource.Destroy(axeBullet);
                yield break;
            }

            axeBullet.transform.position += dir.normalized * speed * Time.deltaTime;
            axeBullet.transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);

            yield return null;
        }
    }
}
