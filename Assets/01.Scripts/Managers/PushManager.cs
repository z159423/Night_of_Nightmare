#if UNITY_IOS
using Unity.Notifications.iOS;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PushManager : MonoBehaviour
{
    private readonly Dictionary<string, int> androidNotificationIds = new();

    void Start()
    {
#if UNITY_ANDROID
        var channel = new Unity.Notifications.Android.AndroidNotificationChannel()
        {
            Id = "default_channel",
            Name = "Default Channel",
            Importance = Unity.Notifications.Android.Importance.Default,
            Description = "Generic notifications",
        };
        Unity.Notifications.Android.AndroidNotificationCenter.RegisterNotificationChannel(channel);
#elif UNITY_IOS
        // iOS는 AuthorizationRequest를 코루틴으로 기다리는 방식
        StartCoroutine(RequestIOSAuthorization());
#endif
    }

#if UNITY_IOS
    private IEnumerator RequestIOSAuthorization()
    {
        // 두 번째 파라미터: 원격 푸시 토큰 등록 여부(true면 APNs 토큰도 요청)
        var request = new AuthorizationRequest(
            AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound,
            true
        );

        while (!request.IsFinished)
            yield return null;

        if (!request.Granted)
        {
            Debug.LogError($"iOS 알림 권한이 거부되었습니다. Error: {request.Error}");
        }
        else
        {
            Debug.Log("iOS 알림 권한이 허용되었습니다.");
        }

        // 뱃지 리셋이 필요하다면(선택)
        iOSNotificationCenter.ApplicationBadge = 0;

        request.Dispose();
    }
#endif

    // 이하 기존 코드 그대로...
    public void ScheduleNotification(string title, string message, DateTime fireTime, string identifier = null)
    {
        if (string.IsNullOrEmpty(identifier))
            identifier = Guid.NewGuid().ToString();

#if UNITY_ANDROID
        var notification = new Unity.Notifications.Android.AndroidNotification
        {
            Title = title,
            Text = message,
            FireTime = fireTime
        };
        int androidId = Unity.Notifications.Android.AndroidNotificationCenter.SendNotification(notification, "default_channel");
        androidNotificationIds[identifier] = androidId;

#elif UNITY_IOS
        if (fireTime < DateTime.Now)
        {
            Debug.LogWarning("알림 시간이 현재 시간보다 이전입니다. 알림을 등록하지 않습니다.");
            return;
        }

        var notification = new iOSNotification
        {
            Identifier = identifier,
            Title = title,
            Body = message,
            ShowInForeground = true,
            ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
            Trigger = new iOSNotificationCalendarTrigger
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

    public void ScheduleNotificationAfterSeconds(string title, string message, int delaySeconds, string identifier = null)
    {
        DateTime fireTime = DateTime.Now.AddSeconds(delaySeconds);
        ScheduleNotification(title, message, fireTime, identifier);
    }

    public void CancelNotification(string identifier)
    {
#if UNITY_ANDROID
        if (androidNotificationIds.ContainsKey(identifier))
        {
            Unity.Notifications.Android.AndroidNotificationCenter.CancelNotification(androidNotificationIds[identifier]);
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

    public void CancelAllNotifications()
    {
#if UNITY_ANDROID
        Unity.Notifications.Android.AndroidNotificationCenter.CancelAllNotifications();
        androidNotificationIds.Clear();
#elif UNITY_IOS
        iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
        Debug.Log("모든 알림이 취소되었습니다.");
    }
}
