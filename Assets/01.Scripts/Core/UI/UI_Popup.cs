using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class UI_Popup : UI_Base
{
    protected CanvasGroup _canvasGroup;
    protected bool _isSetting = false;

    public List<UIAnimationTarget> uIAnimationTargets = new List<UIAnimationTarget>();

    public override void Init()
    {
        if (_isSetting)
        {
        }
        else
        {
            _canvasGroup = gameObject.GetComponent<CanvasGroup>();
            FirstSetting();
        }
    }

    public virtual void FirstSetting()
    {
        _isSetting = true;
    }

    public virtual void Reset()
    {

    }

    protected void SetCanvasOrder(int order)
    {
        var canvas = GetComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = order;
    }

    // CloseAllPopup을 했을 때 호출됨
    public virtual void ClosePopupUI()
    {
        Reset();
        Managers.Resource.Destroy(gameObject);
    }

    protected bool _isTransition = false;

    private Transform _tr;
    protected void OpenPop(Transform tr, Action action = null, bool isForced = false)
    {
        if (!isForced && _isTransition) return;

        _canvasGroup.alpha = 0;
        _canvasGroup.DOFade(1, 0.3f).SetUpdate(true);

        _isTransition = true;
        _tr = tr;
        tr.localScale = Vector3.zero;
        tr.DOScale(1, 0.3f).SetEase(Ease.OutBack).SetUpdate(true).onComplete = () =>
        {
            _isTransition = false;
            action?.Invoke();
        };
    }

    public void OpenAnimation(Action OnComplete = null)
    {
        uIAnimationTargets = GetComponentsInChildren<UIAnimationTarget>(true).ToList();

        StartCoroutine(Animation());

        IEnumerator Animation()
        {
            foreach (var target in uIAnimationTargets)
            {
                target.ScaleZero();
            }

            foreach (var target in uIAnimationTargets.Where(n => n.phase == UIAnimationTargetPhase.Phase1))
            {
                target.DoAnimation();
            }

            yield return new WaitForSeconds(0.3f);

            foreach (var target in uIAnimationTargets.Where(n => n.phase == UIAnimationTargetPhase.Phase2))
            {
                target.DoAnimation();
            }

            yield return new WaitForSeconds(0.3f);

            foreach (var target in uIAnimationTargets.Where(n => n.phase == UIAnimationTargetPhase.Phase3))
            {
                target.DoAnimation();
            }

            yield return new WaitForSeconds(0.3f);

            OnComplete?.Invoke();
        }

    }

    protected void ClosePop(Transform tr, Action action = null, bool isForced = false, bool fade = true)
    {
        if (!isForced && _isTransition) return;

        if (fade)
            _canvasGroup.DOFade(0, 0.3f);

        _isTransition = true;
        _tr = tr;
        tr.DOScale(0, 0.3f).SetEase(Ease.InBack).SetUpdate(true).onComplete = () =>
        {
            _isTransition = false;
            ClosePopupUI();
            action?.Invoke();
        };
    }
}
