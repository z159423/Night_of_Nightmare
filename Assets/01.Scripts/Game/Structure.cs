using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class Structure : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer spriteRenderer;
    protected GameObject upgradeIcon;
    protected Transform hpBar;
    protected Transform hpBarBody;
    protected Transform hpBarFill;

    public bool playerStructure = false;

    protected int MaxHp = 1;
    protected int Hp = 1;
    public bool destroyed = false;


    protected virtual void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        upgradeIcon = gameObject.FindRecursive("UpgradeIcon");

        if (gameObject.FindRecursive("HpBar") != null)
            hpBar = gameObject.FindRecursive("HpBar").transform;

            if (gameObject.FindRecursive("HpBarBody") != null)
            hpBarBody = gameObject.FindRecursive("HpBarBody").transform;

        if (gameObject.FindRecursive("Fill") != null)
            hpBarFill = gameObject.FindRecursive("Fill").transform;
    }

    public abstract void Upgrade();

    public virtual void Hit(int damage)
    {
        Hp -= damage;
        if (Hp <= 0)
        {
            DestroyStructure();
        }

        // DOTween을 사용하여 펀치 효과 애니메이션 추가
        if (spriteRenderer != null)
        {
            // 이미 트윈이 있다면 중복 방지
            spriteRenderer.transform.DOKill();
            // 상하좌우로 랜덤 방향으로 펀치 효과
            Vector2 randomDir = Random.insideUnitCircle.normalized * 0.1f;
            spriteRenderer.transform.DOPunchPosition((Vector3)randomDir, 0.2f, 10, 1);
        }
    }

    protected virtual void DestroyStructure()
    {
        // Implement destruction logic here, e.g., play animation, destroy object
        gameObject.SetActive(false);

        destroyed = true;
    }
}
