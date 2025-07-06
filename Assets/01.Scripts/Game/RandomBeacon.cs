using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomBeacon : MonoBehaviour
{
    private void Start()
    {
        GetComponentInParent<Map>().randomBeacons.Add(this);
    }
}
