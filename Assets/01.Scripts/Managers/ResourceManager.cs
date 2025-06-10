using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    ///<summary>이미 로드 되었던 Asset을 담고있는 Dic</summary>
    private readonly Dictionary<string, object> loadedAsset = new Dictionary<string, object>();

    ///<summary>Resources.Load의 역할을 대신함</summary>
    public T Load<T>(string name) where T : Object
    {
        //풀링되어있는경우 풀 오브젝트를 줄것
        if (typeof(T) == typeof(GameObject))
        {
            GameObject go = Managers.Pool.GetOriginal(name);
            if (go != null)
                return go as T;
        }
        if (loadedAsset.TryGetValue(name, out object value))
        {
            if (value == null)
            {
                loadedAsset.Remove(name);
                return Load<T>(name);
            }
            return value as T;
        }

        // Addressable
        // T ret = Managers.Global.Addressable.Load<T>(name);
        // if (ret != null)
        //     return ret;

        // 어디에도 없으면
        T ret = Resources.Load<T>(name);
        loadedAsset[name] = ret;
        return ret;
    }

    public T[] LoadAll<T>(string path) where T : Object
    {
        if (loadedAsset.TryGetValue(path, out object value))
        {
            if (value == null)
            {
                loadedAsset.Remove(path);
                return LoadAll<T>(path);
            }
            return value as T[];
        }

        T[] ret = Resources.LoadAll<T>(path);
        loadedAsset[path] = ret;
        return ret;
    }

    public GameObject Instantiate(string name, Transform parent = null)
    {
        GameObject original = Load<GameObject>(name);
        if (original == null)
        {
            Debug.LogError($"Failed to load prefab : {name}");
            return null;
        }

        if (original.GetComponent<Poolable>() != null)
        {
            var pop = Managers.Pool.Pop(original, parent).gameObject;

            var origRect = original.GetComponent<RectTransform>();
            if (origRect != null)
            {
                var popRect = pop.GetComponent<RectTransform>();
                popRect.anchorMin = origRect.anchorMin;
                popRect.anchorMax = origRect.anchorMax;
                popRect.anchoredPosition = origRect.anchoredPosition;
                popRect.sizeDelta = origRect.sizeDelta;
                popRect.localScale = origRect.localScale;
            }

            return pop;
        }

        GameObject go = Instantiate(original, parent);
        go.name = original.name;

        return go;
    }

    ///<summary>Object.Destroy 역할을 대신함 // timer에 변수 할당 시 timer(초) 후 반환 또는 파괴</summary>
    public void Destroy(GameObject go)
    {
        if (go == null)
            return;

        Poolable poolable = go.GetComponent<Poolable>();
        if (poolable != null)
        {
            Managers.Pool.Push(poolable);
            return;
        }

        Object.Destroy(go);
    }

    public void PopupCasting<T>(T popup) where T : UI_Popup
    {
        switch (popup)
        {

        }
    }

    /// <summary>
    /// 오브젝트의 자식 전부 부셔버림
    /// </summary>
    public void DestroyInChild(GameObject go)
    {
        var count = go.transform.childCount;
        for (int i = 0; i < count; i++)
            Destroy(go.transform.GetChild(i).gameObject);
    }

    public Sprite GetCharactorImage(int id)
    {
        return Resources.Load<Sprite>($"Charactors/spr_character_0{id}");
    }

    public Sprite GetCharactorIcons(int id)
    {
        return Resources.Load<Sprite>($"CharactorIcons/spr_character_{id}_ui");
    }

    public Sprite GetEnemyImage(int id)
    {
        return Resources.Load<Sprite>($"Enemy/spr_enemy_{id}");
    }
}
