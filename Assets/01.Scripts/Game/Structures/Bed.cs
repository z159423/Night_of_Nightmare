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

    public PlayerableCharactor currentCharactor;

    public bool active = false;
    bool playerActive = false;

    float delay = 0;

    protected override void Start()
    {
        base.Start();

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

        bed.enabled = false;
        player.enabled = false;

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

        delay = charactor is PlayerCharactor ? 0 : UnityEngine.Random.Range(0, 1f);

        return gameObject.GetComponentInParent<Room>();
    }

    public override void Upgrade()
    {
        base.Upgrade();

        blanket.sprite = Managers.Game.GetStructureData(Define.StructureType.Bed).sprite1[level];
        bed.sprite = Managers.Game.GetStructureData(Define.StructureType.Bed).sprite2[level];
    }

    public override void DestroyStructure()
    {
        currentCharactor.Die();

        base.DestroyStructure();
    }

    public void ResourceGetParticle(int value)
    {
        var particle = Managers.Resource.Instantiate("ResourceGetParticle", transform);
        particle.transform.localPosition = Vector3.zero;
        particle.GetComponent<ResourceGetParticle>().Setting(
            "coin",
            value,
            delay
        );
    }
}
