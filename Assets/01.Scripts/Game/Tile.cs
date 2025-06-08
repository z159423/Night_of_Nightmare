using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public DOTweenAnimation animation;

    public Structure currentStructure;

    void Start()
    {
        animation = GetComponent<DOTweenAnimation>();
    }

    public void OnActive()
    {
        if (animation != null)
        {
            animation.DORestart();
        }
        else
        {
            Debug.LogWarning("DOTweenAnimation component is not assigned to the Tile.");
        }
    }
}
