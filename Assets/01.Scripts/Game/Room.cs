using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] public Room parentRoom;

    [SerializeField] public Tile[] tiles;
    [SerializeField] public Bed bed;
    [SerializeField] public Door door;

    void Start()
    {
        if (tiles.Length == 0)
            tiles = gameObject.GetComponentsInChildren<Tile>(true);
        if (bed == null)
            bed = gameObject.GetComponentInChildren<Bed>();
        if (door == null)
        {
            door = gameObject.GetComponentInChildren<Door>();
        }

        foreach (Tile tile in tiles)
        {
            tile.gameObject.SetActive(false);
        }
    }

    public void OnActive(bool isPlayer)
    {
        door.CloseDoor();

        foreach (Tile tile in tiles)
        {
            tile.gameObject.SetActive(true);
            tile.OnActive(isPlayer);
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
        float minPathDist = GetPathDistance(position, closest.transform.position);

        for (int i = 1; i < findStructures.Count; i++)
        {
            float pathDist = GetPathDistance(position, findStructures[i].transform.position);
            if (pathDist < minPathDist)
            {
                minPathDist = pathDist;
                closest = findStructures[i];
            }
        }

        return closest;
    }

    // NavMesh를 이용해 두 지점 사이의 경로 길이를 구하는 함수
    float GetPathDistance(Vector3 start, Vector3 end)
    {
        UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
        if (UnityEngine.AI.NavMesh.CalculatePath(start, end, UnityEngine.AI.NavMesh.AllAreas, path))
        {
            float distance = 0f;
            if (path.corners.Length < 2)
                return float.MaxValue;

            for (int i = 1; i < path.corners.Length; i++)
            {
                distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
            return distance;
        }
        return float.MaxValue;
    }
}
