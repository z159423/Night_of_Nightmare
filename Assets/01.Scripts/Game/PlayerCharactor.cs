using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class PlayerCharactor : Charactor
{
    private Transform body;
    private Tween _moveTween;

    void Start()
    {
        body = gameObject.FindRecursive("Body").transform;
        Managers.Game.playerCharactor = this;
    }

    public void OnMove()
    {
        if (_moveTween != null && _moveTween.IsActive())
            return;

        _moveTween = body.DOLocalRotate(new Vector3(0, 0, 3), 0.15f)
            .SetLoops(-1, LoopType.Yoyo)
            .From(new Vector3(0, 0, -3));
    }

    public void OnMoveStop()
    {
        if (_moveTween != null && _moveTween.IsActive())
        {
            _moveTween.Kill();
            _moveTween = null;
        }
        body.DOLocalRotate(Vector3.zero, 0.1f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Bed>(out Bed bed))
        {
            if (!bed.active)
            {
                bed.OnActive(charactorType, true);
                gameObject.SetActive(false);
                GameObserver.Call(GameObserverType.Game.OnActivePlayerBed);
            }
        }
    }
}
