using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public enum UIAnimationTargetPhase
{
    Phase1,
    Phase2,
    Phase3,
}

public enum UIAnimationType
{
    Pop,
    Appear
}
public class UIAnimationTarget : MonoBehaviour
{
    public UIAnimationTargetPhase phase = UIAnimationTargetPhase.Phase1;
    public UIAnimationType animationType = UIAnimationType.Pop;

    private Vector3 originalScale;

    public bool staticSize = false;

    public void ScaleZero()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    public void DoAnimation()
    {
        switch (animationType)
        {
            case UIAnimationType.Pop:

                if (staticSize)
                    transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                else
                    transform.DOScale(originalScale, 0.3f).SetEase(Ease.OutBack);
                break;
            case UIAnimationType.Appear:
                if (staticSize)
                    transform.localScale = Vector3.one;
                else
                    transform.localScale = originalScale;
                break;
        }
    }
}
