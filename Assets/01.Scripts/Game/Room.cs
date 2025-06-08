using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private Tile[] tiles;
    [SerializeField] private Bed bed;
    [SerializeField] private Door door;

    void Start()
    {
        if (tiles.Length == 0)
            tiles = gameObject.GetComponentsInChildren<Tile>(true);
        if (bed == null)
            bed = gameObject.GetComponentInChildren<Bed>();
        if (door == null)
            door = gameObject.GetComponentInChildren<Door>();

        foreach (Tile tile in tiles)
        {
            tile.gameObject.SetActive(false);
        }
    }

    public void OnActive(bool isPlayer)
    {
        door.CloseDoor();

        if (!isPlayer)
            return;

        foreach (Tile tile in tiles)
        {
            tile.gameObject.SetActive(true);
            tile.OnActive();
        }
    }

    public Structure GetAttackableStructure(Vector3 position)
    {
        if (door != null && !door.destroyed)
            return door;

        List<Structure> findStructures = new List<Structure>();

        foreach (Tile tile in tiles)
        {
            if (tile.currentStructure != null && !tile.currentStructure.destroyed)
            {
                findStructures.Add(tile.currentStructure);
            }
        }

        if (bed != null && !bed.destroyed)
            findStructures.Add(bed);

        if (findStructures.Count == 0)
            return null;

        Structure closest = findStructures[0];
        float minDist = Vector3.Distance(position, closest.transform.position);

        for (int i = 1; i < findStructures.Count; i++)
        {
            float dist = Vector3.Distance(position, findStructures[i].transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = findStructures[i];
            }
        }

        return closest;
    }
}
