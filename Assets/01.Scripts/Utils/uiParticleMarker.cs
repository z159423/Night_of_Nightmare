using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class uiParticleMarker : MonoBehaviour
{
    public uiParticleMarkerType markerType;

    void Start()
    {
        Managers.UI.uiParticleMarkers.Add(markerType, transform);
    }
}
