using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine.AI;


public class PlayerCharactor : PlayerableCharactor
{
    public void Setting()
    {
        Managers.Game.playerCharactor = this;

        if (Managers.UI._currentScene is UI_GameScene_Map)
            Managers.UI._currentScene.GetComponent<UI_GameScene_Map>().SetPlayerIcon(charactorType);

        StartCoroutine(GetGold());

        GetComponentInParent<NavMeshAgent>(true).enabled = true;

        Managers.Game.charactors.Add(this);
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Bed>(out Bed bed))
        {
            if (!bed.active)
            {
                currentActiveRoom = bed.OnActive(this);
                gameObject.SetActive(false);
                GameObserver.Call(GameObserverType.Game.OnActivePlayerBed);
            }
        }
    }

    public override void Hit(int damage)
    {
        // AI 캐릭터는 Hit 메서드를 구현하지 않음
        // 필요시 AI 캐릭터의 행동을 정의할 수 있음
    }

    public override void Die()
    {
        Managers.Game.GameOver();
    }
}
