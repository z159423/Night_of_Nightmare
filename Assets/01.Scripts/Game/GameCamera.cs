using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public Define.GameMode cameraMode;

    void Start()
    {
        Managers.Camera.cameras.Add(cameraMode, GetComponent<Cinemachine.CinemachineVirtualCamera>());
    }
}
