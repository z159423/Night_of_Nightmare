using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Crashlytics;
using Firebase.Analytics;
using System;

public class FirebaseManager : MonoBehaviour
{
    private static bool _isInitialized = false;
    private static System.Action _onInitialized;
    void Start()
    {
        InitializeFirebase();

        // 즉시 크래시 테스트 실행 (메인 스레드)
        // Invoke(nameof(TestCrashOnMainThread), 5f);
    }

    private void TestCrashOnMainThread()
    {
        Debug.LogWarning("메인 스레드에서 크래시 테스트를 실행합니다!");
        throw new System.Exception("메인 스레드 크래시 테스트 - Firebase Crashlytics 테스트용");
    }

    private void InitializeFirebase()
    {
        Debug.Log("Firebase 초기화 시작...");

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Firebase 앱 인스턴스 생성
                Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

                Debug.Log("Firebase 초기화 완료!");

                // Crashlytics 설정
                Crashlytics.ReportUncaughtExceptionsAsFatal = true;
                Debug.Log("Firebase Crashlytics 활성화");

                // Analytics 초기화
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                Debug.Log("Firebase Analytics 활성화");

                // 초기화 완료 플래그 설정
                _isInitialized = true;

                // 대기 중인 이벤트들 실행
                _onInitialized?.Invoke();
                _onInitialized = null;

                Debug.Log("Firebase 모든 서비스 초기화 완료");

                Firebase.FirebaseApp.LogLevel = Firebase.LogLevel.Debug;
            }
            else
            {
                Debug.LogError($"Firebase dependencies를 해결할 수 없습니다: {dependencyStatus}");
                // Firebase Unity SDK 사용 불가
            }
        });
    }

    public static bool IsInitialized => _isInitialized;

    public void GameEvent(string parameterName, string parameterValue)
    {
        var parameters = new Dictionary<string, string>
        {
            { parameterName, parameterValue },
            { "version", Application.version }
        };

        LogCustomEvent("Game", parameters);
    }

    public static void LogCustomEvent(string eventName, Dictionary<string, string> parameters)
    {
        if (!_isInitialized)
        {
            _onInitialized += () => LogCustomEvent(eventName, parameters);
            return;
        }

        if (parameters == null)
        {
            FirebaseAnalytics.LogEvent(eventName);
        }
        else
        {
            var eventParams = new Parameter[parameters.Count];
            int i = 0;
            foreach (var item in parameters)
            {
                eventParams[i++] = new Parameter(item.Key, item.Value);
            }
            FirebaseAnalytics.LogEvent(eventName, eventParams);
        }
    }

    // 간단한 이벤트 로깅 (매개변수 없음)
    public static void LogEvent(string eventName)
    {
        LogCustomEvent(eventName, null);
    }

    // 하나의 매개변수를 가진 이벤트 로깅
    public static void LogEvent(string eventName, string parameterName, string parameterValue)
    {
        var parameters = new Dictionary<string, string>
        {
            { parameterName, parameterValue }
        };
        LogCustomEvent(eventName, parameters);
    }

    // 숫자 값을 가진 매개변수 이벤트 로깅
    public static void LogEvent(string eventName, string parameterName, long parameterValue)
    {
        if (!_isInitialized)
        {
            _onInitialized += () => LogEvent(eventName, parameterName, parameterValue);
            return;
        }

        FirebaseAnalytics.LogEvent(eventName, new Parameter(parameterName, parameterValue));
    }

    // 사용자 속성 설정
    public static void SetUserProperty(string name, string property)
    {
        if (!_isInitialized)
        {
            _onInitialized += () => SetUserProperty(name, property);
            return;
        }

        FirebaseAnalytics.SetUserProperty(name, property);
    }

    // 사용자 ID 설정
    public static void SetUserId(string userId)
    {
        if (!_isInitialized)
        {
            _onInitialized += () => SetUserId(userId);
            return;
        }

        FirebaseAnalytics.SetUserId(userId);
    }

    // Crashlytics 테스트 메서드들

    /// <summary>
    /// 강제로 크래시를 발생시켜 Crashlytics 테스트
    /// 주의: 실제 앱이 종료됩니다!
    /// </summary>
    public static void TestCrash()
    {
        Debug.LogWarning("Crashlytics 테스트 크래시를 발생시킵니다!");
        throw new System.Exception("Firebase Crashlytics 테스트용 크래시입니다.");
    }

    /// <summary>
    /// 치명적이지 않은 예외를 Crashlytics에 로그
    /// 앱이 종료되지 않으면서 예외 상황을 기록
    /// </summary>
    /// <param name="message">예외 메시지</param>
    public static void LogNonFatalException(string message)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("Firebase가 초기화되지 않았습니다.");
            return;
        }

        var exception = new System.Exception($"Non-Fatal Exception: {message}");
        Crashlytics.LogException(exception);
        Debug.Log($"Non-Fatal Exception 로그됨: {message}");
    }

    /// <summary>
    /// 커스텀 키-값 쌍을 Crashlytics에 설정
    /// 크래시 발생 시 추가 컨텍스트 정보 제공
    /// </summary>
    /// <param name="key">키</param>
    /// <param name="value">값</param>
    public static void SetCrashlyticsCustomKey(string key, string value)
    {
        if (!_isInitialized)
        {
            _onInitialized += () => SetCrashlyticsCustomKey(key, value);
            return;
        }

        Crashlytics.SetCustomKey(key, value);
        Debug.Log($"Crashlytics 커스텀 키 설정: {key} = {value}");
    }

    /// <summary>
    /// Crashlytics에 커스텀 로그 메시지 추가
    /// 크래시 발생 시 디버깅에 도움이 되는 정보 제공
    /// </summary>
    /// <param name="message">로그 메시지</param>
    public static void LogCrashlyticsMessage(string message)
    {
        if (!_isInitialized)
        {
            _onInitialized += () => LogCrashlyticsMessage(message);
            return;
        }

        Crashlytics.Log(message);
        Debug.Log($"Crashlytics 로그: {message}");
    }

    /// <summary>
    /// 게임 상태 정보를 Crashlytics에 설정 (디버깅용)
    /// </summary>
    public static void SetGameStateForCrashlytics()
    {
        if (!_isInitialized)
        {
            _onInitialized += SetGameStateForCrashlytics;
            return;
        }

        // 게임 상태 정보 설정
        SetCrashlyticsCustomKey("game_version", Application.version);
        SetCrashlyticsCustomKey("unity_version", Application.unityVersion);
        SetCrashlyticsCustomKey("platform", Application.platform.ToString());
        SetCrashlyticsCustomKey("device_model", SystemInfo.deviceModel);
        SetCrashlyticsCustomKey("device_memory", SystemInfo.systemMemorySize.ToString());

        // 게임 특화 정보 (예시)
        if (Managers.LocalData != null)
        {
            SetCrashlyticsCustomKey("player_level", Managers.LocalData.PlayerHighestTier.ToString());
            SetCrashlyticsCustomKey("selected_character", Managers.LocalData.SelectedCharactor.ToString());
        }

        LogCrashlyticsMessage("게임 상태 정보가 Crashlytics에 설정되었습니다.");
    }
}
