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
        yield return new WaitForSeconds(1f);

        // StructureData 폴더 전체에서 Lamp 타입만 필터링
        var allData = Managers.Resource.LoadAll<StructureData>("StructureData");
        var finds = allData.Where(sd => sd.lampProp > 0).ToList();

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

        var find = Managers.Resource.LoadAll<GameObject>("Structures")
            .First(n => n.GetComponentInChildren<Structure>() != null && n.GetComponentInChildren<Structure>().type == selectedData.structureType);
        var lampStructures = Instantiate(find, transform.GetComponentInParent<Tile>().transform).GetComponentInChildren<Structure>();

        var particle = Managers.Resource.Instantiate("Particles/StructureProductParticle");
        particle.transform.position = transform.GetComponentInParent<Tile>().transform.position;

        StartCoroutine(destroy());

        IEnumerator destroy()
        {
            yield return new WaitForSeconds(1.2f);
            Managers.Resource.Destroy(particle);
        }

        Managers.Game.playerData.BuildStructure(lampStructures);

        transform.GetComponentInParent<Tile>().currentStructure = lampStructures;

        GameObserver.Call(GameObserverType.Game.OnChangeStructure);

        DestroyStructure();
    }

}
