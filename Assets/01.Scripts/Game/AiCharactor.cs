using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiCharactor : PlayerableCharactor
{
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

    public void ActiveAiCharactor()
    {
        Managers.Game.charactors.Add(this);

        StartCoroutine(wait());

        IEnumerator wait()
        {
            yield return new WaitForSeconds(Random.Range(Managers.LocalData.PlayerWinCount > 0 ? 1 : 3, 6));
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
    }

    public override void Die()
    {
        //죽으면 방에 있는 모든 건축물 파괴
        //플레이어 리스트에서 삭제

        die = true;
    }
}
