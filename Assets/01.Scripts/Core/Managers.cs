using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Text.RegularExpressions;


public class Managers : SingletonStatic<Managers>
{
    public static ResourceManager Resource { get; set; } = new ResourceManager();
    public static LocalizeManager Localize { get; set; } = new LocalizeManager();
    public static PoolManager Pool { get; set; } = new PoolManager();
    public static AudioManager Audio { get; set; } = new AudioManager();
    public static UIManager UI { get; set; } = new UIManager();
    public static LocalDataManager LocalData { get; set; } = new LocalDataManager();
    public static GameManager Game { get; set; } = new GameManager();
    public static CameraManager Camera { get; set; } = new CameraManager();
    public static IapManager IAP { get; set; } = new IapManager();
    public static AdManager Ad { get; set; } = new AdManager();
    public static AttendanceManager Attendance { get; set; } = new AttendanceManager();
    public static PushManager Push { get; set; } = new PushManager();
    public static SessionRewardManager SessionReward { get; set; } = new SessionRewardManager();
    public static AbilityManager Ability { get; set; } = new AbilityManager();
    public static TutorialManager Tutorial { get; set; } = new TutorialManager();
#if UNITY_EDITOR || Development_BUILD
    public static CheatManager Cheat { get; set; } = new CheatManager();
#endif


    public void CreateManagers()
    {
        DontDestroyOnLoad(this);

        Localize = transform.AddComponent<LocalizeManager>();
        Resource = transform.AddComponent<ResourceManager>();
        Pool = transform.AddComponent<PoolManager>();
        UI = transform.AddComponent<UIManager>();
        Game = transform.AddComponent<GameManager>();
        Camera = transform.AddComponent<CameraManager>();
        Audio = transform.AddComponent<AudioManager>();
        IAP = transform.AddComponent<IapManager>();
        Ad = transform.AddComponent<AdManager>();
        Attendance = transform.AddComponent<AttendanceManager>();
        Push = transform.AddComponent<PushManager>();
        SessionReward = transform.AddComponent<SessionRewardManager>();
        Ability = transform.AddComponent<AbilityManager>();
        Tutorial = transform.AddComponent<TutorialManager>();
#if UNITY_EDITOR || Development_BUILD
        Cheat = transform.AddComponent<CheatManager>();
#endif

    }

    private void Update()
    {
        LocalDataSave();
    }

    public void LocalDataSave()
    {
        if (LocalData?.IsSave ?? false)
        {
            PlayerPrefs.Save();
            LocalData.IsSave = false;
        }
    }
}