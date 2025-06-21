using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using VHierarchy.Libs;

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

    public Define.StructureType type;
    public int level = 0;

    public PlayerData playerData;

    protected virtual void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        upgradeIcon = gameObject.FindRecursive("UpgradeIcon");

        upgradeIcon?.SetActive(false);

        if (gameObject.FindRecursive("HpBar") != null)
            hpBar = gameObject.FindRecursive("HpBar").transform;

        if (gameObject.FindRecursive("HpBarBody") != null)
            hpBarBody = gameObject.FindRecursive("HpBarBody").transform;

        if (gameObject.FindRecursive("Fill") != null)
            hpBarFill = gameObject.FindRecursive("Fill").transform;

        CheckUpgrade();

        this.SetListener(GameObserverType.Game.OnChangeCoinCount, () =>
        {
            CheckUpgrade();
        });

        this.SetListener(GameObserverType.Game.OnChangeEnergyCount, () =>
        {
            CheckUpgrade();
        });

        this.SetListener(GameObserverType.Game.OnChangeStructure, () =>
        {
            CheckUpgrade();
        });
    }

    public virtual void Upgrade()
    {
        level++;
    }

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

    public virtual void DestroyStructure()
    {
        // Implement destruction logic here, e.g., play animation, destroy object
        gameObject.SetActive(false);

        destroyed = true;
    }

    public int GetSellValue()
    {
        int sellValue = Managers.Game.GetStructureData(type).upgradeCoin[level] / 4; // 판매가는 업그레이드 비용의 1/4
        return sellValue;
    }

    public void CheckUpgrade()
    {
        if (Managers.Game.playerCharactor.playerData.structures.Contains(this) == false)
            return;

        var _data = Managers.Game.GetStructureData(type);

        // 업그레이드가 가능한 상태가 아니면
        if (_data.upgradeCoin.Length < 2 && _data.upgradeEnergy.Length < 2)
        {
            upgradeIcon.gameObject.SetActive(false);
        }
        // 업그레이드 가능한 상태라면
        else if (
            CheckIsReqired() &&
            (_data.upgradeCoin.Length > level + 1 ? (Managers.Game.playerData.coin >= _data.upgradeCoin[level + 1]) : false) &&
            (_data.upgradeEnergy.Length > level + 1 ? (Managers.Game.playerData.energy >= _data.upgradeEnergy[level + 1]) : false)
        )
            upgradeIcon.gameObject.SetActive(true);
        else
            upgradeIcon.gameObject.SetActive(false);
    }

    public bool CheckIsReqired()
    {
        var _data = Managers.Game.GetStructureData(type);

        bool isMet = true;
        if (_data.requireStructures != null && _data.requireStructures.Length > 0 && _data.requireStructures.Length >= level + 1
          && _data.requireStructures[level + 1].type != _data.structureType)
        {
            var currentRequire = Managers.Game.playerData.GetStructure(_data.requireStructures[level + 1].type);

            isMet = currentRequire != null && currentRequire.level >= _data.requireStructures[level + 1].level;
        }

        return isMet;
    }
}
