using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using System;

public static class TextUtils
{
    static Dictionary<TextMeshProUGUI, Tween> _numberTweens = new Dictionary<TextMeshProUGUI, Tween>();

    public static bool CheckNickName(string str)
    {
        return Regex.IsMatch(str, "^[a-zA-Z0-9_-]{3,15}$");
    }

    public static bool CheckBanWord(string str)
    {
        var textFile = Resources.Load("badwords") as TextAsset;

        if (textFile == null) return true;

        var stringReader = new StringReader(textFile.text);

        while (true)
        {
            var banWord = stringReader.ReadLine();

            if (string.IsNullOrEmpty(banWord)) break;

            var removeHype = str.Replace("-", "");
            var removeUnderBar = removeHype.Replace("_", "");

            if (Regex.IsMatch(removeUnderBar, banWord, RegexOptions.IgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    public static Tween UINumberTween(TextMeshProUGUI text, int start, int end, float duration, string format = null, bool ts = false, Action<int> onChanging = null)
    {
        start = Mathf.Max(int.MinValue, start);
        end = Mathf.Max(int.MinValue, end);
        var value = start;
        if (string.IsNullOrEmpty(format))
            format = "%d";
        text.text = format.Replace("%d", ts ? $"{start:#,##0}" : $"{start}");
        if (_numberTweens.ContainsKey(text))
            _numberTweens[text].Kill();
        _numberTweens[text] = DOTween.To(() => value, (v) =>
        {
            value = v;
            text.text = format.Replace("%d", ts ? $"{v:#,##0}" : $"{v}");
            onChanging?.Invoke(value); // DOTween 값이 바뀔 때마다 onChanging 실행
        }, end, duration).SetEase(Ease.Linear);
        return _numberTweens[text];
    }

    public static string GetUTCFormat(DateTimeOffset timeOffset)
    {
        return timeOffset.ToString("yyyy - MMM - dd UTC h:mm tt");
    }

    public static string GetLocalTimeFormat(DateTimeOffset timeOffset)
    {
        return timeOffset.LocalDateTime.ToString("yyyy - MMM - dd h:mm tt<br>'(GMT'zzz')'");
    }

    public static string GetRemainFormat(TimeSpan timeSpan)
    {
        if (timeSpan.TotalSeconds < 0)
            timeSpan = TimeSpan.Zero;
        return $"{(int)timeSpan.TotalDays}d {timeSpan.ToString(@"hh\:mm\:ss")}";
    }

    public static string GetRemainHourFormat(TimeSpan timeSpan)
    {
        return $"{timeSpan.ToString(@"hh\:mm\:ss")}";
    }

    //시간 전체 표시
    public static string GetRemainHourFormat2(TimeSpan timeSpan)
    {
        if (timeSpan.TotalSeconds < 0)
        {
            return "00:00:00";
        }

        return $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }

    public static string GetPeriodFormat(DateTimeOffset start, DateTimeOffset end)
    {
        return $"{start.SimpleTime().LocalDateTime:MMM-dd HH:mm} ~ {end.SimpleTime().LocalDateTime:MMM-dd HH:mm}";
    }

    public static string GetReverseSting(string str)
    {
        return new string(str.ToCharArray().Reverse().ToArray());
    }

    public static string FormatNumberWithUnits(int number)
    {
        if (number >= 1_000_000_000)
        {
            return Math.Floor(number / 1_000_000_000.0).ToString() + "B";
        }
        else if (number >= 1_000_000)
        {
            return Math.Floor(number / 1_000_000.0).ToString() + "M";
        }
        else if (number >= 1_000)
        {
            return Math.Floor(number / 1_000.0).ToString() + "K";
        }
        else
        {
            return number.ToString();
        }
    }

    public static string FormatNumberWithUnits2(int number)
    {
        if (number >= 1_000_000_000)
        {
            return Math.Floor(number / 1_000_000_000.0).ToString() + "B";
        }
        else if (number >= 1_000_000)
        {
            return Math.Floor(number / 1_000_000.0).ToString() + "M";
        }
        else if (number >= 1_000)
        {
            return Math.Round(number / 1_000.0, 1).ToString() + "K";  // 소수점 첫째 자리까지 표시
        }
        else
        {
            return number.ToString();
        }
    }

    /// <summary>
    /// 4h 30min 50s 형식으로 반환
    /// </summary>
    public static string GetRemainFormatWord(TimeSpan timeSpan)
    {
        // 시간과 분을 추출
        int hours = timeSpan.Hours + (timeSpan.Days * 24); // days는 시간으로 변환
        int minutes = timeSpan.Minutes;

        // 포맷팅
        if (hours > 0 && minutes > 0)
        {
            return $"{hours}h {minutes}min";
        }
        else if (hours > 0)
        {
            return $"{hours}h";
        }
        else if (minutes > 0)
        {
            return $"{minutes}min";
        }
        else
        {
            return $"{timeSpan.Seconds}s"; // 시간이 없으면 초 단위로 표현
        }
    }
    public static string FormatWithCommas(int value)
    {
        return value.ToString("N0");
    }
    public static string FormatWithCommas(float value)
    {
        return ((int)value).ToString("N0");
    }
    public static string FormatWithCommas(double value)
    {
        return ((int)value).ToString("N0");
    }

    public static (string textKey, int time) GetMemberLastTimeValue(int min)
    {
        if (min / 1440 > 0) return ("time_day", min / 1440);
        else if (min / 60 > 0) return ("time_hour", min / 60);
        else return ("time_minute", min);
    }


}
