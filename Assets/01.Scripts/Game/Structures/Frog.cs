using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frog : Structure
{
    private Transform mouth;
    private Transform tongue;
    protected Transform body;
    private LineRenderer lineRenderer;

    private Vector3 tongueOriginPos;
    public float attackCooldown = 2f;
    public float lastAttackTime = -999f;
    public Coroutine attackRoutine;

    protected Action attackStartAction;
    protected Action attackEndAction;
    protected Action<Enemy> attackHitAction;

    protected override void Start()
    {
        base.Start();

        mouth = gameObject.FindRecursive("Mouth").transform;
        tongue = gameObject.FindRecursive("Tongue").transform;
        body = gameObject.FindRecursive("Body").transform;
        lineRenderer = gameObject.FindRecursive("Mouth").GetComponent<LineRenderer>();

        StartCoroutine(FrogAttackCheckRoutine());
    }

    protected override void Update()
    {
        base.Update();
        if (lineRenderer != null && mouth != null && tongue != null)
        {
            lineRenderer.SetPosition(0, mouth.position);
            lineRenderer.SetPosition(1, tongue.position);
        }
    }

    private IEnumerator FrogAttackCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            var target = Managers.Game.enemy;
            if (target == null) continue;

            float distance = Vector3.Distance(transform.position, target.transform.position);

            if (Time.time - lastAttackTime >= attackCooldown && distance < 8f)
            {
                lastAttackTime = Time.time;
                if (attackRoutine != null)
                    StopCoroutine(attackRoutine);
                attackRoutine = StartCoroutine(Attack(target));
            }
        }
    }

    public IEnumerator Attack(Enemy target)
    {
        if (mouth == null || tongue == null || target == null)
            yield break;

        tongueOriginPos = tongue.position; // 원래 위치 저장

        Vector3 start = tongueOriginPos;
        Vector3 end = target.transform.position;
        float speed = 16f; // 원하는 속도(거리/초)
        float distance = Vector3.Distance(start, end);
        float duration = distance / speed;
        float t = 0f;

        var enemy = Managers.Game.enemy;
        var player = transform;
        if (body != null && enemy != null && player != null)
        {
            float rotY = enemy.transform.position.x < player.transform.position.x ? 180f : 0f;
            Vector3 euler = body.eulerAngles;
            euler.y = rotY;
            body.eulerAngles = euler;
        }

        // 공격 시작 액션 호출
        attackStartAction?.Invoke();

        // tongue를 타겟까지 늘리기
        while (t < 1f)
        {
            if (target == null) yield break;
            t += Time.deltaTime / duration;
            tongue.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        // 적중 시 데미지
        if (target != null)
        {
            target.Hit((int)Managers.Game.GetStructureData(Define.StructureType.Frog).argment1[0], false);
            attackHitAction?.Invoke(target);

            Managers.Audio.PlaySound("snd_tower_hit", target.transform, minRangeVolumeMul: 0.4f);
        }

        // tongue를 원래 위치로 되돌리기
        t = 0f;
        Vector3 backStart = tongue.position;
        Vector3 backEnd = tongueOriginPos;
        float backDistance = Vector3.Distance(backStart, backEnd);
        float backDuration = backDistance / speed;

        while (t < 1f)
        {
            t += Time.deltaTime / backDuration;
            tongue.position = Vector3.Lerp(backStart, backEnd, t);
            yield return null;
        }

        tongue.localPosition = new Vector3(0, 0, 0); // 원래 위치로 되돌리기

        // 공격 종료 액션 호출
        attackEndAction?.Invoke();
    }
}
