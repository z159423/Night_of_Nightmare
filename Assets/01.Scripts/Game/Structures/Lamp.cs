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
            for (int i = 0; i < Random.Range(3, 7); i++)
            {
                lampStructures.Upgrade();
            }
        }

        if (lampStructures is Generator)
        {
            for (int i = 0; i < Random.Range(1, 6); i++)
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
