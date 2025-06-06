using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;

using static Define;


public class CameraManager : MonoBehaviour
{

    public enum MapCameraMode
    {
        Player,
        Room
    }

    public static CameraManager instance;
    public Dictionary<GameMode, CinemachineVirtualCamera> cameras = new Dictionary<GameMode, CinemachineVirtualCamera>();

    public MapCameraMode currentMapCameraMode = MapCameraMode.Player;

    private void Awake()
    {
        instance = this;
    }

    public void ChangeCamera(GameMode mode)
    {
        // 현재 게임 모드의 카메라 priority를 낮춤
        if (cameras.TryGetValue(Managers.Game.currentGameMode, out var prevCamera))
        {
            prevCamera.Priority = 0;
        }

        // 새 게임 모드의 카메라 priority를 높임
        if (cameras.TryGetValue(mode, out var newCamera))
        {
            newCamera.Priority = 10;
        }
    }

    public void TurnVinettaEffect(bool isOn)
    {
        Camera.main.GetComponent<PostProcessVolume>().enabled = isOn;
    }

    public void ChangeMapCameraMode(MapCameraMode mode)
    {
        if (currentMapCameraMode == mode)
            return;

        currentMapCameraMode = mode;

        if (mode == MapCameraMode.Player)
        {

        }
        else if (mode == MapCameraMode.Room)
        {
            Managers.Camera.cameras[GameMode.Map].Follow = null;
        }
    }
}
