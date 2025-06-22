using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneySack : Structure
{
    private int goldPerDrop;
    private int dropCount;
    private int currentDrop = 0;
    private Coroutine dropRoutine;

    protected override void Start()
    {
        base.Start();

        goldPerDrop = Random.Range(20, 41); // 20~40 골드
        dropCount = Random.Range(10, 101);  // 10~100회

        dropRoutine = StartCoroutine(DropGoldRoutine());
    }

    private IEnumerator DropGoldRoutine()
    {
        yield return new WaitForSeconds(1); // 약간의 딜레이 후 시작
        while (currentDrop < dropCount)
        {
            ResourceGetParticle(goldPerDrop);
            Managers.Game.playerData.AddCoin(goldPerDrop);
            currentDrop++;
            yield return new WaitForSeconds(0.14f);
        }

        // 모든 돈이 나오면 건축물 파괴
        RemoveThisStructrue();
        DestroyStructure();
    }

    public void ResourceGetParticle(int value)
    {
        var particle = Managers.Resource.Instantiate("ResourceGetParticle");
        particle.transform.position = transform.position + (Vector3.up * 0.2f);
        particle.GetComponent<ResourceGetParticle>().Setting(
            "coin",
            value,
            0, Random.Range(0.875f, 1.125f), Random.Range(-0.3f, 0.3f), 0.4f
        );
    }
}
