using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Lamp : Structure
{
    protected override void Start()
    {
        base.Start();

        StartCoroutine(LampEffect());
    }

    public IEnumerator LampEffect()
    {
        playerData.buyLampCount++;

        yield return new WaitForSeconds(1f);

        // StructureData 폴더 전체에서 Lamp 타입만 필터링
        var allData = Managers.Resource.LoadAll<StructureData>("StructureData");
        var finds = allData.Where(sd => sd.lampProp > 0 && IsOnlyOne(sd.structureType)).ToList();

        // LampProp(확률 비중)으로 랜덤 선택
        float totalWeight = finds.Sum(sd => sd.lampProp);
        float rand = Random.Range(0f, totalWeight);
        float accum = 0f;
        StructureData selectedData = finds[0];
        foreach (var sd in finds)
        {
            accum += sd.lampProp;
            if (rand <= accum)
            {
                selectedData = sd;
                break;
            }
        }

        bool IsOnlyOne(Define.StructureType type)
        {
            var find = playerData.structures.Find(n => n.type == type);
            if (find == null)
                return true;
            else if (Managers.Game.GetStructureData(type).onlyOnePurcahse)
                return false;

            return false;
        }

        var find = Managers.Resource.LoadAll<GameObject>("Structures");
        var find2 = find.First(n => n.GetComponentInChildren<Structure>() != null && n.GetComponentInChildren<Structure>().type == selectedData.structureType);
        var lampStructures = Instantiate(find2, transform.GetComponentInParent<Tile>().transform).GetComponentInChildren<Structure>();

        if (lampStructures is Turret)
        {
            // 3~6레벨 중 확률 분포에 따라 최대 업그레이드 레벨 결정
            int maxLevel;
            float rand2 = Random.value;
            if (rand2 < 0.4f)
                maxLevel = 1;
            else if (rand2 < 0.8f)
                maxLevel = 2;
            else if (rand2 < 0.9f)
                maxLevel = 3;
            else
                maxLevel = 4;

            int upgradeCount = Mathf.Max(0, 3 + maxLevel);

            for (int i = 0; i < upgradeCount; i++)
            {
                lampStructures.Upgrade();
            }
        }

        if (lampStructures is Generator)
        {
            int maxLevel;
            float rand2 = Random.value;
            if (rand2 < 0.4f)
                maxLevel = 1;
            else if (rand2 < 0.8f)
                maxLevel = 2;
            else if (rand2 < 0.9f)
                maxLevel = 3;
            else if (rand2 < 0.95f)
                maxLevel = 4;
            else
                maxLevel = 5;

            for (int i = 0; i < Random.Range(0, maxLevel); i++)
            {
                lampStructures.Upgrade();
            }
        }

        var particle = Managers.Resource.Instantiate("Particles/StructureProductParticle");
        particle.transform.position = transform.GetComponentInParent<Tile>().transform.position;

        StartCoroutine(destroy());

        IEnumerator destroy()
        {
            yield return new WaitForSeconds(1.2f);
            Managers.Resource.Destroy(particle);
        }

        Managers.Game.playerData.BuildStructure(lampStructures);

        if (selectedData.structureType != Define.StructureType.MovingFrog)
            transform.GetComponentInParent<Tile>().currentStructure = lampStructures;
        else
            transform.GetComponentInParent<Tile>().currentStructure = null;

        GameObserver.Call(GameObserverType.Game.OnChangeStructure);

        DestroyStructure();
    }

}
