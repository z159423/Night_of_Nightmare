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

    public int Hp { get; private set; } = 100;
    [SerializeField] CloseType closeType;

    public bool isClose = false;

    void Start()
    {
        upgradeIcon.SetActive(false);

        switch (closeType)
        {
            case CloseType.RightToLeft:
                transform.localPosition += new Vector3(0.8f, 0, 0);
                break;
            case CloseType.DownToUp:
                transform.localPosition += new Vector3(0, -0.8f, 0);
                break;
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
    }

    public void Hit(int damage)
    {
        Hp -= damage;
        if (Hp <= 0)
        {
            DestroyDoor();
        }
    }

    public void DestroyDoor()
    {

    }
}
