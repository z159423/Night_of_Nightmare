using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : Structure
{
    private GameObject net;
    private float cooldown = 120f;
    private float lastUsedTime = -999f;
    private Coroutine trapRoutine;

    private Enemy target;

    protected override void Start()
    {
        base.Start();

        net = gameObject.FindRecursive("Net");

        trapRoutine = StartCoroutine(TrapCheckRoutine());
    }

    private IEnumerator TrapCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            target = Managers.Game.enemy;
            if (target == null) continue;

            float distance = Vector3.Distance(transform.position, target.transform.position);

            // target의 hp가 max hp의 22% 이하일 때만 발동
            bool isLowHp = target.hp <= target.MaxHp * 0.22f;

            // 쿨타임 체크, 거리 체크, hp 체크
            if (Time.time - lastUsedTime >= cooldown && distance <= 5f && isLowHp)
            {
                lastUsedTime = Time.time;
                if (net != null) net.SetActive(false);

                StartCoroutine(FireNetBullet(target));
            }
        }
    }

    private IEnumerator FireNetBullet(Enemy target)
    {
        // NetBullet 생성 및 초기화
        GameObject bullet = Managers.Resource.Instantiate("NetBullet");
        bullet.transform.position = transform.position;

        Managers.Audio.PlaySound("snd_sword_swing", minRangeVolumeMul: -1f);

        float flightTime = 0.6f; // 포물선 비행 시간
        float elapsed = 0f;
        Vector2 start = transform.position;
        Vector2 end = target.transform.position;
        float height = 1f; // 포물선 최고점(y축)

        Quaternion originalRotation = bullet.transform.rotation; // 원래 회전값 저장

        while (elapsed < flightTime)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                Managers.Resource.Destroy(bullet);
                yield break;
            }

            // 타겟의 현재 위치를 계속 추적 (2D)
            end = target.transform.position;

            float t = elapsed / flightTime;
            // 2D 포물선 공식 (x, y)
            Vector2 pos = Vector2.Lerp(start, end, t);
            pos.y += Mathf.Sin(Mathf.PI * t) * height;
            bullet.transform.position = new Vector3(pos.x, pos.y, bullet.transform.position.z);

            // 회전값을 항상 원래대로 유지
            bullet.transform.rotation = originalRotation;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 도착 처리 (여기서 메소드 호출, 예: OnNetBulletArrived(target);)
        // 예시: OnNetBulletArrived(target);

        if (target != null)
        {
            target.AddEffect(new StunEffect(2f));
            Managers.Audio.PlaySound("snd_tower_hit", target.transform, minRangeVolumeMul: 0.4f);
        }

        yield return new WaitForSeconds(2f); // 잠시 대기 후
        Managers.Resource.Destroy(bullet);

        // 쿨타임 후 net 오브젝트 재활성화
        yield return new WaitForSeconds(cooldown);
        if (net != null) net.SetActive(true);
    }

    public override void DestroyStructure()
    {
        if (trapRoutine != null)
            StopCoroutine(trapRoutine);
        base.DestroyStructure();
    }
}
