using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Audio;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    public Sound[] Bgm;
    public GameObject bgmPrefab;
    private AudioSource BGMaudioSource;
    private string previousBgmName = "";
    private float previousBgmTime = 0;

    private string nowBgmName = "";

    private bool isFadeOut = false;
    [SerializeField] private bool oneAudioSource = false;

    [Space]
    public Sound[] SFX;
    public GameObject SFXPrefab;
    private AudioSource SFXaudioSource;


    private static bool Initialized = false;

    // Start is called before the first frame update
    void Awake()
    {
        if (!Initialized)
        {
            Initialized = true;
        }

        BGMaudioSource = bgmPrefab.AddComponent<AudioSource>();
        SFXaudioSource = SFXPrefab.AddComponent<AudioSource>();

        /*foreach (Sound s in Bgm)
        {
            if(!s.bgm)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;

                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
                s.source.outputAudioMixerGroup = s.audioMixerGroup;
            }
            
        }

        foreach (Sound s in SFX)
        {
            if (!s.bgm)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;

                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
                s.source.outputAudioMixerGroup = s.audioMixerGroup;
            }

        }*/
    }

    public void FindBGM(string name = "", bool isPreviousBgm = false)
    {
        Sound s;

        if (isPreviousBgm)
        {
            s = Array.Find(Bgm, sound => sound.name == previousBgmName);
        }
        else
        {
            s = Array.Find(Bgm, sound => sound.name == name);
        }

        PlayBGM(s, isPreviousBgm);
    }

    public void PlayPreviousBGM()
    {
        Sound s = Array.Find(Bgm, sound => sound.name == previousBgmName);

        PlayBGM(s, isPrevious: true);
    }


    public void PlayRandomBossFightBGM()
    {
        string name = "boss" + UnityEngine.Random.Range(2, 4).ToString();

        Sound s = Array.Find(Bgm, sound => sound.name == name);

        PlayBGM(s);
    }

    public void PlaySFX(string name)
    {
        Sound sound = SFX.Where(n => n.name == name).FirstOrDefault();

        SFXaudioSource.clip = sound.clip;

        SFXaudioSource.volume = sound.volume;
        SFXaudioSource.pitch = sound.pitch;
        SFXaudioSource.loop = sound.loop;
        SFXaudioSource.outputAudioMixerGroup = sound.audioMixerGroup;
        SFXaudioSource.time = sound.time;

        SFXaudioSource.Play();
    }


    public void PlaySFXWithAudioSource(string name, AudioSource audio)
    {
        Sound s = Array.Find(SFX, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError("SFX Ŭ���� ã�� �� �����ϴ�.");
            return;
        }

        if (audio == null)
        {
            Debug.LogError("Audio Source�� �����ϴ�.");
            return;
        }

        audio.clip = s.clip;

        audio.volume = s.volume;
        audio.pitch = s.pitch;
        audio.loop = s.loop;
        audio.outputAudioMixerGroup = s.audioMixerGroup;
        audio.time = s.time;

        audio.Play();
    }

    public AudioSource GenerateAudioAndPlaySFX(string name, Vector3 position, float pitch = 1f, bool _override = false)
    {
        Sound s = Array.Find(SFX, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError("SFX Ŭ���� ã�� �� �����ϴ�.");
            return null;
        }

        Poolable audio = null;
        AudioSource audioSource = null;

        if (s.useOneAudioSource)
        {
            if (s.useableAudioSource == null)
            {
                audio = Managers.Pool.Pop(SFXPrefab);
                audioSource = audio.GetComponent<AudioSource>();
                s.useableAudioSource = audio.GetComponent<AudioSource>();
            }
            else
            {
                s.useableAudioSource.transform.position = position;
                audioSource = s.useableAudioSource;
            }
        }
        else if (s.limitPlayCount)
        {
            if (s.playList.Count < 10)
            {
                audio = Managers.Pool.Pop(SFXPrefab);
                audioSource = audio.GetComponent<AudioSource>();

                s.playList.Add(audio.gameObject);
            }
            else
            {
                audio = s.playList[0].GetComponent<Poolable>();
                s.playList[0].transform.position = position;
                audioSource = s.playList[0].GetComponent<AudioSource>();
            }
        }
        else
        {
            audio = Managers.Pool.Pop(SFXPrefab);
            audioSource = audio.GetComponent<AudioSource>();
        }

        //audio.AddComponent<AudioSource>();

        audioSource.clip = s.clip;

        audioSource.volume = s.volume;
        audioSource.pitch = pitch;
        audioSource.loop = s.loop;
        audioSource.outputAudioMixerGroup = s.audioMixerGroup;
        audioSource.time = s.time;

        if (_override)
            audioSource.PlayOneShot(audioSource.clip);
        else
            audioSource.Play();


        if (!s.useOneAudioSource)
            StartCoroutine(enqueue());

        IEnumerator enqueue()
        {
            yield return Util.GetWait(s.clip.length + 1);

            if (s.limitPlayCount && s.playList.Contains(audio.gameObject))
                s.playList.Remove(audio.gameObject);

            Managers.Pool.Push(audio);
        }

        //Destroy(audio, s.clip.length + 1);

        return audioSource;
    }

    private void PlayBGM(Sound s, bool isPrevious = false)
    {
        if (BGMaudioSource.isPlaying)
        {
            if (s.name == nowBgmName)                   //���� bgm�̸� ����
                return;

            float previousBgmTimeSave = previousBgmTime;

            previousBgmName = nowBgmName;

            if (!isFadeOut)
                previousBgmTime = BGMaudioSource.time;

            nowBgmName = s.name;

            StopAllCoroutines();
            StartCoroutine(FadeOutBgm(s, isPrevious, previousBgmTime: previousBgmTimeSave));
        }
        else
        {
            nowBgmName = s.name;

            StartCoroutine(FadeInBgm(s, isPrevious));
        }

    }


    private IEnumerator FadeInBgm(Sound s, bool isPreviousBgm = false, float previousBgmTime = 0)
    {
        float timeToFade = 1.25f;
        float timeElapsed = 0f;
        float maxVolume = s.volume;

        BGMaudioSource.clip = s.clip;
        BGMaudioSource.pitch = s.pitch;
        BGMaudioSource.loop = s.loop;
        BGMaudioSource.outputAudioMixerGroup = s.audioMixerGroup;

        if (isPreviousBgm)
        {
            //Debug.LogError(previousBgmTime);

            BGMaudioSource.time = previousBgmTime;
        }
        else
        {
            BGMaudioSource.time = s.time;
        }

        BGMaudioSource.Play();

        while (timeElapsed < timeToFade)
        {
            BGMaudioSource.volume = Mathf.Lerp(0, maxVolume, timeElapsed / timeToFade);
            timeElapsed += Time.deltaTime / 3;
            yield return null;
        }

    }

    private IEnumerator FadeOutBgm(Sound s, bool isPreviousBgm = false, float previousBgmTime = 0)
    {
        float timeToFade = 0.02f;

        isFadeOut = true;
        while (BGMaudioSource.volume > 0.05f)
        {
            //Debug.LogError("FadeOutBgm");
            BGMaudioSource.volume -= timeToFade;
            yield return Util.GetWait(0.1f);
        }
        isFadeOut = false;


        StartCoroutine(FadeInBgm(s, isPreviousBgm, previousBgmTime: previousBgmTime));

    }

    public void ReplayBG()
    {
        BGMaudioSource.Play();
    }



}

[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;

    public float time = 0;

    public bool loop;
    public bool bgm;

    public AudioMixerGroup audioMixerGroup;

    [HideInInspector]
    public AudioSource source;

    [Space]
    public bool useOneAudioSource = false;
    public bool limitPlayCount = false;
    public List<GameObject> playList = new List<GameObject>();
    [HideInInspector]
    public AudioSource useableAudioSource;

}
