using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonParent : MonoBehaviour
{
    static GameObject parent;
    public static GameObject Parent
    {
        get
        {
            if (parent == null)
            {
                parent = new GameObject("@Managers");
                DontDestroyOnLoad(parent);
            }
            return parent;
        }
    }
}

public class SingletonInstance<T> : MonoBehaviour where T : MonoBehaviour
{
    static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<T>();
            return instance;
        }
    }
}

public class SingletonStatic<T> : MonoBehaviour where T : MonoBehaviour
{   
    static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
                instance = SingletonParent.Parent.AddComponent<T>();
            return instance;
        }
    }

    public static bool InstanceExist
    {
        get => instance != null;
    }
}