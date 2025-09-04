using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class SessionRewardManager : MonoBehaviour
{
    // 10, 20, 30, 40, 50, 60 분 (초로 보관)
    private static readonly int[] ThresholdSeconds = { 600, 1200, 1800, 2400, 3000, 3600 };

    // 저장 파일명
    private const string SaveFileName = "session_rewards.json";

    // 1분 주기 저장/체크용
    private const int SaveIntervalSeconds = 60;

    private int _capSeconds = 0; // 현재 미수령 보상 구간의 목표 누적 초(도달하면 멈춤)

    [Serializable]
    private class RewardState
    {
        public string serviceDate;                 // "서비스 날짜" (9시 기준)
        public int accumulatedSecondsToday;        // 오늘 접속 누적(초)
        public List<int> claimedMinutes;           // 수령 완료한 분 단위(10,20,...)
        public long lastSavedUnix;                 // 마지막 저장 시각(Unix)
        public long sessionStartUnix;              // 현재 세션 시작 시각(Unix) - 추가
    }

    private RewardState _state;
    private float _tick;                      // 1초 카운팅
    private int _sinceLastSave;               // 지난 저장 이후 경과(초)
    private bool _running;
    private bool _isFirstFrame = true;        // 첫 프레임 체크용 - 추가

    // 더미 보상 테이블 (분 -> 더미 보상)
    private readonly List<int> _dummyRewards = new List<int>()
    {
        10, 20, 30, 40, 50, 60
    };

    // ====== Unity lifecycle ======
    private void Start()
    {
        LoadOrInit();
        CheckDailyResetIfNeeded();
        
        // 세션 시작 시각 기록
        _state.sessionStartUnix = NowUnix();
        _isFirstFrame = true;
    }

    private void OnEnable()
    {
        _running = true;
        _isFirstFrame = true;
        
        // 세션 재시작 시각 기록
        _state.sessionStartUnix = NowUnix();
    }

    private void OnDisable()
    {
        _running = false;
        Save();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            // 백그라운드로 갈 때 현재까지의 접속 시간 저장
            Save();
        }
        else
        {
            // 포그라운드로 복귀할 때
            CheckDailyResetIfNeeded();
            
            // 세션 재시작 시각 기록 (중요!)
            _state.sessionStartUnix = NowUnix();
            _isFirstFrame = true;
            _tick = 0f; // tick 초기화
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    private void Update()
    {
        if (!_running) return;

        // 첫 프레임은 deltaTime이 비정상적으로 클 수 있으므로 스킵
        if (_isFirstFrame)
        {
            _isFirstFrame = false;
            return;
        }

        // deltaTime 제한 (최대 1초로 제한하여 비정상적인 시간 점프 방지)
        float deltaTime = Mathf.Min(Time.unscaledDeltaTime, 1f);
        _tick += deltaTime;

        // 9시 경계는 1분 저장 타이밍에도 검사하지만, 실시간 반영을 위해 1초 루프에서도 가볍게 체크
        if (IsServiceDateChanged())
        {
            ResetForNewServiceDay();
        }

        // 1초마다 접속 시간 증가
        if (_tick >= 1f)
        {
            _tick -= 1f;

            // 캡까지 도달한 경우 더 이상 증가하지 않음
            if (_state.accumulatedSecondsToday < _capSeconds)
            {
                _state.accumulatedSecondsToday++;
                _sinceLastSave++;
            }

            if (_sinceLastSave >= SaveIntervalSeconds)
            {
                _sinceLastSave = 0;
                CheckDailyResetIfNeeded();
                Save();
            }
        }
    }

    // ====== Public API ======

    /// <summary>
    /// 현재 누적된 접속 시간(초)
    /// </summary>
    public int AccumulatedSecondsToday => _state.accumulatedSecondsToday;

    /// <summary>
    /// 지정된 분(10,20,...,60) 보상을 수령할 수 있는지 확인
    /// </summary>
    public bool CanClaim(int minutes)
    {
        if (minutes % 10 != 0 || minutes < 10 || minutes > 60) return false;
        if (IsServiceDateChanged()) return false; // 경계 넘어가면 false 처리(곧 리셋됨)
        if (_state.claimedMinutes.Contains(minutes)) return false;

        int need = minutes * 60;
        return _state.accumulatedSecondsToday >= need;
    }

    public bool TryClaim(int minutes, out List<(Define.ItemType, uiParticleMarkerType)> items)
    {
        items = new List<(Define.ItemType, uiParticleMarkerType)>();

        if (!CanClaim(minutes)) return false;

        if (_dummyRewards.Any(x => x == minutes))
        {
            _state.claimedMinutes.Add(minutes);
            Save();

            switch (minutes)
            {
                case 10:
                    Managers.LocalData.PlayerGemCount += 100;
                    items.Add((Define.ItemType.Gem, uiParticleMarkerType.GemIcon));
                    break;

                case 20:
                    Managers.LocalData.PlayerRvTicketCount += 2;
                    items.Add((Define.ItemType.Ticket, uiParticleMarkerType.TicketIcon));
                    break;

                case 30:
                    Managers.LocalData.PlayerLampCount += 1;
                    Managers.LocalData.playerHammerCount += 1;
                    Managers.LocalData.PlayerHolyShieldCount += 1;
                    Managers.LocalData.PlayerOverHeatCount += 1;
                    items.Add((Define.ItemType.Boost_Fire, uiParticleMarkerType.BoostBtn));
                    items.Add((Define.ItemType.Boost_Hammer, uiParticleMarkerType.BoostBtn));
                    items.Add((Define.ItemType.Boost_Ramp, uiParticleMarkerType.BoostBtn));
                    items.Add((Define.ItemType.Boost_Shield, uiParticleMarkerType.BoostBtn));
                    break;

                case 40:
                    Managers.LocalData.PlayerGemCount += 300;
                    items.Add((Define.ItemType.Gem, uiParticleMarkerType.GemIcon));
                    break;

                case 50:
                    Managers.LocalData.PlayerRvTicketCount += 2;
                    items.Add((Define.ItemType.Ticket, uiParticleMarkerType.TicketIcon));
                    break;

                case 60:
                    Managers.LocalData.PlayerLampCount += 3;
                    Managers.LocalData.playerHammerCount += 3;
                    Managers.LocalData.PlayerHolyShieldCount += 3;
                    Managers.LocalData.PlayerOverHeatCount += 3;
                    items.Add((Define.ItemType.Boost_Fire, uiParticleMarkerType.BoostBtn));
                    items.Add((Define.ItemType.Boost_Hammer, uiParticleMarkerType.BoostBtn));
                    items.Add((Define.ItemType.Boost_Ramp, uiParticleMarkerType.BoostBtn));
                    items.Add((Define.ItemType.Boost_Shield, uiParticleMarkerType.BoostBtn));
                    break;
            }

            Managers.Audio.PlaySound("snd_get_item");

            // 다음 구간으로 캡 이동
            RecomputeCap();
            return true;
        }

        // 더미에 없더라도 수령 처리
        _state.claimedMinutes.Add(minutes);
        Save();
        RecomputeCap();
        return false;
    }

    /// <summary>
    /// 다음 보상까지 남은 초 (모든 보상을 다 받았으면 -1)
    /// </summary>
    public int SecondsToNextThreshold()
    {
        int acc = _state.accumulatedSecondsToday;
        foreach (var th in ThresholdSeconds)
        {
            int minutes = th / 60;
            if (_state.claimedMinutes.Contains(minutes)) continue;
            if (acc < th) return th - acc;
            // 도달했는데 미수령이면 0
            if (acc >= th) return 0;
        }
        return -1; // 모두 완료
    }

    /// <summary>
    /// 각 보상(10,20,...,60)에 대해 (분, 도달여부, 수령여부) 반환
    /// UI에서 상태 뿌릴 때 사용
    /// </summary>
    public IEnumerable<(int minutes, bool reached, bool claimed)> GetAllRewardStatus()
    {
        int acc = _state.accumulatedSecondsToday;
        foreach (var th in ThresholdSeconds)
        {
            int m = th / 60;
            bool reached = acc >= th;
            bool claimed = _state.claimedMinutes.Contains(m);
            yield return (m, reached, claimed);
        }
    }

    // ====== Internal ======

    private void LoadOrInit()
    {
        string path = GetSavePath();
        if (File.Exists(path))
        {
            try
            {
                var json = File.ReadAllText(path);
                _state = JsonUtility.FromJson<RewardState>(json);
                if (_state == null) throw new Exception("State null after json parse.");

                if (string.IsNullOrEmpty(_state.serviceDate))
                {
                    _state.serviceDate = GetServiceDate(DateTime.Now);
                }
                if (_state.claimedMinutes == null) _state.claimedMinutes = new List<int>();

                // sessionStartUnix 초기화 (기존 저장 데이터에는 없을 수 있음)
                if (_state.sessionStartUnix == 0)
                {
                    _state.sessionStartUnix = NowUnix();
                }

                RecomputeCap();
                return;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SessionRewardManager] Load failed, re-init. {e}");
            }
        }

        // 초기 상태
        _state = new RewardState
        {
            serviceDate = GetServiceDate(DateTime.Now),
            accumulatedSecondsToday = 0,
            claimedMinutes = new List<int>(),
            lastSavedUnix = NowUnix(),
            sessionStartUnix = NowUnix()
        };

        RecomputeCap();
        Save();
    }

    private void Save()
    {
        try
        {
            _state.lastSavedUnix = NowUnix();
            var json = JsonUtility.ToJson(_state);
            File.WriteAllText(GetSavePath(), json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SessionRewardManager] Save failed: {e}");
        }
    }

    private void CheckDailyResetIfNeeded()
    {
        if (IsServiceDateChanged())
        {
            ResetForNewServiceDay();
            Save();
        }
    }

    private bool IsServiceDateChanged()
    {
        string currentServiceDate = GetServiceDate(DateTime.Now);
        return !string.Equals(_state.serviceDate, currentServiceDate, StringComparison.Ordinal);
    }

    private void ResetForNewServiceDay()
    {
        _state.serviceDate = GetServiceDate(DateTime.Now);
        _state.accumulatedSecondsToday = 0;
        _state.claimedMinutes.Clear();
        _sinceLastSave = 0;
        _tick = 0f;
        _state.sessionStartUnix = NowUnix(); // 새로운 세션 시작

        RecomputeCap();
        Debug.Log("[SessionRewardManager] Daily reset at 9AM (local) applied.");
    }

    /// <summary>
    /// '서비스 날짜' 계산: 로컬 시간 기준 9시 이전은 전날로 간주
    /// 예) 08:59 → 어제, 09:00 → 오늘
    /// </summary>
    private static string GetServiceDate(DateTime nowLocal)
    {
        if (nowLocal.Hour < 9)
            nowLocal = nowLocal.AddDays(-1);
        return nowLocal.ToString("yyyy-MM-dd");
    }

    private static string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    private static long NowUnix()
    {
        return (long)(DateTimeOffset.Now.ToUnixTimeSeconds());
    }

    private int GetNextUnclaimedIndex()
    {
        for (int i = 0; i < ThresholdSeconds.Length; i++)
        {
            int minutes = ThresholdSeconds[i] / 60;
            if (!_state.claimedMinutes.Contains(minutes))
                return i;
        }
        return -1;
    }

    private void RecomputeCap()
    {
        int idx = GetNextUnclaimedIndex();
        if (idx < 0)
        {
            _capSeconds = ThresholdSeconds[ThresholdSeconds.Length - 1];
        }
        else
        {
            _capSeconds = ThresholdSeconds[idx];
        }

        if (_state.accumulatedSecondsToday > _capSeconds)
            _state.accumulatedSecondsToday = _capSeconds;
    }

    public bool IsAlreadyClaimed(int minutes)
    {
        return _state.claimedMinutes.Contains(minutes);
    }

    private static DateTime GetNextResetLocalTime(DateTime nowLocal)
    {
        var todayReset = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, 9, 0, 0);
        return (nowLocal < todayReset) ? todayReset : todayReset.AddDays(1);
    }

    public int SecondsToNextReward()
    {
        int idx = GetNextUnclaimedIndex();
        if (idx < 0) return -1;

        int remaining = _capSeconds - _state.accumulatedSecondsToday;
        return remaining;
    }

    public string SecondsToDailyReset()
    {
        DateTime now = DateTime.Now;
        DateTime nextReset = GetNextResetLocalTime(now);
        var span = nextReset - now;
        return FormatHMS((int)Math.Max(0, Math.Floor(span.TotalSeconds)));
    }

    public static string FormatHMS(int seconds)
    {
        if (seconds < 0) return "--:--:--";
        var ts = TimeSpan.FromSeconds(seconds);
        int hours = (int)Math.Floor(ts.TotalHours);
        return $"{hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    public bool IsNextRewardClaimable()
    {
        int idx = GetNextUnclaimedIndex();
        if (idx < 0) return false;
        int minutes = ThresholdSeconds[idx] / 60;
        return CanClaim(minutes);
    }
}
