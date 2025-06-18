using System.Collections;
using System.Collections.Generic;
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

    private Transform repair;


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

        MaxHp = (int)Managers.Game.GetStructureData(Define.StructureType.Door).argment1[level];
        Hp = MaxHp;

        repair = gameObject.FindRecursive("Repair").transform;
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
                    RepaireDoor(0.02f);
                }
            }
        }
    }

    public void CloseDoor()
    {
        isClose = true;

        switch (closeType)
        {
            case CloseType.RightToLeft:
                transform.DOLocalMoveX(transform.localPosition.x - 0.8f, 1);
                break;
            case CloseType.DownToUp:
                transform.DOLocalMoveY(transform.localPosition.y + 0.8f, 1);
                break;
        }

        ShowHpBar();
    }

    public override void Hit(int damage)
    {
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

        MaxHp = (int)Managers.Game.GetStructureData(Define.StructureType.Door).argment1[level];
        Hp = MaxHp;

        GetComponent<SpriteRenderer>().sprite = Managers.Game.GetStructureData(Define.StructureType.Door).sprite1[level];

        ShowHpBar();
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

    }

    private IEnumerator DisableRepairAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        repair.gameObject.SetActive(false);
    }
}
