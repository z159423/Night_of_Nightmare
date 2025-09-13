using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 기본 출석체크 매니저 (서버 없음 / 로컬 저장)
/// - UTC 자정 기준 하루 1회 수령 가능
/// - 여러 날 건너뛰어도 "다음 날 보상"만 진행(1일만 증가)
/// - 7일 보상 완료 후 종료
/// - PlayerPrefs 로 간단 저장
/// </summary>
public class AttendanceManager : MonoBehaviour
{
    // ======= 임시 보상 테이블(원하면 외부 JSON/ScriptableObject로 교체 가능) =======
    [Serializable]
    public struct Reward
    {
        public string id;     // 임시 보상 식별자 (예: "COIN", "GEM" 등)
        public int amount;    // 수량
    }

    // ======= 저장 키 =======
    const string KEY_LAST_CLAIM_UTC_DATE = "ATT_LAST_CLAIM_UTC_DATE"; // yyyy-MM-dd
    const string KEY_DAYS_CLAIMED = "ATT_DAYS_CLAIMED";         // 0~7 (진행 포인터)
    const string KEY_FINISHED = "ATT_FINISHED";             // 0/1

    // ======= 공개 상태 조회용 프로퍼티 =======
    /// <summary>지금까지 수령한 일수(0~7)</summary>
    public int DaysClaimed => PlayerPrefs.GetInt(KEY_DAYS_CLAIMED, 0);
    /// <summary>모든 7일 보상 완료 여부</summary>
    public bool IsFinished => PlayerPrefs.GetInt(KEY_FINISHED, 0) == 1;
    /// <summary>오늘(UTC) 날짜 문자열 "yyyy-MM-dd"</summary>
    public static string TodayUtcDateStr => DateTime.UtcNow.Date.ToString("yyyy-MM-dd");

    /// <summary>오늘 수령 가능 여부와 이유를 반환</summary>
    public bool CanClaimToday()
    {
        if (IsFinished)
        {
            return false;
        }

        string last = PlayerPrefs.GetString(KEY_LAST_CLAIM_UTC_DATE, string.Empty);
        string today = TodayUtcDateStr;

        if (!string.IsNullOrEmpty(last) && last == today)
        {
            return false;
        }

        if (DaysClaimed >= 7)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 오늘 수령 시도. 성공 시 true, 보상 정보 반환.
    /// - 여러 날 건너뛰었어도 하루에 1칸만 진행
    /// - 7일차 수령 시 완료 처리
    /// </summary>
    public bool TryClaimToday(out List<(Define.ItemType, uiParticleMarkerType)> items)
    {
        items = new List<(Define.ItemType, uiParticleMarkerType)>();

        if (!CanClaimToday())
            return false;

        int nextIndex = DaysClaimed; // 0-based 인덱스
        if (nextIndex < 0 || nextIndex >= 7)
        {
            return false;
        }

        // 지급될 아이템 목록

        switch (nextIndex)
        {
            case 0:
                Managers.LocalData.PlayerGemCount += 200;

                items.Add((Define.ItemType.Gem, uiParticleMarkerType.GemIcon));
                break;

            case 1:
                Managers.LocalData.SetCharactorOwned(Define.CharactorType.Chef, true);
                break;

            case 2:
                Managers.LocalData.PlayerGemCount += 300;
                Managers.LocalData.PlayerRvTicketCount += 3;
                items.Add((Define.ItemType.Gem, uiParticleMarkerType.GemIcon));
                items.Add((Define.ItemType.Ticket, uiParticleMarkerType.TicketIcon));
                break;

            case 3:
                Managers.LocalData.PlayerGemCount += 500;
                items.Add((Define.ItemType.Gem, uiParticleMarkerType.GemIcon));

                break;

            case 4:
                Managers.LocalData.PlayerGemCount += 600;
                Managers.LocalData.PlayerRvTicketCount += 6;
                items.Add((Define.ItemType.Gem, uiParticleMarkerType.GemIcon));
                items.Add((Define.ItemType.Ticket, uiParticleMarkerType.TicketIcon));
                break;

            case 5:
                Managers.LocalData.PlayerGemCount += 1000;
                items.Add((Define.ItemType.Gem, uiParticleMarkerType.GemIcon));
                break;

            case 6:
                Managers.LocalData.SetCharactorOwned(Define.CharactorType.LampGirl, true);
                break;
        }

        Managers.Firebase.GameEvent("Attendance_Claim", nextIndex.ToString());

        Managers.Audio.PlaySound("snd_get_item");

        // 상태 갱신
        PlayerPrefs.SetString(KEY_LAST_CLAIM_UTC_DATE, TodayUtcDateStr);
        PlayerPrefs.SetInt(KEY_DAYS_CLAIMED, nextIndex + 1);

        // 7일차 완료 처리
        bool isCompleted = false;
        if (nextIndex + 1 >= 7)
        {
            PlayerPrefs.SetInt(KEY_FINISHED, 1);
            isCompleted = true;
        }

        PlayerPrefs.Save();

        // 다음 출석체크 푸시 알림 예약 (완료되지 않은 경우에만)
        if (!isCompleted)
        {
            // 내일 오전 9시에 알림 예약
            DateTime tomorrow9AM = DateTime.Now.Date.AddDays(1).AddHours(9);

            Managers.Push.ScheduleNotification(Managers.Localize.GetText("non_title"), Managers.Localize.GetText("attendance_push"), tomorrow9AM, "Attendance");
        }

        return true;
    }

    /// <summary>
    /// 오늘 받을 예정인 보상을 미리보기(수령 가능할 때). 수령 불가면 false.
    /// </summary>
    public bool TryGetTodayPreview()
    {
        if (!CanClaimToday())
            return false;

        int nextIndex = DaysClaimed; // 0-based
        if (nextIndex < 0 || nextIndex >= 7)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 현재 진행 현황(일수/오늘 수령 여부/다음 수령 가능 시각)을 알려줍니다.
    /// </summary>
    public Status GetStatus()
    {
        string last = PlayerPrefs.GetString(KEY_LAST_CLAIM_UTC_DATE, string.Empty);
        bool claimedToday = (!string.IsNullOrEmpty(last) && last == TodayUtcDateStr);

        // 다음 수령 가능 시각(UTC 자정 이후)
        DateTime nextClaimAtUtc = DateTime.UtcNow.Date;
        if (claimedToday) nextClaimAtUtc = nextClaimAtUtc.AddDays(1);

        return new Status
        {
            daysClaimed = DaysClaimed,
            claimedToday = claimedToday,
            finished = IsFinished,
            nextClaimAtUtc = nextClaimAtUtc
        };
    }

    [Serializable]
    public struct Status
    {
        public int daysClaimed;      // 0~7
        public bool claimedToday;    // 오늘 수령했는지
        public bool finished;        // 7일 완료 여부
        public DateTime nextClaimAtUtc; // 다음 수령 가능(UTC 자정 이후)
    }

    /// <summary>
    /// 특정 날짜(1~7)의 보상을 획득 가능한지 확인합니다.
    /// </summary>
    /// <param name="day">확인할 출석체크 날짜 (1~7일차)</param>
    /// <param name="reason">획득 불가능한 이유</param>
    /// <returns>획득 가능하면 true, 불가능하면 false</returns>
    public bool CanClaimDay(int day, out string reason)
    {
        // 유효하지 않은 날짜 범위
        if (day < 1 || day > 7)
        {
            reason = "Invalid day range. Must be 1-7.";
            return false;
        }

        // 모든 보상이 완료된 경우
        if (IsFinished)
        {
            reason = "already claimed";
            return false;
        }

        int currentDaysClaimed = DaysClaimed; // 현재까지 수령한 일수 (0~7)

        // 이미 해당 일차의 보상을 받은 경우
        if (day <= currentDaysClaimed)
        {
            reason = "already claimed";
            return false;
        }

        // 순서대로만 받을 수 있음 (다음 차례가 아닌 경우)
        if (day != currentDaysClaimed + 1)
        {
            reason = "not yet";
            return false;
        }

        // 오늘 이미 받은 경우 (당일 중복 수령 방지)
        string last = PlayerPrefs.GetString(KEY_LAST_CLAIM_UTC_DATE, string.Empty);
        string today = TodayUtcDateStr;
        if (!string.IsNullOrEmpty(last) && last == today)
        {
            reason = "already claimed today";
            return false;
        }

        reason = null;
        return true;
    }

    /// <summary>
    /// 다음 수령까지 남은 시간을 반환합니다.
    /// </summary>
    /// <returns>남은 시간 TimeSpan. 이미 수령 가능하면 TimeSpan.Zero</returns>
    public TimeSpan GetTimeUntilNextClaim()
    {
        // 모든 보상이 완료된 경우
        if (IsFinished)
            return TimeSpan.Zero;

        string last = PlayerPrefs.GetString(KEY_LAST_CLAIM_UTC_DATE, string.Empty);
        string today = TodayUtcDateStr;

        // 오늘 아직 수령하지 않았거나, 처음 실행하는 경우
        if (string.IsNullOrEmpty(last) || last != today)
            return TimeSpan.Zero;

        // 오늘 이미 수령했으면 내일 UTC 자정까지 대기
        DateTime nextClaimTime = DateTime.UtcNow.Date.AddDays(1);
        TimeSpan timeUntilNext = nextClaimTime - DateTime.UtcNow;

        // 음수가 나오면 0으로 처리 (이미 수령 가능)
        return timeUntilNext.TotalSeconds > 0 ? timeUntilNext : TimeSpan.Zero;
    }

    /// <summary>
    /// 다음 수령까지 남은 시간을 문자열로 반환합니다.
    /// </summary>
    /// <returns>남은 시간 문자열 (예: "23:45:30", "수령 가능")</returns>
    public string GetTimeUntilNextClaimString()
    {
        TimeSpan timeLeft = GetTimeUntilNextClaim();

        if (timeLeft == TimeSpan.Zero)
            return "";
        // return Managers.Localize.GetText("attendance_ready");

        return Managers.Localize.GetDynamicText("attendance_remain_time", $"{timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}");
    }



    // ======= 예시: UI 버튼에서 호출 =======
    // public void OnClickClaim()
    // {
    //     if (TryClaimToday(out var reward, out var reason))
    //     {
    //         // TODO: 보상 지급 처리 (인벤토리/재화 등)
    //         Debug.Log($"Claimed: {reward.id} x {reward.amount}, Day {DaysClaimed}/7");
    //     }
    //     else
    //     {
    //         Debug.Log($"Cannot claim: {reason}");
    //     }
    // }

    /// <summary>
    /// [테스트용] 출석체크를 다음 날 획득 가능한 상태로 만듭니다.
    /// 현재 날짜를 어제로 설정하여 오늘 수령이 가능하도록 합니다.
    /// </summary>
    public void TestSetNextDayAvailable()
    {
        if (IsFinished)
        {
            Debug.LogWarning("AttendanceManager: 출석체크가 이미 완료된 상태입니다.");
            return;
        }

        // 어제 날짜로 설정하여 오늘 수령 가능하게 만듦
        DateTime yesterday = DateTime.UtcNow.Date.AddDays(-1);
        string yesterdayStr = yesterday.ToString("yyyy-MM-dd");

        PlayerPrefs.SetString(KEY_LAST_CLAIM_UTC_DATE, yesterdayStr);
        PlayerPrefs.Save();

        Debug.Log($"AttendanceManager: 테스트용 - 다음 출석체크 수령 가능하도록 설정됨 (마지막 수령일: {yesterdayStr})");
        Debug.Log($"현재 진행 상황: {DaysClaimed}/7일, 오늘 수령 가능: {CanClaimToday()}");
    }

    /// <summary>
    /// [테스트용] 출석체크를 완전히 리셋합니다.
    /// 모든 출석체크 데이터를 초기화하여 1일차부터 다시 시작할 수 있게 합니다.
    /// </summary>
    public void TestResetAttendance()
    {
        PlayerPrefs.DeleteKey(KEY_LAST_CLAIM_UTC_DATE);
        PlayerPrefs.DeleteKey(KEY_DAYS_CLAIMED);
        PlayerPrefs.DeleteKey(KEY_FINISHED);
        PlayerPrefs.Save();

        Debug.Log("AttendanceManager: 테스트용 - 출석체크 완전 리셋됨");
        Debug.Log($"현재 진행 상황: {DaysClaimed}/7일, 오늘 수령 가능: {CanClaimToday()}");
    }

    /// <summary>
    /// [테스트용] 특정 날짜로 출석체크 진행 상황을 설정합니다.
    /// </summary>
    /// <param name="targetDay">설정할 날짜 (0~7, 0=시작 전, 7=완료)</param>
    /// <param name="makeAvailableToday">오늘 수령 가능하게 할지 여부</param>
    public void TestSetAttendanceDay(int targetDay, bool makeAvailableToday = true)
    {
        if (targetDay < 0 || targetDay > 7)
        {
            Debug.LogError("AttendanceManager: 유효하지 않은 날짜입니다. (0~7 범위)");
            return;
        }

        // 진행 일수 설정
        PlayerPrefs.SetInt(KEY_DAYS_CLAIMED, targetDay);

        // 7일 완료 여부 설정
        if (targetDay >= 7)
        {
            PlayerPrefs.SetInt(KEY_FINISHED, 1);
            PlayerPrefs.SetString(KEY_LAST_CLAIM_UTC_DATE, TodayUtcDateStr);
        }
        else
        {
            PlayerPrefs.SetInt(KEY_FINISHED, 0);

            if (makeAvailableToday)
            {
                // 오늘 수령 가능하게 하려면 어제 날짜로 설정
                DateTime yesterday = DateTime.UtcNow.Date.AddDays(-1);
                PlayerPrefs.SetString(KEY_LAST_CLAIM_UTC_DATE, yesterday.ToString("yyyy-MM-dd"));
            }
            else
            {
                // 오늘 수령 불가능하게 하려면 오늘 날짜로 설정
                PlayerPrefs.SetString(KEY_LAST_CLAIM_UTC_DATE, TodayUtcDateStr);
            }
        }

        PlayerPrefs.Save();

        string availableText = targetDay >= 7 ? "완료됨" : (makeAvailableToday ? "가능" : "불가능");
        Debug.Log($"AttendanceManager: 테스트용 - {targetDay}일차로 설정됨, 오늘 수령: {availableText}");
        Debug.Log($"현재 진행 상황: {DaysClaimed}/7일, 오늘 수령 가능: {CanClaimToday()}");
    }

    /// <summary>
    /// [테스트용] 현재 출석체크 상태를 로그로 출력합니다.
    /// </summary>
    public void TestLogCurrentStatus()
    {
        var status = GetStatus();
        string lastClaimDate = PlayerPrefs.GetString(KEY_LAST_CLAIM_UTC_DATE, "없음");

        Debug.Log("=== 출석체크 현재 상태 ===");
        Debug.Log($"진행 일수: {status.daysClaimed}/7일");
        Debug.Log($"오늘 수령 여부: {status.claimedToday}");
        Debug.Log($"완료 여부: {status.finished}");
        Debug.Log($"마지막 수령 날짜: {lastClaimDate}");
        Debug.Log($"오늘 수령 가능: {CanClaimToday()}");
        Debug.Log($"다음 수령까지 남은 시간: {GetTimeUntilNextClaimString()}");
        Debug.Log($"오늘 UTC 날짜: {TodayUtcDateStr}");
        Debug.Log("=======================");
    }
}
