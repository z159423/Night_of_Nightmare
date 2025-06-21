using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Notification_Popup : UI_Base
{
    enum Texts
    {
        Text
    }

    enum Images
    {
        Image
    }

    private bool _isInitialized = false;

    public override void Init()
    {
        if (_isInitialized)
            return;

        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        _isInitialized = true;
    }

    public void Setting(string text)
    {
        GetTextMesh(Texts.Text).text = text;

        // DOTween이 필요하므로 using DG.Tweening; 추가 필요
        var image = GetImage(Images.Image);
        var canvasGroup = image.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = image.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        image.rectTransform.anchoredPosition = Vector2.zero;

        // 알파값 0 -> 1 (1초)
        canvasGroup.DOFade(1f, 0.4f).OnComplete(() =>
        {
            // 3초 대기 후
            DOVirtual.DelayedCall(2.5f, () =>
            {
                // y좌표 70만큼 1.5초 동안 이동 + 알파값 0으로
                var seq = DOTween.Sequence();
                seq.Join(image.rectTransform.DOAnchorPosY(100f, 0.6f));
                seq.Join(canvasGroup.DOFade(0f, 0.6f));
                seq.OnComplete(() => Exit());
            });
        });
    }

    private void Exit()
    {
        Managers.Resource.Destroy(gameObject);
    }
}
