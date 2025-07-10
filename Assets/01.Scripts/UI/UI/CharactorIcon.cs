using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;

public class CharactorIcon : MonoBehaviour
{
    public PlayerableCharactor charactor;

    TextMeshProUGUI coinText;
    TextMeshProUGUI energyText;


    public void Setting(PlayerableCharactor charactor)
    {
        this.charactor = charactor;

        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (this.charactor != null && !this.charactor.die)
            {
                Managers.Camera.StartFollowTarget(this.charactor.transform);
            }
        });

        coinText = gameObject.FindRecursive("CoinText").GetComponent<TextMeshProUGUI>();
        energyText = gameObject.FindRecursive("EnergyText").GetComponent<TextMeshProUGUI>();

        StartCoroutine(ResourceCheck());

        this.SetListener(GameObserverType.Game.OnCheatModeOn, () =>
        {
            coinText.gameObject.SetActive(Managers.LocalData.CheatMode == 1);
            energyText.gameObject.SetActive(Managers.LocalData.CheatMode == 1);
        });

        coinText.gameObject.SetActive(Managers.LocalData.CheatMode == 1);
        energyText.gameObject.SetActive(Managers.LocalData.CheatMode == 1);
    }

    IEnumerator ResourceCheck()
    {
        while (!charactor.die)
        {
            if (charactor != null)
            {
                coinText.text = charactor.playerData.coin.ToString();
                energyText.text = charactor.playerData.energy.ToString();
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void OnDie()
    {
        var iconObj = gameObject.FindRecursive("Icon");
        var image = iconObj.GetComponent<Image>();

        // 모든 Tween 중지 (Image와 Transform 모두)
        DOTween.Kill(image);
        DOTween.Kill(iconObj.transform);

        // 컬러 즉시 적용
        image.color = new Color32(30, 30, 30, 255);
    }

    public void AttackedAnimation()
    {
        if (charactor == null || charactor.die)
            return;

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
