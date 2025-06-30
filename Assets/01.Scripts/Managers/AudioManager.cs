using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Audio;
using System.Linq;

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

    public void PlaySound(string soundKey, Transform sourceTransform = null)
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
            PlaySFXInternal(clip, data, sourceTransform);
        }
    }

    private void PlayBGMInternal(AudioClip clip, SoundData data)
    {
        bgmSource.clip = clip;
        bgmSource.volume = data.baseVolume;
        bgmSource.pitch = data.pitch;
        bgmSource.Play();
    }

    private void PlaySFXInternal(AudioClip clip, SoundData data, Transform sourceTransform)
    {
        if (sourceTransform == null)
        {
            sourceTransform = Camera.main.transform; // fallback
        }

        float finalVolume = data.baseVolume;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (!data.IsGlobal())
        {
            Vector2 screenCenter = mainCamera.ViewportToWorldPoint(new Vector2(0.5f, 0.5f));
            float distance = Vector2.Distance(screenCenter, sourceTransform.position);
            float volumeRatio = Mathf.Clamp01(data.maxVolumeRange / distance);

            if (volumeRatio < data.minRangeVolumeMul)
                return;

            finalVolume *= volumeRatio;
        }

        AudioSource.PlayClipAtPoint(clip, sourceTransform.position, finalVolume);
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