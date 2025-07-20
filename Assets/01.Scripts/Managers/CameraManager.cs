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

    public CinemachineVirtualCamera GetCurrentCamera()
    {
        if (cameras.TryGetValue(Managers.Game.currentGameMode, out var camera))
        {
            return camera;
        }
        return null;
    }

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

    public void FollowTargetSmoothly(Transform target, float speed = 5f)
    {
        if (Managers.Game.currentGameMode == GameMode.Map)
        {
            if (cameras.TryGetValue(Managers.Game.currentGameMode, out var camera))
            {
                Vector3 camPos = camera.transform.position;
                Vector3 targetPos = target.position;
                Vector3 newPos = Vector3.Lerp(camPos, new Vector3(targetPos.x, targetPos.y, camPos.z), Time.deltaTime * speed);
                camera.transform.position = newPos;
            }
        }
    }

    public void TurnVinettaEffect(bool isOn)
    {
        Camera.main.GetComponent<PostProcessVolume>().enabled = isOn;
    }

    public void ChangeVinettaIntensity(float intensity)
    {
        if (Camera.main.TryGetComponent<PostProcessVolume>(out var volume))
        {
            if (volume.profile.TryGetSettings<Vignette>(out var vignette))
            {
                vignette.intensity.Override(intensity);
            }
        }
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

    public void ChangeCameraLensOrthoSizeAndPosition(float size, Vector3 targetPosition, float duration = 1.5f)
    {
        if (cameras.TryGetValue(Managers.Game.currentGameMode, out var camera))
        {
            StartCoroutine(LerpOrthoSizeAndPosition(camera, size, targetPosition, duration));
        }
    }

    private IEnumerator LerpOrthoSizeAndPosition(CinemachineVirtualCamera camera, float targetSize, Vector3 targetPosition, float duration)
    {
        float startSize = camera.m_Lens.OrthographicSize;
        Vector3 startPos = camera.transform.position;
        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;
            camera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, t);
            camera.transform.position = Vector3.Lerp(startPos, targetPosition, t);
            time += Time.deltaTime;
            yield return null;
        }
        camera.m_Lens.OrthographicSize = targetSize;
        camera.transform.position = targetPosition;
    }

    private Coroutine followCoroutine;

    public void StartFollowTarget(Transform target, float speed = 25f)
    {
        if (followCoroutine != null)
            StopCoroutine(followCoroutine);
        followCoroutine = StartCoroutine(FollowTargetCoroutine(target, speed));
    }

    private IEnumerator FollowTargetCoroutine(Transform target, float speed)
    {
        const float stopDistance = 0.5f; // 도달했다고 판단할 거리 오차

        while (target != null && Managers.Game.currentGameMode == GameMode.Map)
        {
            if (cameras.TryGetValue(Managers.Game.currentGameMode, out var camera))
            {
                Vector3 camPos = camera.transform.position;
                Vector3 targetPos = target.position;
                Vector3 desiredPos = new Vector3(targetPos.x, targetPos.y, camPos.z);

                // 리니어(등속) 이동
                Vector3 direction = (desiredPos - camPos).normalized;
                float distance = Vector3.Distance(camPos, desiredPos);
                float moveStep = speed * Time.deltaTime;

                if (distance <= stopDistance || moveStep >= distance)
                {
                    camera.transform.position = desiredPos;
                    break;
                }
                else
                {
                    camera.transform.position = camPos + direction * moveStep;
                }
            }
            yield return null;
        }
        followCoroutine = null;
    }

    public void OnTouched()
    {
        if (Managers.Game.currentGameMode == GameMode.Map && followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
            followCoroutine = null;
        }
    }
}
