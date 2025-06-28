using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;


public class PlayerCharactor : PlayerableCharactor
{
    public void Setting()
    {
        charactorType = (Define.CharactorType)Managers.LocalData.SelectedCharactor;

        Managers.Game.playerCharactor = this;

        if (Managers.UI._currentScene is UI_GameScene_Map)
            Managers.UI._currentScene.GetComponent<UI_GameScene_Map>().SetPlayerIcon(charactorType);

        GetComponentInParent<NavMeshAgent>(true).enabled = true;

        Managers.Game.charactors.Add(this);

        playerData = new PlayerData(charactorType);
        Managers.Game.playerData = playerData;

        SetBodySkin();
    }

    public override void SetBodySkin()
    {
        if (bodySpriteRenderer == null)
        {
            bodySpriteRenderer = gameObject.FindRecursive("Icon").GetComponent<SpriteRenderer>();
        }
        bodySpriteRenderer.sprite = Managers.Resource.GetCharactorImage((int)charactorType + 1);
    }

    protected override void Start()
    {
        base.Start();
    }

    private Vector3 lastPosition;

    protected override void Update()
    {
        // NavMeshAgent가 할당되어 있는지 확인
        if (agent != null)
        {
            Vector3 velocity;

            // agent.Move를 사용할 때는 velocity가 자동으로 갱신되지 않으므로 직접 계산
            velocity = (transform.position - lastPosition) / Time.deltaTime;
            lastPosition = transform.position;

            // 직접 계산한 velocity가 0이 아니면 방향을 계산
            if (velocity.sqrMagnitude > 0.01f)
            {
                float dir = velocity.x;
                icon.transform.localRotation = Quaternion.Euler(0, dir >= 0 ? 0 : 180, 0);
            }
        }
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

                playerData.room = currentActiveRoom;

                playerData.structures.Add(currentActiveRoom.bed);
                currentActiveRoom.bed.playerData = playerData;
                playerData.structures.Add(currentActiveRoom.door);
                currentActiveRoom.door.playerData = playerData;
            }
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
        Managers.Game.GameOver();
    }
}
