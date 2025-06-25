using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public DOTweenAnimation animation;

    public Structure currentStructure;

    public bool playerTile = false;

    void Start()
    {
        animation = GetComponent<DOTweenAnimation>();
    }

    public void OnActive(bool isPlayer)
    {
        playerTile = isPlayer;

        if (!isPlayer)
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }

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
