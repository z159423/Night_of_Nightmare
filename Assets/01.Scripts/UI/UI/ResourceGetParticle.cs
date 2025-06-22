using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using VInspector.Libs;

public class ResourceGetParticle : MonoBehaviour
{
    private SpriteRenderer icon;
    private TextMeshPro text;

    [SerializeField] Sprite[] icons;

    public void Setting(string icon, int value, float delay, float moveY = 1.125f, float moveX = 0, float duration = 0.8f)
    {
        this.icon = GetComponentInChildren<SpriteRenderer>();
        this.text = GetComponentInChildren<TextMeshPro>();

        // 처음에는 비활성화
        this.icon.gameObject.SetActive(false);
        this.text.gameObject.SetActive(false);

        this.icon.sprite = icon == "coin" ? icons[0] : icons[1];
        this.text.text = value.ToString();

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(delay);
        seq.AppendCallback(() =>
        {
            // delay 후 활성화
            this.icon.gameObject.SetActive(true);
            this.text.gameObject.SetActive(true);
        });
        seq.Append(transform.DOLocalMove(new Vector3(moveX, moveY, 0), duration).SetEase(Ease.Linear).SetRelative());
        seq.AppendInterval(0.2f);
        seq.OnComplete(() =>
        {
            Managers.Resource.Destroy(gameObject);
        });
    }
}
