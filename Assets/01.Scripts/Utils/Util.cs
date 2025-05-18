using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using MoreMountains.NiceVibrations;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using DG.Tweening;
using Object = UnityEngine.Object;
using System.Runtime.InteropServices;

public static class Util
{
    private static Dictionary<GameObject, Dictionary<string, object>> _cachedFindComponent = new();

    private static Dictionary<float, WaitForSeconds> waitDic = new Dictionary<float, WaitForSeconds>();
    private static Dictionary<float, WaitForSecondsRealtime> realWaitDic = new Dictionary<float, WaitForSecondsRealtime>();

    public static WaitForSeconds GetWait(float waitSec)
    {
        if (waitDic.TryGetValue(waitSec, out WaitForSeconds waittime)) return waittime;
        return waitDic[waitSec] = new WaitForSeconds(waitSec);
    }

    public static void RemoveWait(float waitSec)
    {
        if (waitDic.ContainsKey(waitSec)) waitDic.Remove(waitSec);
    }

    public static WaitForSecondsRealtime GetRealWait(float waitSec)
    {
        if (realWaitDic.TryGetValue(waitSec, out WaitForSecondsRealtime waittime)) return waittime;
        return realWaitDic[waitSec] = new WaitForSecondsRealtime(waitSec);
    }
    public static WaitForFixedUpdate waitFixed = new WaitForFixedUpdate();

    public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    public static GameObject FindRecursive(this GameObject obj, string name)
         => FindChild(obj, name, true);

    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T SafeGet<T>(this T[] array, int index)
    {
        return array[Mathf.Clamp(index, 0, array.Length - 1)];
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>(true))
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }
        return null;
    }

    /// <summary>
    ///  Find + GetComponent 인데 캐싱을 곁들인
    /// </summary>
    public static T FindComponent<T>(this GameObject obj, string n) where T : Object
    {
        var keyN = $"{n}_{typeof(T).Name}";

        if (_cachedFindComponent.TryGetValue(obj, out var cachedComponent))
        {
            if (cachedComponent.TryGetValue(keyN, out var component))
                return component as T;

            if (typeof(T) == typeof(GameObject))
                _cachedFindComponent[obj][keyN] = obj.transform.Find(n).gameObject;
            else
                _cachedFindComponent[obj][keyN] = obj.transform.Find(n).GetComponent<T>();
        }
        else
        {
            _cachedFindComponent[obj] = new Dictionary<string, object>();

            if (typeof(T) == typeof(GameObject))
                _cachedFindComponent[obj][keyN] = obj.transform.Find(n).gameObject;
            else
                _cachedFindComponent[obj][keyN] = obj.transform.Find(n).GetComponent<T>();
        }

        return _cachedFindComponent[obj][keyN] as T;
    }

    /// <summary>
    ///  Find + GetComponent 인데 캐싱을 곁들인
    /// </summary>
    public static T FindComponent<T>(this Transform tf, string n) where T : Object
    {
        return FindComponent<T>(tf.gameObject, n);
    }

    /// <summary>
    ///  Find + GetComponent 인데 캐싱을 곁들인
    /// </summary>
    public static T FindComponent<T>(this MonoBehaviour mono, string n) where T : Object
    {
        return FindComponent<T>(mono.gameObject, n);
    }

    public static T DeepCopy<T>(T obj)
    {
        using (var stream = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            stream.Position = 0;

            return (T)formatter.Deserialize(stream);
        }
    }

    public static T StringToEnum<T>(string name) where T : Enum
    {
        return (T)Enum.Parse(typeof(T), name);
    }

    public static Tween ManualTo(Action<float> func, float duration, System.Action onComplete)
    {
        float elapse = 0f;
        return DOTween.To(() => elapse, (v) => { elapse = v; if (func != null) func(elapse); }, 1f, duration).
            OnComplete(() => { if (onComplete != null) onComplete(); });
    }

    ///<summary>분산랜덤</summary>
    /// <param name="variance">클수록 뾰족해짐! 최소값 기본값이 1</param>
    public static int DistributionRandom(int middleValue, float range, int variance = 1)
    {
        variance = Mathf.Max(1, variance);
        variance = (int)Math.Pow(10, variance);

        float u = UnityEngine.Random.Range(0.0f, 1.0f - Mathf.Epsilon);
        int ret = (int)(Mathf.Log(u / (1 - u), variance) * range + middleValue);

        while (ret < middleValue - range || ret > middleValue + range)
        {
            u = UnityEngine.Random.Range(0.0f, 1.0f - Mathf.Epsilon);
            ret = (int)(Mathf.Log(u / (1 - u), variance) * range + middleValue);
        }
        return ret;
    }

    /// <param name="power">1 lightimpact   //  2 MediumImpact //  3heavyimpact</param>
    public static void Haptic(int power = 1)
    {
        // if (!Managers.LocalData.UseHaptic) return;

        if (power == 1)
            MMVibrationManager.Haptic(HapticTypes.LightImpact);
        else if (power == 2)
            MMVibrationManager.Haptic(HapticTypes.MediumImpact);
        else
            MMVibrationManager.Haptic(HapticTypes.HeavyImpact);

    }

    public static List<T> Shuffle<T>(this List<T> c)
    {
        for (int i = 0; i < c.Count; i++)
        {
            var rnd = UnityEngine.Random.Range(0, c.Count);
            c.Insert(rnd, c[0]);
            c.RemoveAt(0);

            rnd = UnityEngine.Random.Range(0, c.Count);
            c.Add(c[rnd]);
            c.RemoveAt(rnd);
        }

        return c;
    }

    public static T RandomType<T>() where T : Enum
    {
        var list = new List<T>();
        foreach (T item in Enum.GetValues(typeof(T)))
            list.Add(item);
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static List<T> Swap<T>(this List<T> c, int index1, int index2)
    {
        var temp = c[index1];
        c[index1] = c[index2];
        c[index2] = temp;
        return c;
    }

    public static Vector3 UiToRealPos(Vector2 uiPos)
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(uiPos.x, uiPos.y, (Camera.main.nearClipPlane + Camera.main.farClipPlane) * 0.5f));
    }
    public static Vector2 RealPosToUi(Vector3 realPos)
    {
        return Camera.main.WorldToScreenPoint(realPos);
    }

    public static void GizmoText(string text, Vector3 worldPos, Color color, int fontSize = 12)
    {
#if UNITY_EDITOR
        var style = GUI.skin.box;
        style.alignment = TextAnchor.MiddleCenter;
        style.padding = new RectOffset(0, 0, 0, 0);
        style.margin = new RectOffset(0, 0, 0, 0);
        style.border = new RectOffset(0, 0, 0, 0);
        style.normal.textColor = color;
        style.fontSize = fontSize;
        UnityEditor.Handles.Label(worldPos, text, style);
#endif
    }

    public static void ParticleStart(GameObject particle)
    {
        particle.GetComponent<ParticleSystem>()?.PlayAllParticle();
    }

    public static void ParticleStop(GameObject particle)
    {
        particle.GetComponent<ParticleSystem>()?.StopAllParticle();
    }

    public static void PlayAllParticle(this ParticleSystem particle)
    {
        ParticleSystem[] particles = particle.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem part in particles)
        {
            part.Play();
        }
    }
    public static void StopAllParticle(this ParticleSystem particle)
    {
        ParticleSystem[] particles = particle.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem part in particles)
        {
            part.Stop();
        }
    }

    public static int GetRandomNumber(int[] nums, int maxNum)
    {
        HashSet<int> exclude;
        exclude = new HashSet<int>(nums);

        var range = Enumerable.Range(0, maxNum).Where(i => !exclude.Contains(i));

        var rand = new System.Random();
        int index = rand.Next(0, maxNum - exclude.Count);

        return range.ElementAt(index);
    }

    /// <summary>
    /// 시간, 분, 초, 미리초 삭제
    /// </summary>
    public static DateTimeOffset ZeroTime(this DateTimeOffset time)
    {
        return new DateTimeOffset(time.Year, time.Month, time.Day, 0, 0, 0, time.Offset);
    }

    /// <summary>
    /// 1초 정도의 시간 반올림 59 -> 00, 01 -> 00
    /// </summary>
    public static DateTimeOffset SimpleTime(this DateTimeOffset time)
    {
        if (time.Second == 59)
            time = time.AddSeconds(1);
        if (time.Second == 1)
            time = time.AddSeconds(-1);
        return time;
    }

    /// <summary>
    /// 10분 단위로 시간을 조정
    /// </summary>
    public static DateTimeOffset FetchTime(this DateTimeOffset time)
    {
        return time.AddSeconds(-time.Minute % 10 * 60 - time.Second);
    }

    public static string ToTimerTextFormat(int seconds)
    {
        // if (time < 60) return time + "s"; // seconds
        // else if (time < 3600) return (time / 60) + "m " + (time % 60) + "s"; // minute, seconds
        // else if (time < 86400) return (time / 3600) + "h " + ((time % 3600) / 60) + "m"; // hour, minute
        // else return (time / 86400) + "d " + ((time % 86400) / 3600) + "h " + (((time % 86400) % 3600) / 60) + "m"; // day, hour minute
        // if (seconds >= 86400)
        //     return (seconds / 86400).ToString("D2") + ":" + (seconds % 86400 / 3600).ToString("D2") + ":" + ((seconds % 3600) / 60).ToString("D2") + ":" + (seconds % 60).ToString("D2");
        if (seconds >= 86400)
            return (seconds / 86400).ToString("D2") + ":" + (seconds % 86400 / 3600).ToString("D2") + ":" + ((seconds % 3600) / 60).ToString("D2") + ":" + (seconds % 60).ToString("D2");
        return (seconds / 3600).ToString("D2") + ":" + ((seconds % 3600) / 60).ToString("D2") + ":" + (seconds % 60).ToString("D2"); // hour, minute
    }

    public static string ToTimerTextFormat_SlotType(int time)
    {
        if (time < 60) return time.ToString("D2") + "s"; // seconds
        else if (time < 3600) return (time / 60).ToString("D2") + "m\n" + (time % 60).ToString("D2") + "s"; // minute, seconds
        else if (time < 86400) return (time / 3600).ToString("D2") + "h\n" + ((time % 3600) / 60).ToString("D2") + "m"; // hour, minute
        else return (time / 86400).ToString("D2") + "d\n" + ((time % 86400) / 3600).ToString("D2") + "h\n" + (((time % 86400) % 3600) / 60).ToString("D2") + "m"; // day, hour minute
    }

    public static string ToSizeString(long size)
    {
        if (size >= 1024 * 1024)
            return $"{size / 1024f / 1024f:F1} MB";
        else if (size >= 1024)
            return $"{size / 1024f:F1} KB";
        else
            return $"{size} byte";
    }

    public static T DeserializeObject<T>(string res)
    {
        return JsonConvert.DeserializeObject<T>(res);
    }

    public static string GenerateRandomString(int length)
    {
        var chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        StringBuilder stringBuilder = new StringBuilder(length);
        for (int i = 0; i < length; i++)
            stringBuilder.Append(chars[UnityEngine.Random.Range(0, chars.Length)]);
        return stringBuilder.ToString();
    }

    public static Color HtmlToColor(string code)
    {
        return ColorUtility.TryParseHtmlString(code, out Color result) ? result : Color.white;
    }

    public static string GzipParse(byte[] bytes)
    {
        byte[] decompressedData;
        using (var compressedStream = new MemoryStream(bytes))
        {
            using (var gzipStream = new System.IO.Compression.GZipStream(compressedStream, System.IO.Compression.CompressionMode.Decompress))
            {
                using (var decompressedStream = new MemoryStream())
                {
                    gzipStream.CopyTo(decompressedStream);
                    decompressedData = decompressedStream.ToArray();
                }
            }
        }

        return Encoding.UTF8.GetString(decompressedData);
    }

    /// <summary>
    /// 특정 에니메이션의 길이를 반환하는 함수
    /// </summary>
    /// <param name="animator">에니메이션이 포함되어 있는 animator</param>
    /// <param name="animationName">길이를 찾을 animation</param>
    /// <returns></returns>
    public static float GetAnimationLength(Animator animator, string animationName)
    {
        // Animator의 모든 애니메이션 클립을 가져옴
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

        // 각 클립을 순회하면서 이름이 일치하는 클립을 찾음
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animationName)
            {
                // 클립의 길이를 반환
                return clip.length;
            }
        }

        // 일치하는 클립이 없는 경우 0을 반환하거나 오류 처리를 추가할 수 있음
        Debug.LogWarning($"Animation with name {animationName} not found.");
        return 0f;
    }
}

public static class CustomOpenURL
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _OpenURL(string url);
#endif

    public static void OpenURL(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("CustomOpenURL: URL is empty or null.");
            return;
        }

#if UNITY_IOS && !UNITY_EDITOR
        _OpenURL(url);
#elif UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW", new AndroidJavaObject("android.net.Uri", url)))
        {
            currentActivity.Call("startActivity", intent);
        }
#else
        Application.OpenURL(url);
#endif
    }
}