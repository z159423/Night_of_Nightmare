using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : MonoBehaviour
{
    [SerializeField] SpriteRenderer bed;
    [SerializeField] SpriteRenderer player;
    [SerializeField] SpriteRenderer blanket;
    GameObject upgradeIcon;
    bool active = false;
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
        }

        if (blanket == null)
        {
            blanket = gameObject.FindRecursive("Blanket").GetComponent<SpriteRenderer>();
        }

        upgradeIcon = gameObject.FindRecursive("UpgradeIcon");

        bed.enabled = false;
        player.enabled = false;
        upgradeIcon.SetActive(false);
    }

    void OnValidate()
    {

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
    }
}
