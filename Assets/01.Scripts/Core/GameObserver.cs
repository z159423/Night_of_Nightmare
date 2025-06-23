using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial struct GameObserverType
{
    public enum Game
    {
        OnChangeHomeLowerBtn,
        OnChangeCoinCount,
        OnActivePlayerBed,
        OnChangeEnergyCount,
        OnChangeTicketCount,
        OnChangeGemCount,
        OnMatchedPlayerCharactor,
        OnChangeStructure,
        OnChangeCharactor,
        OnChangeSelectedCharactorType,
        OnChangeLampCount,
        OnChangeHammerCount,
        OnChangeHolyShieldCount,
        OnChangeOverHeatCount
    }

    public enum Data
    {
        
    }
}

public static class GameObserverStatic
{
    public static void SetListener<T>(this T self, Enum type, Action action) where T : MonoBehaviour
    {
        GameObserver.Instance.SetListener(type, action, self.gameObject, typeof(T));
    }

    public static void RemoveListener<T>(this T self, Enum type) where T : MonoBehaviour
    {
        GameObserver.Instance.RemoveListener(type, self.gameObject, typeof(T));
    }

    public static void SetListener<T>(this T self, Enum type, Action<object> action) where T : MonoBehaviour
    {
        GameObserver.Instance.SetListener(type, action, self.gameObject, typeof(T));
    }
}

public class GameObserver : SingletonStatic<GameObserver>
{
    [RuntimeInitializeOnLoadMethod]
    static void LoadInit() { var t = Instance; }

    Dictionary<Enum, Dictionary<GameObject, Dictionary<Type, Action>>> callback = new Dictionary<Enum, Dictionary<GameObject, Dictionary<Type, Action>>>();
    Dictionary<Enum, Dictionary<GameObject, Dictionary<Type, Action<object>>>> callbackObject = new Dictionary<Enum, Dictionary<GameObject, Dictionary<Type, Action<object>>>>();

    /// <summary>
    /// this.SetListener 로 사용
    /// </summary>
    public void SetListener(Enum eventType, Action action, GameObject self, Type listenerType)
    {
        if (!callback.ContainsKey(eventType))
            callback[eventType] = new Dictionary<GameObject, Dictionary<Type, Action>>();
        if (!callback[eventType].ContainsKey(self))
            callback[eventType][self] = new Dictionary<Type, Action>();
        callback[eventType][self][listenerType] = action;

        // Clean
        var list = callback[eventType].ToList();
        foreach (var data in list)
        {
            if (data.Key == null)
                callback[eventType].Remove(data.Key);
        }
    }

    /// <summary>
    /// this.SetListener 로 사용
    /// </summary>
    public void SetListener(Enum eventType, Action<object> action, GameObject self, Type listenerType)
    {
        if (!callbackObject.ContainsKey(eventType))
            callbackObject[eventType] = new Dictionary<GameObject, Dictionary<Type, Action<object>>>();
        if (!callbackObject[eventType].ContainsKey(self))
            callbackObject[eventType][self] = new Dictionary<Type, Action<object>>();
        callbackObject[eventType][self][listenerType] = action;

        // Clean
        var list = callbackObject[eventType].ToList();
        foreach (var data in list)
        {
            if (data.Key == null)
                callbackObject[eventType].Remove(data.Key);
        }
    }

    /// <summary>
    /// this.RemoveListener 로 사용
    /// </summary>
    public void RemoveListener(Enum eventType, GameObject self, Type listenerType)
    {
        if (callback.ContainsKey(eventType))
        {
            if (callback[eventType].ContainsKey(self))
            {
                if (callback[eventType][self].ContainsKey(listenerType))
                    callback[eventType][self].Remove(listenerType);
                if (callback[eventType][self].Count == 0)
                    callback[eventType].Remove(self);
            }
            else if (self == null)
                callback[eventType].Remove(null);
        }

        if (callbackObject.ContainsKey(eventType))
        {
            if (callbackObject[eventType].ContainsKey(self))
            {
                if (callbackObject[eventType][self].ContainsKey(listenerType))
                    callbackObject[eventType][self].Remove(listenerType);
                if (callbackObject[eventType][self].Count == 0)
                    callbackObject[eventType].Remove(self);
            }
            else if (self == null)
                callbackObject[eventType].Remove(null);
        }
    }

    public static void Call(Enum type)
    {
        if (Instance.callback.ContainsKey(type))
        {
            foreach (var obj in Instance.callback[type].ToList())
            {
                if (obj.Key != null && obj.Key.activeInHierarchy)
                {
                    foreach (var com in obj.Value.ToList())
                    {
                        com.Value?.Invoke();
                    }
                }
            }
        }
    }

    public static void Call(Enum type, object o)
    {
        if (Instance.callbackObject.ContainsKey(type))
        {
            foreach (var obj in Instance.callbackObject[type].ToList())
            {
                if (obj.Key != null && obj.Key.activeInHierarchy)
                {
                    foreach (var com in obj.Value.ToList())
                    {
                        com.Value?.Invoke(o);
                    }
                }
            }
        }
    }
}
