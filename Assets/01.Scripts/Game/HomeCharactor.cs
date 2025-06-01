using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HomeCharactor : MonoBehaviour
{


    Transform charactor;

    DOTweenAnimation dOTweenAnimation;

    // Start is called before the first frame updat
    void Start()
    {
        dOTweenAnimation = GetComponent<DOTweenAnimation>();
    }

    public void OnSelect()
    {
        if (dOTweenAnimation != null)
        {
            dOTweenAnimation.DORestart();
        }
        else
        {
            Debug.LogWarning("DOTweenAnimation component is not attached to the GameObject.");
        }
    }

    public void OnUnSelect()
    {
        if (dOTweenAnimation != null)
        {
            dOTweenAnimation.DOPlayBackwards();
        }
        else
        {
            Debug.LogWarning("DOTweenAnimation component is not attached to the GameObject.");
        }
    }
}
