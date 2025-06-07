using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiCharactor : PlayerableCharactor
{
    public bool die = false;

    public void SettingAiCharactor()
    {
        charactorType = (Define.CharactorType)Random.Range(0, System.Enum.GetValues(typeof(Define.CharactorType)).Length);

        SetBodySkin();

        if (Managers.UI._currentScene is UI_GameScene_Map)
            Managers.UI._currentScene.GetComponent<UI_GameScene_Map>().SetCharactorIcon(charactorType);

        StartCoroutine(GetGold());

        GetComponentInParent<NavMeshAgent>(true).enabled = true;

    }

    public void ActiveAiCharactor()
    {
        Managers.Game.aiCharactors.Add(this);

        StartCoroutine(wait());

        IEnumerator wait()
        {
            yield return new WaitForSeconds(Random.Range(Managers.LocalData.PlayerWinCount > 0 ? 1 : 3, 6));
            FindBed();
        }
    }

    void Update()
    {
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

    protected override void Hit(int damage)
    {
        // AI 캐릭터는 Hit 메서드를 구현하지 않음
        // 필요시 AI 캐릭터의 행동을 정의할 수 있음
    }

    protected override void Die()
    {

    }
}
