using System.Collections;
using UnityEngine;
using DG.Tweening;
using System;

public class SpellBlocker : Structure
{
    [SerializeField] private const float coolTime = 30f;
    [SerializeField] private const float bulletSpeed = 4f;
    [SerializeField] private GameObject blockerDeco;

    private float usedTime = -999f;

    private float offset = -90f;

    private GameObject blockerBullet;

    protected override void Start()
    {
        base.Start();

        blockerDeco = gameObject.FindRecursive("Blocker");
    }

    /// <summary>
    /// SpellBlock 발동 시도 (target: 타겟 Transform)
    /// </summary>
    public void TryCastSpellBlock(Enemy target, Action spellDeactiveAction)
    {
        // 쿨타임 체크
        if (Time.time - usedTime < coolTime) return;

        // 30% 확률 체크
        if (UnityEngine.Random.value < 0.3f) return;

        // 발동
        usedTime = Time.time;
        StartCoroutine(SpellBlockRoutine(target, spellDeactiveAction));
    }

    private IEnumerator SpellBlockRoutine(Enemy target, Action spellDeactiveAction)
    {
        if (target == null)
            yield break;

        blockerBullet = Managers.Resource.Instantiate("SpellBlockerBullet");
        blockerBullet.transform.position = transform.position;

        GameObject icon = blockerBullet.FindRecursive("Icon");
        GameObject particle = blockerBullet.FindRecursive("Particle");
        GameObject particle2 = blockerBullet.FindRecursive("Particle2");

        icon.SetActive(true);

        // 파티클 시스템 가져오기
        ParticleSystem ps = particle != null ? particle.GetComponent<ParticleSystem>() : null;
        if (ps != null)
            ps.Play(); // 파티클 생성 시작

        blockerDeco.SetActive(false);

        float minDistance = 0.1f;
        bool hit = false;

        while (!hit && blockerBullet.activeSelf)
        {
            // target이 사라진 경우
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                if (ps != null) ps.Stop(); // 파티클 중단
                Managers.Resource.Destroy(blockerBullet);
                yield break;
            }

            // 방향 계산 및 회전
            Vector3 dir = (target.transform.position - blockerBullet.transform.position);
            float distance = dir.magnitude;
            if (distance > 0.001f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                blockerBullet.transform.rotation = Quaternion.Euler(0, 0, angle + offset);

                // 이동
                float move = bulletSpeed * Time.deltaTime;
                if (move >= distance)
                {
                    blockerBullet.transform.position = target.transform.position;
                }
                else
                {
                    blockerBullet.transform.position += dir.normalized * move;
                }
            }

            // 도달 체크
            if (distance < minDistance)
            {
                Managers.Audio.PlaySound("snd_tower_hit", transform, minRangeVolumeMul: -0.4f);

                hit = true;
                spellDeactiveAction?.Invoke();
                if (ps != null) ps.Stop(); // 파티클 중단
                particle2.SetActive(true);
                icon.gameObject.SetActive(false);

                yield return new WaitForSeconds(2f);
                particle2.SetActive(false);
                Managers.Resource.Destroy(blockerBullet);
                break;
            }
            yield return null;
        }

        // 쿨타임 동안 비활성화, 쿨타임 후 재활성화
        yield return new WaitForSeconds(coolTime - (Time.time - usedTime));
        blockerDeco.SetActive(true);
    }
}
