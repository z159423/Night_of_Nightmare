using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Bed : MonoBehaviour
{
    [SerializeField] SpriteRenderer bed;
    [SerializeField] SpriteRenderer player;
    [SerializeField] DOTweenAnimation playerAnimation;
    [SerializeField] SpriteRenderer blanket;
    [SerializeField] DOTweenAnimation blanketAnimation;

    GameObject upgradeIcon;
    public bool active = false;
    bool playerActive = false;

    void Start()
    {
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

    public void OnActive(Define.CharactorType charactorType, bool isPlayer)
    {
        //캐릭터 타입에 따라 스프라이트 바꿔주기
        player.sprite = Managers.Resource.GetCharactorImage((int)charactorType + 1);

        active = true;
        this.playerActive = isPlayer;

        bed.enabled = true;
        bed.color = Color.white;
        player.enabled = true;
        player.color = Color.white;
        blanket.color = Color.white;

        playerAnimation.DORestart();
        blanketAnimation.DORestart();

        if (isPlayer)
        {
            Managers.Camera.ChangeMapCameraMode(CameraManager.MapCameraMode.Room);
        }

        gameObject.GetComponentInParent<Room>().OnActive(isPlayer);
    }
}
