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

        MaxHp = 100;
        Hp = MaxHp;
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

        var data = Managers.Resource.GetStructureData(type);

        MaxHp = (int)data.argment1[level];
        Hp = MaxHp;

        GetComponent<SpriteRenderer>().sprite = data.sprite1[level];
    }

    public void RepaireDoor()
    {

    }

    public void DestroyDoor()
    {

    }
}
