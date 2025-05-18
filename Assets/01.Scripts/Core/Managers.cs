using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Text.RegularExpressions;


public class Managers : SingletonStatic<Managers>
{
    public static Managers instance = null;

    public static ResourceManager Resource { get; set; } = new ResourceManager();
    public static LocalizeManager Localize { get; set; } = new LocalizeManager();
    public static PoolManager Pool { get; set; } = new PoolManager();
    public static AudioManager AudioManager { get; set; } = new AudioManager();
    public static UIManager UI { get; set; } = new UIManager();

    void CreateManagers()
    {
        Localize = transform.AddComponent<LocalizeManager>();
        Resource = transform.AddComponent<ResourceManager>();
        Pool = transform.AddComponent<PoolManager>();
        UI = transform.AddComponent<UIManager>();

        AudioManager = Resource.Instantiate("Managers/AudioManager", transform).GetComponent<AudioManager>();
    }

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            DontDestroyOnLoad(this);
            CreateManagers();
            instance = this;
        }
    }
}