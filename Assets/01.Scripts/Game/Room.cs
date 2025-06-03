using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private Tile[] tiles;
    [SerializeField] private Bed bed;
    [SerializeField] private Door door;


    void OnValidate()
    {
        if (tiles.Length == 0)
            tiles = gameObject.GetComponentsInChildren<Tile>(true);
        if (bed = null)
            bed = gameObject.GetComponentInChildren<Bed>();
        if (door = null)
            door = gameObject.GetComponentInChildren<Door>();
    }

    void Start()
    {
        foreach (Tile tile in tiles)
        {
            tile.gameObject.SetActive(false);
        }
    }
}
