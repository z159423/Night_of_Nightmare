using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealZone : MonoBehaviour
{
    void Start()
    {
        GetComponentInParent<Map>().healZones.Add(this);
    }
}
