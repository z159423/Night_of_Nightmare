using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldenFrog : Structure
{
    private Transform mouth;
    private Transform tongue;
    private Transform body;
    private LineRenderer lineRenderer;

    private List<GameObject> flies = new List<GameObject>();
    private int flyCount = 1;
    private float flySpawnInterval = 7f;
    private float flyCircleRadius = 0.15f;
    private float flyCircleSpeed = 2f;
    private Coroutine flyRoutine;
    private Coroutine eatRoutine;

    private List<Tweener> flyRotationTweeners = new List<Tweener>();

    protected override void Start()
    {
        base.Start();

        mouth = gameObject.FindRecursive("Mouth").transform;
        tongue = gameObject.FindRecursive("Tongue").transform;
        body = gameObject.FindRecursive("Body").transform;
        lineRenderer = gameObject.FindRecursive("Mouth").GetComponent<LineRenderer>();

        flyRoutine = StartCoroutine(SpawnFliesRoutine());
    }

    protected override void Update()
    {
        base.Update();
        if (lineRenderer != null && mouth != null && tongue != null)
        {
            lineRenderer.SetPosition(0, mouth.position);
            lineRenderer.SetPosition(1, tongue.position);
        }

        // 파리들은 위치만 원을 그리며 움직임 (회전은 DOTween으로 처리)
        for (int i = 0; i < flies.Count; i++)
        {
            if (flies[i] == null) continue;
            var fly = flies[i];
            var flyData = fly.GetComponent<FlyData>();
            if (flyData == null) continue;

            float angle = Time.time * flyCircleSpeed + flyData.angleOffset;
            Vector3 center = flyData.centerPos;
            float radius = flyCircleRadius;
            Vector3 newPos = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;

            fly.transform.localPosition = newPos;
            flyData.lastPos = newPos;
        }
    }

    private IEnumerator SpawnFliesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(flySpawnInterval);

            // 파리 개수 증가 (최대 9, 이후 1로)
            flyCount++;
            if (flyCount > 9) flyCount = 1;

            // 기존 파리 제거 및 트윈 정리
            foreach (var fly in flies)
            {
                if (fly != null) Destroy(fly);
            }
            flies.Clear();

            foreach (var tw in flyRotationTweeners)
            {
                if (tw != null && tw.IsActive()) tw.Kill();
            }
            flyRotationTweeners.Clear();

            // 파리 생성
            for (int i = 0; i < flyCount; i++)
            {
                Vector3 localPos = new Vector3(
                    Random.Range(-0.6f, 0.6f),
                    Random.Range(1f, 1.4f),
                    0
                );
                GameObject fly = Managers.Resource.Instantiate("Fly", transform);
                fly.transform.localPosition = localPos;
                var flyData = fly.AddComponent<FlyData>();
                flyData.centerPos = localPos;
                flyData.angleOffset = Random.Range(0f, Mathf.PI * 2f);
                flyData.lastPos = localPos;
                flies.Add(fly);

                // DOTween으로 360도 회전 트윈 추가 (1바퀴 도는 시간은 2*PI/flyCircleSpeed)
                // z축 90도 오프셋 적용
                float duration = (2 * Mathf.PI) / flyCircleSpeed;
                Tweener tw = fly.transform.DOLocalRotate(
                    new Vector3(0, 0, 360f), // 90도 오프셋
                    duration,
                    RotateMode.FastBeyond360
                )
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
                flyRotationTweeners.Add(tw);

                yield return new WaitForSeconds(0.2f); // 파리 생성 간격
            }

            yield return new WaitForSeconds(0.5f); // 파리 생성 후 잠시 대기

            // 파리 먹기 루틴 시작
            if (eatRoutine != null)
                StopCoroutine(eatRoutine);
            eatRoutine = StartCoroutine(EatFliesRoutine());
        }
    }

    private IEnumerator EatFliesRoutine()
    {
        foreach (var fly in new List<GameObject>(flies))
        {
            if (fly == null) continue;
            yield return StartCoroutine(EatFly(fly));

            yield return new WaitForSeconds(0.1f); // 파리 먹은 후 잠시 대기
        }
    }

    private IEnumerator EatFly(GameObject fly)
    {
        if (mouth == null || tongue == null || fly == null)
            yield break;

        Vector3 tongueOriginPos = tongue.position;
        Vector3 start = tongueOriginPos;
        Vector3 end = fly.transform.position;
        float speed = 12f;
        float distance = Vector3.Distance(start, end);
        float duration = distance / speed;
        float t = 0f;

        // 혓바닥 늘리기
        while (t < 1f)
        {
            if (fly == null) yield break;
            t += Time.deltaTime / duration;
            tongue.position = Vector3.Lerp(start, end, t);

            // 혓바닥이 파리에 닿으면
            if (Vector3.Distance(tongue.position, fly.transform.position) < 0.05f)
            {
                // 파리 먹는 이벤트 (골드 획득 등)
                OnEatFly();

                flies.Remove(fly);
                Managers.Resource.Destroy(fly);
                break;
            }
            yield return null;
        }

        // 혓바닥 원위치
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

        tongue.localPosition = new Vector3(0, 0, 0);
    }

    public void ResourceGetParticle(int value)
    {
        var particle = Managers.Resource.Instantiate("ResourceGetParticle", transform);
        particle.transform.localPosition = Vector3.zero + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
        particle.GetComponent<ResourceGetParticle>().Setting(
            "coin",
            value,
            0
        );
    }

    // 파리 먹었을 때 호출되는 이벤트 (골드 획득 등)
    private void OnEatFly()
    {
        var gold = (int)Managers.Game.GetStructureData(Define.StructureType.GoldenFrog).argment1[level]; // 예시로 1골드 획득

        playerData.AddCoin(gold);
        ResourceGetParticle(gold);

        Managers.Audio.PlaySound("snd_get", transform, minRangeVolumeMul: 0.4f);
    }

    // 파리 움직임 데이터용 컴포넌트
    private class FlyData : MonoBehaviour
    {
        public Vector3 centerPos;
        public float angleOffset;
        public Vector3 lastPos;
    }
}
