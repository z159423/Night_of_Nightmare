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
        var finds = allData.Where(sd => sd.lampProp > 0 && IsOnlyOne(sd.structureType) && !sd.DoNotSpawnInLamp).ToList();

        StructureData selectedData;

        if (Managers.LocalData.FirstUseLamp == 0)
        {
            selectedData = finds.First(n => n.structureType == Define.StructureType.MoneySack);

            Managers.LocalData.FirstUseLamp = 1;
        }
        else
        {
            // LampProp(확률 비중)으로 랜덤 선택
            float totalWeight = finds.Sum(sd => sd.lampProp);
            float rand = Random.Range(0f, totalWeight);
            float accum = 0f;
            selectedData = finds[0];
            foreach (var sd in finds)
            {
                accum += sd.lampProp;
                if (rand <= accum)
                {
                    selectedData = sd;
                    break;
                }
            }
        }


        bool IsOnlyOne(Define.StructureType type)
        {
            var find = playerData.structures.Find(n => n.type == type);
            if (find == null)
                return true;
            else if (Managers.Game.GetStructureData(type).onlyOnePurcahse)
                return false;

            return true;
        }

        var find = Managers.Resource.LoadAll<GameObject>("Structures");
        var find2 = find.First(n => n.GetComponentInChildren<Structure>() != null && n.GetComponentInChildren<Structure>().type == selectedData.structureType);
        var lampStructures = Instantiate(find2, transform.GetComponentInParent<Tile>().transform).GetComponentInChildren<Structure>();

        if (lampStructures.type == Define.StructureType.Turret)
        {
            // 3~6레벨 중 확률 분포에 따라 최대 업그레이드 레벨 결정
            int levelTo;
            float rand2 = Random.value;
            if (rand2 < 0.4f)
                levelTo = 2;
            else if (rand2 < 0.8f)
                levelTo = 3;
            else if (rand2 < 0.9f)
                levelTo = 4;
            else
                levelTo = 5;

            lampStructures.UpgradeTo(levelTo);
        }

        if (lampStructures.type == Define.StructureType.Generator)
        {
            int levelTo;
            float rand2 = Random.value;
            if (rand2 < 0.4f)
                levelTo = 0;
            else if (rand2 < 0.8f)
                levelTo = 1;
            else if (rand2 < 0.9f)
                levelTo = 2;
            else if (rand2 < 0.95f)
                levelTo = 3;
            else
                levelTo = 4;

            lampStructures.UpgradeTo(levelTo);
        }

        var particle = Managers.Resource.Instantiate("Particles/StructureProductParticle");
        particle.transform.position = transform.GetComponentInParent<Tile>().transform.position;

        StartCoroutine(destroy());

        IEnumerator destroy()
        {
            yield return new WaitForSeconds(1.2f);
            Managers.Resource.Destroy(particle);

            Managers.Audio.PlaySound("snd_skill_spawn_effect", minRangeVolumeMul: -1f);
        }

        playerData.BuildStructure(lampStructures);

        if (selectedData.structureType != Define.StructureType.MovingFrog)
            transform.GetComponentInParent<Tile>().currentStructure = lampStructures;
        else
            transform.GetComponentInParent<Tile>().currentStructure = null;

        GameObserver.Call(GameObserverType.Game.OnChangeStructure);

        DestroyStructure();
    }

}
