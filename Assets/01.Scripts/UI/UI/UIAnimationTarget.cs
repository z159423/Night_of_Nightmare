using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
                transform.DOScale(originalScale, 0.3f).SetEase(Ease.OutQuad);
                break;
            case UIAnimationType.Appear:
                transform.localScale = originalScale;
                break;
        }
    }
}
