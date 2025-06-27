using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CharactorIcon : MonoBehaviour
{
    public AiCharactor charactor;

    public void OnDie()
    {
        var iconObj = gameObject.FindRecursive("Icon");
        var image = iconObj.GetComponent<Image>();

        // AttackedAnimation의 모든 DOTween 트윈 중지
        image.DOKill();
        iconObj.transform.DOKill();

        image.color = new Color32(30, 30, 30, 255);
    }

    public void AttackedAnimation()
    {
        RectTransform iconRect = gameObject.FindRecursive("Icon").transform as RectTransform;
        if (iconRect != null)
        {
            iconRect.DOShakeAnchorPos(0.5f, 10f, 30, 90, false, true);
        }
        gameObject.FindRecursive("Icon").GetComponent<Image>().DOColor(Color.red, 0.5f).OnComplete(() =>
        {
            gameObject.FindRecursive("Icon").GetComponent<Image>().DOColor(Color.white, 0.5f);
        });
    }
}
