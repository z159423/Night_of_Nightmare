using Unity.Notifications.Android;
using Unity.Notifications.iOS;
using UnityEngine;
using System;
using System.Collections.Generic;

public class PushManager : MonoBehaviour
{
    // 안드로이드 알림 ID 저장용
    private Dictionary<string, int> androidNotificationIds = new Dictionary<string, int>();

    void Start()
    {
        // 채널 등록은 Start에서 한 번만
#if UNITY_ANDROID
        var channel = new AndroidNotificationChannel()
        {
            Id = "default_channel",
            Name = "Default Channel",
            Importance = Importance.Default,
            Description = "Generic notifications",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
#elif UNITY_IOS
        // 권한 요청은 Start에서 한 번만
        iOSNotificationCenter.RequestAuthorization(
            AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound,
            granted => Debug.Log("iOS Permission: " + granted)
        );
#endif
    }

    /// <summary>
    /// 로컬 푸시 알림을 등록합니다.
    /// </summary>
    /// <param name="title">알림 제목</param>
    /// <param name="message">알림 메시지</param>
    /// <param name="fireTime">알림 발동 시간</param>
    /// <param name="identifier">알림 식별자 (중복 제거용, null이면 자동 생성)</param>
    public void ScheduleNotification(string title, string message, DateTime fireTime, string identifier = null)
    {
        // 식별자가 없으면 자동 생성
        if (string.IsNullOrEmpty(identifier))
            identifier = Guid.NewGuid().ToString();

#if UNITY_ANDROID
        var notification = new AndroidNotification();
        notification.Title = title;
        notification.Text = message;
        notification.FireTime = fireTime;

        // 안드로이드에서는 SendNotification이 int ID를 반환함
        int androidId = AndroidNotificationCenter.SendNotification(notification, "default_channel");
        androidNotificationIds[identifier] = androidId;
        
#elif UNITY_IOS
        var notification = new iOSNotification()
        {
            Identifier = identifier,
            Title = title,
            Body = message,
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            Trigger = new iOSNotificationCalendarTrigger()
            {
                Year = fireTime.Year,
                Month = fireTime.Month,
                Day = fireTime.Day,
                Hour = fireTime.Hour,
                Minute = fireTime.Minute,
                Second = fireTime.Second,
                Repeats = false
            }
        };

        iOSNotificationCenter.ScheduleNotification(notification);
#endif

        Debug.Log($"알림 등록됨: {title} - {message} ({fireTime})");
    }

    /// <summary>
    /// 지정한 시간(초) 후에 알림을 등록합니다.
    /// </summary>
    /// <param name="title">알림 제목</param>
    /// <param name="message">알림 메시지</param>
    /// <param name="delaySeconds">몇 초 후에 알림을 보낼지</param>
    /// <param name="identifier">알림 식별자</param>
    public void ScheduleNotificationAfterSeconds(string title, string message, int delaySeconds, string identifier = null)
    {
        DateTime fireTime = DateTime.Now.AddSeconds(delaySeconds);
        ScheduleNotification(title, message, fireTime, identifier);
    }

    /// <summary>
    /// 특정 식별자의 알림을 취소합니다.
    /// </summary>
    /// <param name="identifier">취소할 알림의 식별자</param>
    public void CancelNotification(string identifier)
    {
#if UNITY_ANDROID
        if (androidNotificationIds.ContainsKey(identifier))
        {
            AndroidNotificationCenter.CancelNotification(androidNotificationIds[identifier]);
            androidNotificationIds.Remove(identifier);
        }
        else
        {
            Debug.LogWarning($"알림 ID를 찾을 수 없습니다: {identifier}");
        }
#elif UNITY_IOS
        iOSNotificationCenter.RemoveScheduledNotification(identifier);
#endif

        Debug.Log($"알림 취소됨: {identifier}");
    }

    /// <summary>
    /// 모든 등록된 알림을 취소합니다.
    /// </summary>
    public void CancelAllNotifications()
    {
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllNotifications();
        androidNotificationIds.Clear();
#elif UNITY_IOS
        iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif

        Debug.Log("모든 알림이 취소되었습니다.");
    }
}
