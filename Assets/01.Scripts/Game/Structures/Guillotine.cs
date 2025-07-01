using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guillotine : Structure
{
    private GameObject cutter;
    private float cooldown = 120f;
    private float lastUsedTime = -999f;
    private Coroutine guillotineRoutine;
    private Enemy target;

    protected override void Start()
    {
        base.Start();
        cutter = gameObject.FindRecursive("Cutter").gameObject;
        guillotineRoutine = StartCoroutine(GuillotineCheckRoutine());
    }

    private IEnumerator GuillotineCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            target = Managers.Game.enemy;
            if (target == null) continue;

            float distance = Vector3.Distance(transform.position, target.transform.position);
            bool isLowHp = target.hp <= target.MaxHp * 0.2f;

            // 쿨타임, 거리, hp 체크
            if (Time.time - lastUsedTime >= cooldown && distance <= 5f && isLowHp)
            {
                lastUsedTime = Time.time;
                if (cutter != null) cutter.SetActive(false);

                // 이벤트 발생 위치 (여기서 원하는 이벤트 호출)
                // 예: OnGuillotineTriggered(target);

                StartCoroutine(FireCutterBullet(target));

                // 쿨타임 후 cutter 오브젝트 재활성화
                StartCoroutine(GuillotineCooldownRoutine());
            }
        }
    }

    private IEnumerator GuillotineCooldownRoutine()
    {
        yield return new WaitForSeconds(cooldown);
        if (cutter != null) cutter.SetActive(true);
    }

    private IEnumerator FireCutterBullet(Enemy target)
    {
        if (target == null || !target.gameObject.activeInHierarchy)
            yield break;

        GameObject cutterBullet = Managers.Resource.Instantiate("GuillotineCutterBullet");
        Vector3 startPos = target.transform.position + new Vector3(0, 4f, 0);
        cutterBullet.transform.position = startPos;

        Managers.Audio.PlaySound("snd_sword_swing", minRangeVolumeMul: -1f);

        float speed = 7f;
        float slowSpeed = 4f;
        float minDistance = 0.1f;

        bool hitted = false;

        while (true)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                Managers.Resource.Destroy(cutterBullet);
                yield break;
            }

            Vector3 targetPos = target.transform.position;
            Vector3 bulletPos = cutterBullet.transform.position;

            float distance = Mathf.Abs(bulletPos.y - targetPos.y);

            // target과의 y축 거리가 1.5 이하이면 속도 절반
            float moveSpeed = distance <= 1.5f ? slowSpeed : speed;

            // y축만 감소시켜서 target을 향해 이동
            float newY = Mathf.MoveTowards(bulletPos.y, targetPos.y, moveSpeed * Time.deltaTime);
            cutterBullet.transform.position = new Vector3(targetPos.x, newY, bulletPos.z);

            if (!hitted && Mathf.Abs(newY - targetPos.y) < 0.8f)
            {
                hitted = true;

                int damage = Mathf.RoundToInt(target.MaxHp * 0.12f);
                target.Hit(damage, false);

                Managers.Audio.PlaySound("snd_cutter", cutterBullet.transform);

            }

            // 도착 체크
            if (Mathf.Abs(newY - targetPos.y) < minDistance)
            {
                // 도착 시 target에게 최대 체력의 12% 데미지
                Managers.Resource.Destroy(cutterBullet);
                yield break;
            }

            yield return null;
        }
    }

    public override void DestroyStructure()
    {
        if (guillotineRoutine != null)
            StopCoroutine(guillotineRoutine);
        base.DestroyStructure();
    }
}
