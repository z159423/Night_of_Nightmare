using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public Transform charactorSpawnPoint;

    public List<HealZone> healZones = new List<HealZone>();

    public List<RandomBeacon> randomBeacons = new List<RandomBeacon>();

    public void Setting()
    {
        charactorSpawnPoint = gameObject.FindRecursive("CharactorSpawnPoint").transform;
    }
}
