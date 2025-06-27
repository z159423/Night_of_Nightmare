using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static Define;


public class AiCharactor : PlayerableCharactor
{
    enum CharactorAction
    {
        BedUpgrade,
        DoorUpgrade,
        BuildStructure,
        Wait
    }

    enum BuildStructureType
    {
        Turret,
        Generator,
        Ore,
        Special
    }


    Dictionary<CharactorAction, float> chractorActionProp = new Dictionary<CharactorAction, float>()
    {
        { CharactorAction.BedUpgrade, 5f},
        { CharactorAction.DoorUpgrade, 3f },
        { CharactorAction.BuildStructure, 4f },
        { CharactorAction.Wait, 3f }
    };

    Dictionary<BuildStructureType, float> buildStructureProp = new Dictionary<BuildStructureType, float>()
    {
        { BuildStructureType.Turret, 0.7f},
        { BuildStructureType.Generator, 54.3f },
        { BuildStructureType.Ore, 35f },
        { BuildStructureType.Special, 10f }
    };

    Dictionary<StructureType, float> buildOreProp = new Dictionary<StructureType, float>()
    {
        { StructureType.CopperOre, 15f},
        { StructureType.SilverOre, 20f },
        { StructureType.GoldOre, 35f },
        { StructureType.DiamondOre, 30f }
    };

    Dictionary<StructureType, float> buildSpecialProp = new Dictionary<StructureType, float>()
    {
        { StructureType.Lamp, 0.5f},
        { StructureType.RepairStation, 8.5f },
        { StructureType.SpellBlocker,5.7f },
        { StructureType.EnergyShield, 7.1f },
        { StructureType.GoldenChest, 8.2f },
        { StructureType.Cooler, 10f },
        { StructureType.Trap, 10f },
        { StructureType.ThornBush, 10f },
        { StructureType.Guillotine, 10f },
        { StructureType.Telescope, 10f },
        { StructureType.SatelliteAntenna, 10f },
        { StructureType.TurretBooster, 10f },
    };

    Coroutine doorRepairCoroutine;
    Coroutine charactorStateMachine;
    Coroutine freeDoorUpgradeCoroutine;


    public void SettingAiCharactor(Define.CharactorType charactorType)
    {
        this.charactorType = charactorType;

        SetBodySkin();

        if (Managers.UI._currentScene is UI_GameScene_Map)
            Managers.UI._currentScene.GetComponent<UI_GameScene_Map>().SetCharactorIcon(charactorType);

        GetComponentInParent<NavMeshAgent>(true).enabled = true;

        playerData = new PlayerData(charactorType);
        Managers.Game.aiPlayerDatas.Add(playerData);
    }

    public void ActiveAiCharactor(float waitTime)
    {
        Managers.Game.charactors.Add(this);

        StartCoroutine(wait());

        IEnumerator wait()
        {
            yield return new WaitForSeconds(waitTime);
            FindBed();
        }
    }

    protected override void Update()
    {
        base.Update();
        // NavMeshAgent가 할당되어 있는지 확인
        if (agent != null)
        {
            // agent가 경로를 따라 이동 중이면 OnMove 호출
            if (agent.hasPath && agent.remainingDistance > agent.stoppingDistance)
            {
                OnMove();
            }
            // 목적지에 도달했거나 이동 중이 아니면 OnMoveStop 호출
            else
            {
                OnMoveStop();
            }
        }

        if (currentFindBed != null && currentFindBed.active)
        {
            FindBed();
        }
    }

    public void FindBed()
    {
        List<Bed> inactiveBeds = Managers.Game.beds.FindAll(bed => !bed.active);
        if (inactiveBeds.Count > 0)
        {
            int randomIndex = Random.Range(0, inactiveBeds.Count);
            currentFindBed = inactiveBeds[randomIndex];

            ModeToBed();
        }
        else
        {
            currentFindBed = null;
        }
    }

    public void ModeToBed()
    {
        if (agent != null)
        {
            agent.SetDestination(currentFindBed.transform.position);
        }
        else
        {
            Debug.LogWarning("NavMeshAgent is not assigned.");
        }
    }

    public override void Hit(int damage)
    {
        // AI 캐릭터는 Hit 메서드를 구현하지 않음
        // 필요시 AI 캐릭터의 행동을 정의할 수 있음

        Die();
    }

    public override void Die()
    {
        //죽으면 방에 있는 모든 건축물 파괴
        //플레이어 리스트에서 삭제
        StopCoroutine(doorRepairCoroutine);
        StopCoroutine(charactorStateMachine);
        StopCoroutine(freeDoorUpgradeCoroutine);

        die = true;
        Managers.Resource.Destroy(gameObject);
    }

    protected override void OnActiveRoom(Bed bed)
    {
        doorRepairCoroutine = bed.StartCoroutine(CheckSelfDoorRepair());
        charactorStateMachine = bed.StartCoroutine(CharactorStateMachine());
        freeDoorUpgradeCoroutine = bed.StartCoroutine(CheckDoorFreeUpgrade());

        base.OnActiveRoom(bed);
    }

    IEnumerator CharactorStateMachine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1); // 다음 프레임까지 대기

            CharactorAction action = GetRandomByProp(chractorActionProp);

            switch (action)
            {
                case CharactorAction.BedUpgrade:
                    UpgradeBed();
                    break;
                case CharactorAction.DoorUpgrade:
                    UpgradeDoor();
                    break;
                case CharactorAction.BuildStructure:
                    BuildStructure();
                    break;
                case CharactorAction.Wait:
                    // 대기 상태는 아무 동작도 하지 않음
                    break;
            }
        }
    }

    private void UpgradeStructure<T>(StructureType type, T structure, int level) where T : Structure
    {
        var structureData = Managers.Game.GetStructureData(type);

        if (structure != null && !structure.destroyed && structureData.CanUpgrade(playerData, level))
        {
            structure.Upgrade();
            playerData.UseResource(structureData.GetPurchaseCoin(level, playerData), structureData.GetPurchaseEnergy(level, playerData));

#if UNITY_EDITOR
            Debug.Log($"{charactorType} upgraded {type} at {currentFindBed?.transform.position}");
#endif
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{charactorType} tried to upgrade a {type}, but upgrade conditions were not met.");
#endif

        }
    }

    private void UpgradeBed()
    {
        int level = playerData.room.bed.level + 1;
        UpgradeStructure(StructureType.Bed, currentActiveRoom.bed, level);
    }

    private void UpgradeDoor()
    {
        int level = playerData.room.door.level + 1;
        UpgradeStructure(StructureType.Door, currentActiveRoom.door, level);
    }

    private void BuildStructure()
    {
        Tile[] tiles = playerData.room.tiles.Where(n => n.currentStructure == null).ToArray();

        BuildStructureType structureType = GetRandomByProp(buildStructureProp);

        tiles = tiles.OrderBy(n => Random.value).ToArray(); // 타일을 랜덤하게 섞기

        StructureType type = StructureType.Turret;

        switch (structureType)
        {
            case BuildStructureType.Turret:
                type = StructureType.Turret;
                break;
            case BuildStructureType.Generator:
                type = StructureType.Generator;
                break;
            case BuildStructureType.Ore:
                type = GetRandomByProp(buildOreProp);
                break;
            case BuildStructureType.Special:
                type = GetRandomByProp(buildSpecialProp);
                break;
        }

        foreach (var tile in tiles)
            if (Managers.Game.GetStructureData(type).CanPurchase(playerData, out string reason, 0))
                BuildStructure(type, tile);
    }

    private void BuildStructure(StructureType type, Tile tile)
    {
        bool success = Managers.Game.BuildStructure(playerData, type, tile);

#if UNITY_EDITOR
        if (success)
        {
            Debug.Log($"{charactorType} successfully built {type} at {tile.transform.position}");
        }
        else
        {
            Debug.LogWarning($"{charactorType} failed to build {type} at {tile.transform.position}");
        }
#endif
    }

    // 확률 비중에 따라 랜덤으로 하나를 뽑는 제네릭 메소드
    private T GetRandomByProp<T>(Dictionary<T, float> propDict)
    {
        float total = 0f;
        foreach (var v in propDict.Values)
            total += v;

        float rand = Random.Range(0f, total);
        float accum = 0f;
        foreach (var kv in propDict)
        {
            accum += kv.Value;
            if (rand <= accum)
                return kv.Key;
        }
        // 혹시라도 못 뽑으면 첫 번째 값 반환
        foreach (var kv in propDict)
            return kv.Key;
        return default;
    }

    public IEnumerator CheckSelfDoorRepair()
    {
        while (true)
        {
            yield return new WaitForSeconds(20f);

            if (playerData.canDoorRepair && currentActiveRoom.door != null && currentActiveRoom.door.GetHp() < currentActiveRoom.door.GetMaxHp() * 0.75f)
            {
                playerData.SelfRepairDoor();
            }
        }
    }

    public IEnumerator CheckDoorFreeUpgrade()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);

            if (!playerData.freeDoorUpgrade && currentActiveRoom.door != null && currentActiveRoom.door.GetHp() < currentActiveRoom.door.GetMaxHp() * 0.2f)
            {
                if (Random.Range(0, 100) < 25)
                {
                    currentActiveRoom.door.Upgrade();
                    playerData.freeDoorUpgrade = true;
                }
            }
        }
    }
}
