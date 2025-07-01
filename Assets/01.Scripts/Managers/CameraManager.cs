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

    public void FocusToTarget(Transform target)
    {
        if (cameras.TryGetValue(Managers.Game.currentGameMode, out var camera))
        {
            Vector3 camPos = camera.transform.position;
            Vector3 targetPos = target.position;
            camera.transform.position = new Vector3(targetPos.x, targetPos.y, camPos.z);
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

    public void ChangeCameraLensOrthoSize(float size, float duration = 1.5f)
    {
        if (cameras.TryGetValue(Managers.Game.currentGameMode, out var camera))
        {
            StartCoroutine(LerpOrthoSize(camera, size, duration));
        }
    }

    private IEnumerator LerpOrthoSize(CinemachineVirtualCamera camera, float targetSize, float duration)
    {
        float startSize = camera.m_Lens.OrthographicSize;
        float time = 0f;
        while (time < duration)
        {
            camera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        camera.m_Lens.OrthographicSize = targetSize;
    }
}
