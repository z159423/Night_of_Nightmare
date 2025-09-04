using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class Door : Structure
{
    enum CloseType
    {
        RightToLeft,
        DownToUp
    }

    [SerializeField] CloseType closeType;

    public bool isClose = false;

    public bool energyShield = false;

    public Transform repair;
    public Transform energyShieldObj;
    private Transform thornBush;
    private Transform coolerParticle;
    private SpriteRenderer bodySpriteRenderer;


    private float energyShieldDuration = 4f;
    private float energyShieldCooldown = 120f;
    private float lastEnergyShieldTime = -999f;

    protected override void Start()
    {
        base.Start();

        switch (closeType)
        {
            case CloseType.RightToLeft:
                transform.localPosition += new Vector3(0.8f, 0, 0);
                break;
            case CloseType.DownToUp:
                transform.localPosition += new Vector3(0, -0.8f, 0);
                break;
        }

        bodySpriteRenderer = gameObject.FindRecursive("Body").GetComponent<SpriteRenderer>();

        MaxHp = GetDoorMaxHp();
        Hp = MaxHp;

        repair = gameObject.FindRecursive("Repair").transform;
        energyShieldObj = gameObject.FindRecursive("EnergyShield").transform;
        energyShieldObj.gameObject.SetActive(false);

        thornBush = gameObject.FindRecursive("ThornBush").transform;
        coolerParticle = gameObject.FindRecursive("CoolerParticle").transform;
    }

    private void OnEnable()
    {
        StartCoroutine(AutoRepairCheck());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator AutoRepairCheck()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (Hp >= MaxHp)
                continue;

            if (playerData != null && playerData.structures != null)
            {
                bool hasRepairStation = playerData.structures.Exists(s => s.type == Define.StructureType.RepairStation);
                if (hasRepairStation)
                {
                    Managers.Audio.PlaySound("snd_stage_unlock", playerData.structures.Find(n => n.type == Define.StructureType.RepairStation).transform, minRangeVolumeMul: 0.6f, volumeMul: 0.8f);

                    float healPercent = 0.02f;

                    if (Managers.Ability.GetHasAbilityValueSum(AbilityType.RepairStationMul) > 0)
                    {
                        healPercent += 0.01f;
                    }

                    RepaireDoor(healPercent);
                }
            }
        }
    }

    private void ActivateEnergyShield()
    {
        energyShield = true;
        lastEnergyShieldTime = Time.time;
        if (energyShieldObj != null)
            energyShieldObj.gameObject.SetActive(true);

        energyShieldObj.GetComponent<SpriteRenderer>().sprite = bodySpriteRenderer.sprite;
        StartCoroutine(EnergyShieldDurationRoutine());
    }

    private IEnumerator EnergyShieldDurationRoutine()
    {
        yield return new WaitForSeconds(energyShieldDuration);
        energyShield = false;
        if (energyShieldObj != null)
            energyShieldObj.gameObject.SetActive(false);
    }

    public void CloseDoor()
    {
        isClose = true;

        switch (closeType)
        {
            case CloseType.RightToLeft:
                transform.DOLocalMoveX(transform.localPosition.x - 0.8f, 1).OnComplete(() =>
                {
                    bodySpriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
                });
                break;
            case CloseType.DownToUp:
                transform.DOLocalMoveY(transform.localPosition.y + 0.8f, 1).OnComplete(() =>
                {
                    bodySpriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
                });
                break;
        }

        ShowHpBar();
    }

    public override void Hit(float damage)
    {
        // 에너지 쉴드가 활성화 중이면 데미지 무시
        if (energyShield || activeEffects.Any(e => e is HolyProtection))
        {
            ShowHpBar();

            Managers.Audio.PlaySound("snd_autoguard", transform);
            return;
        }

        // 에너지 쉴드 발동 조건 체크
        if (!energyShield && Time.time - lastEnergyShieldTime >= energyShieldCooldown && playerData.structures.Any(n => n.type == Define.StructureType.EnergyShield))
        {
            if ((float)Hp / MaxHp <= 0.3f)
            {
                ActivateEnergyShield();
            }
        }

        if (playerData.structures.Any(n => n.type == Define.StructureType.FlowerPot || n.type == Define.StructureType.LushFlowerPot))
        {
            foreach (var flowerPot in playerData.structures.Where(n => n.type == Define.StructureType.FlowerPot))
            {
                playerData.AddCoin((int)Managers.Game.GetStructureData(Define.StructureType.FlowerPot).argment1[flowerPot.level]);
                flowerPot.GetComponent<FlowerPot>().ResourceGetParticle((int)Managers.Game.GetStructureData(Define.StructureType.FlowerPot).argment1[flowerPot.level]);
            }

            foreach (var flowerPot in playerData.structures.Where(n => n.type == Define.StructureType.LushFlowerPot))
            {
                playerData.AddCoin((int)Managers.Game.GetStructureData(Define.StructureType.LushFlowerPot).argment1[flowerPot.level]);
                flowerPot.GetComponent<LushFlowerPot>().ResourceGetParticle((int)Managers.Game.GetStructureData(Define.StructureType.LushFlowerPot).argment1[flowerPot.level]);
            }
        }

        base.Hit(damage);

        ShowHpBar();
    }

    public void ShowHpBar()
    {
        if (hpBar != null)
        {
            hpBar.gameObject.SetActive(true);
            hpBarBody.localScale = new Vector3((float)Hp / MaxHp, 1, 1);
        }
    }

    public override void Upgrade()
    {
        base.Upgrade();

        SetStat();
        SetBodySprite();
        ShowHpBar();
    }

    public override void UpgradeTo(int levelTo)
    {
        base.UpgradeTo(levelTo);

        SetStat();
        SetBodySprite();
        ShowHpBar();
    }

    public override void SetBodySprite()
    {
        bodySpriteRenderer.sprite = Managers.Game.GetStructureData(Define.StructureType.Door).sprite1[level];
    }

    public void SetStat()
    {
        MaxHp = GetDoorMaxHp();
        Hp = MaxHp;
    }

    public void RepaireDoor()
    {

    }

    public void RepaireDoor(float percent)
    {
        if (percent < 0f) percent = 0f;
        if (percent > 1f) percent = 1f;

        Hp = Mathf.Clamp(Hp + Mathf.RoundToInt(MaxHp * percent), 0, MaxHp);
        ShowHpBar();

        repair.gameObject.SetActive(true);
        StartCoroutine(DisableRepairAfterDelay());

        Managers.Audio.PlaySound("snd_stage_unlock", transform, minRangeVolumeMul: 0.6f, volumeMul: 0.8f);
    }

    private IEnumerator DisableRepairAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        repair.gameObject.SetActive(false);
    }

    public void ActiveThornBush()
    {
        thornBush.gameObject.SetActive(true);
    }

    public void DeactiveThornBush()
    {
        thornBush.gameObject.SetActive(false);
    }

    public void ActiveCooler()
    {
        bodySpriteRenderer.color = new Color32(61, 67, 255, 255);

        coolerParticle.gameObject.SetActive(true);
    }

    public void DeactiveCooler()
    {
        bodySpriteRenderer.color = Color.white;

        coolerParticle.gameObject.SetActive(false);
    }

    int GetDoorMaxHp()
    {
        var hp = MaxHp;

        if (IsPlayerStructure())
            hp = Mathf.RoundToInt(hp * Managers.Ability.GetHasAbilityValueSum(AbilityType.DoorHp));

        return hp;
    }
}
