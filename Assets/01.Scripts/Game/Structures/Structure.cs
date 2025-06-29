using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using System.Text;
using VInspector;

public abstract class Structure : MonoBehaviour
{
    public List<StructureEffect> activeEffects = new List<StructureEffect>();

    [SerializeField] protected SpriteRenderer spriteRenderer;
    protected GameObject upgradeIcon;
    protected Transform hpBar;
    protected Transform hpBarBody;
    protected Transform hpBarFill;

    public bool playerStructure = false;

    protected int MaxHp = 1;
    public int GetMaxHp() => MaxHp;
    protected float Hp = 1;
    public float GetHp() => Hp;
    public bool destroyed = false;

    public Define.StructureType type;
    public int level = 0;

    public PlayerData playerData;

    protected float upgradePercent = 2.5f;

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

    protected virtual void Update()
    {
        UpdateEffects();
    }

    public virtual void Upgrade()
    {
        level++;

        if (playerData == Managers.Game.playerData)
            GameObserver.Call(GameObserverType.Game.OnPlayerTutorialActing);
    }

    public virtual void Hit(float damage)
    {
        Hp -= damage;
        if (Hp <= 0)
        {
            DestroyStructure();
            RemoveThisStructrue();
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

    public virtual void Heal(int healAmount)
    {
        Hp += healAmount;
        if (Hp > MaxHp)
            Hp = MaxHp;

        // HP바 업데이트
        if (hpBarBody != null)
        {
            hpBarBody.localScale = new Vector3((float)Hp / MaxHp, 1, 1);
        }
    }

    [Button("Destroy Structure")]
    public virtual void DestroyStructure()
    {
        gameObject.SetActive(false);

        destroyed = true;

        StopAllCoroutines();
    }

    public void CheckUpgrade()
    {
        if (activeEffects.Any(e => e is CreepylaughterEffect))
            return;

        if (Managers.Game.playerCharactor.playerData.structures.Contains(this) == false)
            return;

        if (Managers.Game.GetStructureData(type).CanUpgrade(playerData, level + 1))
            upgradeIcon.gameObject.SetActive(true);
        else
            upgradeIcon.gameObject.SetActive(false);
    }

    protected void RemoveThisStructrue()
    {
        if (playerData.structures.Contains(this))
        {
            playerData.structures.Remove(this);
            if (GetComponentInParent<Tile>() != null)
                GetComponentInParent<Tile>().currentStructure = null;
        }
    }

    public void AddEffect(StructureEffect newEffect)
    {
        // 이미 같은 효과가 있으면 지속시간만 초기화
        var exist = activeEffects.FirstOrDefault(e => e.GetType() == newEffect.GetType());
        if (exist != null)
        {
            exist.Duration = newEffect.Duration;
            exist.elapsedTime = 0f;
            // 기존 효과가 비활성화 상태라면 다시 활성화
            if (!exist.IsActive)
                exist.Apply(this);
        }
        else
        {
            newEffect.Apply(this);
            activeEffects.Add(newEffect);
        }
    }

    // Enemy.Update() 등에서 호출
    private void UpdateEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].Tick(this, Time.deltaTime);
            if (!activeEffects[i].IsActive)
                activeEffects.RemoveAt(i);
        }
    }

    public virtual void CheckPossibleUpgrade()
    {
        StartCoroutine(routine());

        IEnumerator routine()
        {
            var _data = Managers.Game.GetStructureData(type);

            while (true)
            {
                yield return new WaitForSeconds(1f); // 잠시 대기하여 UI 업데이트가 완료되도록 함

                if (upgradePercent > Random.Range(0f, 100f) && _data.CanUpgrade(playerData, level + 1))
                {
#if UNITY_EDITOR
                    Debug.Log($"{playerData.type} upgraded {type} at {transform.position}");
#endif
                    playerData.UseResource(_data.upgradeCoin[level], _data.upgradeEnergy[level]);
                    Upgrade();
                }
            }
        }
    }
}

[System.Serializable]
public abstract class StructureEffect
{
    public string Name;
    public float Duration;
    public float elapsedTime = 0f;

    public bool IsActive { get; private set; }

    public void Apply(Structure structure)
    {
        IsActive = true;
        elapsedTime = 0f;
        OnApply(structure);
    }

    public void Tick(Structure structure, float deltaTime)
    {
        if (!IsActive) return;
        elapsedTime += deltaTime;
        OnTick(structure, deltaTime);

        if (elapsedTime >= Duration)
        {
            Remove(structure);
        }
    }

    public void Remove(Structure structure)
    {
        if (!IsActive) return;
        IsActive = false;
        OnRemove(structure);
    }

    // 효과별 세부 구현
    protected abstract void OnApply(Structure structure);
    protected abstract void OnTick(Structure structure, float deltaTime);
    protected abstract void OnRemove(Structure structure);
}

public class CreepylaughterEffect : StructureEffect
{
    private float tickTimer = 0f;

    private GameObject effectObj;

    public CreepylaughterEffect(float duration)
    {
        Name = "CreepylaughterEffect";
        Duration = duration;
    }

    protected override void OnApply(Structure structure)
    {
        tickTimer = 0f;

        effectObj = Managers.Resource.Instantiate("CreepylaughterEffect", structure.transform);
        effectObj.transform.localPosition = new Vector3(0, 0.1f, 0);
    }

    protected override void OnTick(Structure structure, float deltaTime)
    {
        tickTimer += deltaTime;
    }

    protected override void OnRemove(Structure structure)
    {
        if (effectObj != null)
        {
            Managers.Resource.Destroy(effectObj);
            effectObj = null;
        }
    }
}

public class MothPowderStun : StructureEffect
{
    private float tickTimer = 0f;

    private GameObject effectObj;

    public MothPowderStun(float duration)
    {
        Name = "MothPowderStun";
        Duration = duration;
    }

    protected override void OnApply(Structure structure)
    {
        tickTimer = 0f;

        effectObj = Managers.Resource.Instantiate("MothPowderStunParticle", structure.transform);
        effectObj.transform.localPosition = new Vector3(0, 0.1f, 0);
    }

    protected override void OnTick(Structure structure, float deltaTime)
    {
        tickTimer += deltaTime;
    }

    protected override void OnRemove(Structure structure)
    {
        if (effectObj != null)
        {
            Managers.Resource.Destroy(effectObj);
            effectObj = null;
        }
    }
}

public class SelfDoorRepair : StructureEffect
{
    private float tickTimer = 0f;


    public SelfDoorRepair(float duration)
    {
        Name = "SelfDoorRepair";
        Duration = duration;
    }

    protected override void OnApply(Structure structure)
    {
        if (structure is Door door)
        {
            tickTimer = 0f;

            door.repair.gameObject.SetActive(true);
        }
    }

    protected override void OnTick(Structure structure, float deltaTime)
    {
        if (structure is Door door)
        {
            while (tickTimer >= 1f)
            {
                tickTimer -= 1f;
                door.Heal(Mathf.RoundToInt(door.GetMaxHp() * 0.07f));
            }
        }

        tickTimer += deltaTime;
    }

    protected override void OnRemove(Structure structure)
    {
        if (structure is Door door)
        {
            door.repair.gameObject.SetActive(false);
        }
    }
}

public class OverHeat : StructureEffect
{
    private float tickTimer = 0f;

    public OverHeat(float duration)
    {
        Name = "OverHeat";
        Duration = duration;
    }

    protected override void OnApply(Structure structure)
    {
        if (structure is Turret turret)
        {
            tickTimer = 0f;
        }
    }

    protected override void OnTick(Structure structure, float deltaTime)
    {


        tickTimer += deltaTime;
    }

    protected override void OnRemove(Structure structure)
    {
        if (structure is Turret turret)
        {

        }
    }
}

public class HolyProtection : StructureEffect
{
    private float tickTimer = 0f;

    public HolyProtection(float duration)
    {
        Name = "HolyProtection";
        Duration = duration;
    }

    protected override void OnApply(Structure structure)
    {
        if (structure is Door door)
        {
            tickTimer = 0f;

            door.energyShieldObj.gameObject.SetActive(true);
        }
    }

    protected override void OnTick(Structure structure, float deltaTime)
    {

        tickTimer += deltaTime;
    }

    protected override void OnRemove(Structure structure)
    {
        if (structure is Door door)
        {
            door.energyShieldObj.gameObject.SetActive(false);
        }
    }
}
