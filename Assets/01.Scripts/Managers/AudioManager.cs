using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Audio;
using System.Linq;
using System.Threading;

public class AudioManager : MonoBehaviour
{
    private Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

    private AudioSource bgmSource;
    private Camera mainCamera;

    private void Start()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;

        mainCamera = Camera.main;
    }

    public void PlaySound(string soundKey, Transform sourceTransform = null, float minRangeVolumeMul = -2f, float volumeMul = 1f, float pitch = 1f)
    {
        if (!Define.soundDatas.TryGetValue(soundKey, out var data))
        {
            Debug.LogWarning($"[AudioManager] SoundData not found: {soundKey}");
            return;
        }

        var clip = GetClip(soundKey);
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] AudioClip not found: {soundKey}");
            return;
        }

        if (data.IsBGM())
        {
            PlayBGMInternal(clip, data);
        }
        else
        {
            PlaySFXInternal(clip, data, sourceTransform, minRangeVolumeMul, volumeMul, pitch);
        }
    }

    private void PlayBGMInternal(AudioClip clip, SoundData data)
    {
        bgmSource.clip = clip;
        bgmSource.volume = data.baseVolume;
        bgmSource.pitch = data.pitch;
        bgmSource.Play();
    }

    private void PlaySFXInternal(AudioClip clip, SoundData data, Transform sourceTransform, float minRangeVolumeMul = -2f, float volumeMul = 1f, float pitch = 1f)
    {
        if (sourceTransform == null)
            sourceTransform = Camera.main.transform;

        if (mainCamera == null)
            mainCamera = Camera.main;

        float finalVolume = data.baseVolume * volumeMul;

        // minRangeVolumeMul이 -2가 아니면 data.minRangeVolumeMul 대신 매개변수 값 사용
        float minVolumeMul = (minRangeVolumeMul != -2f) ? minRangeVolumeMul : data.minRangeVolumeMul;

        if (sourceTransform != null)
        {
            // minVolumeMul이 -1이면 거리 무시하고 baseVolume으로 재생
            if (minVolumeMul == -1f)
            {
                finalVolume = data.baseVolume;
            }
            else
            {
                // 화면 중심과 오브젝트의 픽셀 거리 계산
                Vector3 screenPos = mainCamera.WorldToScreenPoint(sourceTransform.position);
                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
                float pixelDistance = Vector2.Distance(screenCenter, screenPos);

                // 거리에 따른 사운드 값 계산
                // float soundValue = data.maxVolumeRange / pixelDistance;

                float soundValue = data.maxVolumeRange / pixelDistance * 1.5f;

                // minVolumeMul보다 커야만 재생
                if (soundValue <= minVolumeMul)
                    return;

                // 최종 볼륨 계산 (최대 1)
                // finalVolume = data.baseVolume * Mathf.Clamp01(soundValue);
                finalVolume = data.baseVolume * Mathf.Clamp01(soundValue);

            }
        }

        var source = Managers.Resource.Instantiate("AudioSource").GetComponent<AudioSource>();
        source.transform.position = sourceTransform.position;
        source.clip = clip;
        source.spatialBlend = 1f;
        source.volume = finalVolume;
        source.pitch = pitch == 1 ? data.pitch : pitch;
        source.Play();

        StartCoroutine(destroy());

        IEnumerator destroy()
        {
            yield return new WaitForSeconds(clip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
            Managers.Resource.Destroy(source.gameObject);
        }

        // AudioSource.PlayClipAtPoint(clip, sourceTransform.position, finalVolume);
    }

    private AudioClip GetClip(string key)
    {
        if (clipCache.TryGetValue(key, out var clip)) return clip;

        clip = Resources.Load<AudioClip>($"Sound/{key}");
        if (clip != null)
            clipCache[key] = clip;

        return clip;
    }

    // BGM 제어
    public void StopBGM() => bgmSource.Stop();
    public void PauseBGM() => bgmSource.Pause();
    public void ResumeBGM() => bgmSource.UnPause();
}

[System.Serializable]
public class SoundData
{
    public string soundKey;
    public float baseVolume = 1f;
    public float pitch = 1f;
    public float maxVolumeRange = -1f; // -1이면 거리 무시
    public float minRangeVolumeMul = -1f;

    public bool IsGlobal() => maxVolumeRange < 0f || minRangeVolumeMul < 0f;
    public bool IsBGM() => soundKey.StartsWith("bgm_"); // 이름 규칙으로 분기
}