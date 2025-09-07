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
        public string serviceDate;                 // “서비스 날짜” (9시 기준)
        public int accumulatedSecondsToday;        // 오늘 접속 누적(초)
        public List<int> claimedMinutes;           // 수령 완료한 분 단위(10,20,...)
        public long lastSavedUnix;                 // 마지막 저장 시각(Unix)
        public long sessionStartUnix;              // 세션 시작 시각 (Unix)
    }

    private RewardState _state;
    private float _tick;                      // 1초 카운팅
    private int _sinceLastSave;               // 지난 저장 이후 경과(초)
    private bool _running;
    private bool _isFirstFrame;               // 첫 프레임 여부 플래그

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
        _running = true; // Start에서 running 설정
    }

    private void OnEnable()
    {
        // _state가 null일 수 있으므로 null 체크 추가
        if (_state != null)
        {
            _running = true;
            _isFirstFrame = true;
            
            // 세션 재시작 시각 기록
            _state.sessionStartUnix = NowUnix();
        }
    }

    private void OnDisable()
    {
        _running = false;
        // _state가 null일 수 있으므로 null 체크 추가
        if (_state != null)
        {
            Save();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        // _state가 null일 수 있으므로 null 체크 추가
        if (_state == null) return;
        
        if (pause)
        {
            // 백그라운드로 갈 때 현재까지의 접속 시간 저장
            _running = false; // 백그라운드에서는 시간 계산 중지
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
            _running = true; // 시간 계산 재시작
        }
    }

    private void OnApplicationQuit()
    {
        // _state가 null일 수 있으므로 null 체크 추가
        if (_state != null)
        {
            Save();
        }
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
        // (날짜 바뀌는 순간을 놓치지 않기 위함)
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

    // === [수정] TryClaim(): 수령 성공 시 캡 재계산 ===
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

                // ★ 추가: 로드 후 캡 재계산
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
            sessionStartUnix = NowUnix() // 세션 시작 시각 초기화
        };

        // ★ 추가: 초기화 후 캡 설정
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

    // === [수정] ResetForNewServiceDay(): 리셋 시 캡 초기화 ===
    private void ResetForNewServiceDay()
    {
        _state.serviceDate = GetServiceDate(DateTime.Now);
        _state.accumulatedSecondsToday = 0;
        _state.claimedMinutes.Clear();
        _sinceLastSave = 0;
        _tick = 0f;

        RecomputeCap(); // ★ 리셋 후 첫 구간(10분)으로 캡 설정
        Debug.Log("[SessionRewardManager] Daily reset at 9AM (local) applied.");
    }

    /// <summary>
    /// ‘서비스 날짜’ 계산: 로컬 시간 기준 9시 이전은 전날로 간주
    /// 예) 08:59 → 어제, 09:00 → 오늘
    /// </summary>
    private static string GetServiceDate(DateTime nowLocal)
    {
        // Unity는 기본적으로 로컬 타임존의 DateTime.Now 사용.
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

    // === [추가] 유틸: 다음 미수령 구간 인덱스 ===
    private int GetNextUnclaimedIndex()
    {
        // ThresholdSeconds: {600, 1200, 1800, 2400, 3000, 3600}
        for (int i = 0; i < ThresholdSeconds.Length; i++)
        {
            int minutes = ThresholdSeconds[i] / 60;
            if (!_state.claimedMinutes.Contains(minutes))
                return i;
        }
        return -1; // 모두 수령 완료
    }

    // === [추가] 캡 재계산 ===
    private void RecomputeCap()
    {
        int idx = GetNextUnclaimedIndex();
        if (idx < 0)
        {
            // 모든 보상 수령됨 → 더 누적해도 의미 없으니 마지막 구간(60분)에서 멈춤
            _capSeconds = ThresholdSeconds[ThresholdSeconds.Length - 1];
        }
        else
        {
            _capSeconds = ThresholdSeconds[idx];
        }

        // 이미 캡을 초과한 저장값이 있다면(이전 버전 데이터 등) 캡으로 절삭
        if (_state.accumulatedSecondsToday > _capSeconds)
            _state.accumulatedSecondsToday = _capSeconds;
    }

    public bool IsAlreadyClaimed(int minutes)
    {
        return _state.claimedMinutes.Contains(minutes);
    }

    // === [추가] 내부 유틸: 다음 리셋(로컬 09:00) DateTime ===
    private static DateTime GetNextResetLocalTime(DateTime nowLocal)
    {
        // 오늘 09:00
        var todayReset = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, 9, 0, 0);
        // 지금이 09:00 전이면 오늘 09:00, 아니면 내일 09:00
        return (nowLocal < todayReset) ? todayReset : todayReset.AddDays(1);
    }

    // === [추가] 공개 API: 다음 보상까지 남은 초(모두 수령했다면 -1, 수령 가능 상태면 0)
    public int SecondsToNextReward()
    {
        // 모든 보상 수령 여부
        int idx = GetNextUnclaimedIndex();
        if (idx < 0) return -1; // 오늘치 모두 끝

        // 캡(다음 미수령 보상 목표치)까지 남은 초
        int remaining = _capSeconds - _state.accumulatedSecondsToday;
        return remaining; // 이미 도달했으면 0 (수령 대기 상태)
    }

    // === [추가] 공개 API: 초기화(로컬 09:00)까지 남은 초
    public string SecondsToDailyReset()
    {
        DateTime now = DateTime.Now; // 로컬
        DateTime nextReset = GetNextResetLocalTime(now);
        var span = nextReset - now;
        // (정밀도를 초 단위로 내림 처리)
        return FormatHMS((int)Math.Max(0, Math.Floor(span.TotalSeconds)));
    }

    // === [선택] 포맷터: 초를 "HH:MM:SS"로 보기 좋게
    public static string FormatHMS(int seconds)
    {
        if (seconds < 0) return "--:--:--";
        var ts = TimeSpan.FromSeconds(seconds);
        // 100시간 이상도 깔끔히 표시되도록 TotalHours 사용
        int hours = (int)Math.Floor(ts.TotalHours);
        return $"{hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    public bool IsNextRewardClaimable()
    {
        int idx = GetNextUnclaimedIndex();
        if (idx < 0) return false; // 오늘 보상 모두 수령 완료
        int minutes = ThresholdSeconds[idx] / 60;
        return CanClaim(minutes);  // 캡 도달 & 미수령이면 true
    }

    /// <summary>
    /// 세션 보상 데이터를 완전히 삭제 (디버그/치트용)
    /// </summary>
    public void DeleteSaveData()
    {
        try
        {
            string path = GetSavePath();
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log("[SessionRewardManager] Save data deleted successfully.");
            }
            else
            {
                Debug.Log("[SessionRewardManager] No save data found to delete.");
            }

            // 메모리 상태도 초기화
            _state = new RewardState
            {
                serviceDate = GetServiceDate(DateTime.Now),
                accumulatedSecondsToday = 0,
                claimedMinutes = new List<int>(),
                lastSavedUnix = NowUnix(),
                sessionStartUnix = NowUnix()
            };

            // 캡 재설정
            RecomputeCap();
            
            // 카운터들 초기화
            _sinceLastSave = 0;
            _tick = 0f;
            _isFirstFrame = true;

            Debug.Log("[SessionRewardManager] Session reward data reset to initial state.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SessionRewardManager] Failed to delete save data: {e}");
        }
    }

    /// <summary>
    /// 강제로 오늘 데이터만 리셋 (파일은 유지하고 오늘 데이터만 초기화)
    /// </summary>
    public void ResetTodayData()
    {
        if (_state == null)
        {
            Debug.LogWarning("[SessionRewardManager] State is null, cannot reset today data.");
            return;
        }

        _state.serviceDate = GetServiceDate(DateTime.Now);
        _state.accumulatedSecondsToday = 0;
        _state.claimedMinutes.Clear();
        _state.sessionStartUnix = NowUnix();
        _sinceLastSave = 0;
        _tick = 0f;
        _isFirstFrame = true;

        RecomputeCap();
        Save();

        Debug.Log("[SessionRewardManager] Today's session reward data has been reset.");
    }

    /// <summary>
    /// [테스트용] 세션 보상 시간을 1분(60초) 앞당깁니다.
    /// 누적 시간을 60초 증가시켜서 다음 보상에 더 빨리 도달할 수 있게 합니다.
    /// </summary>
    public void TestAddOneMinute()
    {
        if (_state == null)
        {
            Debug.LogWarning("[SessionRewardManager] State is null, cannot add time.");
            return;
        }

        // 캡을 넘지 않는 선에서 60초 추가
        int targetSeconds = _state.accumulatedSecondsToday + 60;
        if (targetSeconds > _capSeconds)
        {
            targetSeconds = _capSeconds;
        }

        _state.accumulatedSecondsToday = targetSeconds;
        Save();

        Debug.Log($"[SessionRewardManager] 테스트용 - 1분(60초) 추가됨");
        Debug.Log($"현재 누적 시간: {_state.accumulatedSecondsToday}초 ({_state.accumulatedSecondsToday / 60}분)");
        Debug.Log($"다음 보상까지: {SecondsToNextReward()}초");
    }

    /// <summary>
    /// [테스트용] 다음 세션 보상을 바로 받을 수 있게 만듭니다.
    /// 현재 미수령 보상의 필요 시간까지 누적 시간을 설정합니다.
    /// </summary>
    public void TestSetNextRewardAvailable()
    {
        if (_state == null)
        {
            Debug.LogWarning("[SessionRewardManager] State is null, cannot set next reward.");
            return;
        }

        int nextIndex = GetNextUnclaimedIndex();
        if (nextIndex < 0)
        {
            Debug.Log("[SessionRewardManager] 모든 세션 보상이 이미 수령 완료되었습니다.");
            return;
        }

        int requiredSeconds = ThresholdSeconds[nextIndex];
        _state.accumulatedSecondsToday = requiredSeconds;
        Save();

        int minutes = requiredSeconds / 60;
        Debug.Log($"[SessionRewardManager] 테스트용 - {minutes}분 보상을 바로 받을 수 있도록 설정됨");
        Debug.Log($"현재 누적 시간: {_state.accumulatedSecondsToday}초 ({_state.accumulatedSecondsToday / 60}분)");
        Debug.Log($"수령 가능한 보상: {minutes}분");
    }

    /// <summary>
    /// [테스트용] 세션 보상을 완전히 초기화합니다.
    /// 모든 세션 보상 데이터를 삭제하고 0분부터 다시 시작합니다.
    /// </summary>
    public void TestResetSessionReward()
    {
        DeleteSaveData(); // 기존 메서드 활용
        Debug.Log("[SessionRewardManager] 테스트용 - 세션 보상 완전 초기화됨");
    }

    /// <summary>
    /// [테스트용] 특정 시간(분)까지 누적 시간을 설정합니다.
    /// </summary>
    /// <param name="targetMinutes">설정할 누적 시간(분)</param>
    public void TestSetAccumulatedTime(int targetMinutes)
    {
        if (_state == null)
        {
            Debug.LogWarning("[SessionRewardManager] State is null, cannot set time.");
            return;
        }

        if (targetMinutes < 0 || targetMinutes > 60)
        {
            Debug.LogError("[SessionRewardManager] 유효하지 않은 시간입니다. (0~60분 범위)");
            return;
        }

        int targetSeconds = targetMinutes * 60;
        
        // 캡을 넘지 않도록 조정
        if (targetSeconds > _capSeconds)
        {
            targetSeconds = _capSeconds;
        }

        _state.accumulatedSecondsToday = targetSeconds;
        Save();

        Debug.Log($"[SessionRewardManager] 테스트용 - 누적 시간을 {targetMinutes}분({targetSeconds}초)로 설정됨");
        Debug.Log($"다음 보상까지: {SecondsToNextReward()}초");
    }

    /// <summary>
    /// [테스트용] 모든 세션 보상을 수령 완료 상태로 만듭니다.
    /// </summary>
    public void TestCompleteAllRewards()
    {
        if (_state == null)
        {
            Debug.LogWarning("[SessionRewardManager] State is null, cannot complete rewards.");
            return;
        }

        // 60분까지 누적 시간 설정
        _state.accumulatedSecondsToday = 3600; // 60분 = 3600초
        
        // 모든 보상을 수령 완료로 설정
        _state.claimedMinutes.Clear();
        _state.claimedMinutes.AddRange(new int[] { 10, 20, 30, 40, 50, 60 });

        RecomputeCap();
        Save();

        Debug.Log("[SessionRewardManager] 테스트용 - 모든 세션 보상을 수령 완료 상태로 설정됨");
    }

    /// <summary>
    /// [테스트용] 특정 보상만 수령 완료 상태로 설정합니다.
    /// </summary>
    /// <param name="minutes">수령 완료로 설정할 보상 시간(10, 20, 30, 40, 50, 60)</param>
    public void TestClaimSpecificReward(int minutes)
    {
        if (_state == null)
        {
            Debug.LogWarning("[SessionRewardManager] State is null, cannot claim reward.");
            return;
        }

        if (minutes % 10 != 0 || minutes < 10 || minutes > 60)
        {
            Debug.LogError("[SessionRewardManager] 유효하지 않은 보상 시간입니다. (10, 20, 30, 40, 50, 60 중 선택)");
            return;
        }

        // 해당 시간까지 누적 시간 설정 (최소한)
        int requiredSeconds = minutes * 60;
        if (_state.accumulatedSecondsToday < requiredSeconds)
        {
            _state.accumulatedSecondsToday = requiredSeconds;
        }

        // 보상 수령 처리
        if (!_state.claimedMinutes.Contains(minutes))
        {
            _state.claimedMinutes.Add(minutes);
        }

        RecomputeCap();
        Save();

        Debug.Log($"[SessionRewardManager] 테스트용 - {minutes}분 보상을 수령 완료로 설정됨");
    }

    /// <summary>
    /// [테스트용] 현재 세션 보상 상태를 로그로 출력합니다.
    /// </summary>
    public void TestLogCurrentStatus()
    {
        if (_state == null)
        {
            Debug.LogWarning("[SessionRewardManager] State is null, cannot log status.");
            return;
        }

        Debug.Log("=== 세션 보상 현재 상태 ===");
        Debug.Log($"서비스 날짜: {_state.serviceDate}");
        Debug.Log($"누적 시간: {_state.accumulatedSecondsToday}초 ({_state.accumulatedSecondsToday / 60}분)");
        Debug.Log($"현재 캡: {_capSeconds}초 ({_capSeconds / 60}분)");
        Debug.Log($"수령 완료 보상: [{string.Join(", ", _state.claimedMinutes)}]분");
        Debug.Log($"다음 보상까지: {SecondsToNextReward()}초");
        Debug.Log($"다음 리셋까지: {SecondsToDailyReset()}");
        
        Debug.Log("=== 각 보상 상태 ===");
        foreach (var (minutes, reached, claimed) in GetAllRewardStatus())
        {
            string status = claimed ? "수령완료" : (reached ? "수령가능" : "미도달");
            Debug.Log($"{minutes}분 보상: {status} (필요: {minutes * 60}초)");
        }
        Debug.Log("========================");
    }
}
