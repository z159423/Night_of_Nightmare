using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : MonoBehaviour
{
    [SerializeField] SpriteRenderer bed;
    [SerializeField] SpriteRenderer player;
    [SerializeField] SpriteRenderer blanket;
    GameObject upgradeIcon;

    void Start()
    {
        this.SetListener(GameObserverType.Game.OnChangeCoinCount, () =>
        {
            //만약 업그레이드 가능한 상태라면
            
        });
    }

    void OnValidate()
    {
        if (bed == null)
        {
            gameObject.FindRecursive("Bed").GetComponent<SpriteRenderer>();
        }

        if (player == null)
        {
            gameObject.FindRecursive("Player").GetComponent<SpriteRenderer>();
        }

        if (blanket == null)
        {
            gameObject.FindRecursive("Blanket").GetComponent<SpriteRenderer>();
        }

        upgradeIcon = gameObject.FindRecursive("UpgradeIcon");
    }

    public void OnActive(Define.CharactorType charactorType)
    {
        //캐릭터 타입에 따라 스프라이트 바꿔주기

    }
}
