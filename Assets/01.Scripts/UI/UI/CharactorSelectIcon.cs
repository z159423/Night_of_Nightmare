using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using DG.Tweening;

public class CharactorSelectIcon : MonoBehaviour
{
    public CharactorType type;
    Image icon;

    public void Setting(CharactorType type, Action onClick)
    {
        icon = gameObject.FindRecursive("Icon").GetComponent<Image>();

        this.type = type;

        GetComponent<Button>().AddButtonEvent(() =>
        {
            onClick?.Invoke();
        });

        icon.sprite = Managers.Resource.GetCharactorIcons((int)type + 1);
    }

    public void OnSelect()
    {
        icon.transform.DOKill();
        icon.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack);
    }

    public void UnSelect()
    {
        // 선택 해제 시 원래 크기로 복귀
        if (icon != null)
        {
            icon.transform.DOKill();
            icon.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
        }
    }
}
