using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Bed : Structure
{
    [SerializeField] SpriteRenderer bed;
    [SerializeField] SpriteRenderer player;
    [SerializeField] DOTweenAnimation playerAnimation;
    [SerializeField] SpriteRenderer blanket;
    [SerializeField] DOTweenAnimation blanketAnimation;

    PlayerableCharactor currentCharactor;

    public bool active = false;
    bool playerActive = false;

    protected override void Start()
    {
        base.Start();
        this.SetListener(GameObserverType.Game.OnChangeCoinCount, () =>
        {
            //만약 업그레이드 가능한 상태라면

        });

        if (bed == null)
        {
            bed = gameObject.FindRecursive("Bed").GetComponent<SpriteRenderer>();
        }

        if (player == null)
        {
            player = gameObject.FindRecursive("Player").GetComponent<SpriteRenderer>();
            playerAnimation = player.GetComponent<DOTweenAnimation>();
        }

        if (blanket == null)
        {
            blanket = gameObject.FindRecursive("Blanket").GetComponent<SpriteRenderer>();
            blanketAnimation = blanket.GetComponent<DOTweenAnimation>();
        }

        upgradeIcon = gameObject.FindRecursive("UpgradeIcon");

        bed.enabled = false;
        player.enabled = false;
        upgradeIcon.SetActive(false);

        Managers.Game.beds.Add(this);
    }

    public Room OnActive(PlayerableCharactor charactor)
    {
        //캐릭터 타입에 따라 스프라이트 바꿔주기
        player.sprite = Managers.Resource.GetCharactorImage((int)charactor.charactorType + 1);

        bool isPlayer = charactor is PlayerCharactor;

        active = true;
        if (isPlayer)
        {
            playerActive = true;
            Managers.Camera.ChangeMapCameraMode(CameraManager.MapCameraMode.Room);
        }
        else
            playerActive = false;

        this.currentCharactor = charactor;

        bed.enabled = true;
        bed.color = Color.white;
        player.enabled = true;
        player.color = Color.white;
        blanket.color = Color.white;

        playerAnimation.DORestart();
        blanketAnimation.DORestart();


        gameObject.GetComponentInParent<Room>().OnActive(isPlayer);

        return gameObject.GetComponentInParent<Room>();
    }

    public override void Upgrade()
    {
        base.Upgrade();

        var data = Managers.Resource.GetStructureData(type);

        blanket.sprite = data.sprite1[level];
        bed.sprite = data.sprite2[level];
    }

    public override void DestroyStructure()
    {
        currentCharactor.Die();

        base.DestroyStructure();
    }
}
