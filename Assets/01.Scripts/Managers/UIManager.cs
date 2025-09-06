using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public enum uiParticleMarkerType
{
    GemIcon,
    TicketIcon,
    BoostBtn
}

public class UIManager : MonoBehaviour
{
    public Dictionary<uiParticleMarkerType, Transform> uiParticleMarkers = new Dictionary<uiParticleMarkerType, Transform>();

    int _order = -10;

    List<UI_Popup> _popupList = new List<UI_Popup>();


    List<UI_Scene> _scenes = new List<UI_Scene>();
    public UI_Scene _currentScene;

    Queue<Type> _cleanPopupStack = new Queue<Type>();

    public UI_Popup _currentPopup;

    private GameObject notification;


    // Tribe //

    private int _totalPopupCount = 16; // change value after add new popup
    private int _settingPopupCount = 0;
    // Popup

    public Action OnPopupClosed = default;

    private GameObject _root;
    public GameObject Root
    {
        get
        {
            if (_root == null)
            {
                _root = new GameObject { name = "@UI_Root" };

                var canvas = _root.GetOrAddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 0;

                var scaler = _root.GetOrAddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1242, 2688);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 1f;

                var raycaster = _root.GetOrAddComponent<GraphicRaycaster>();
                raycaster.ignoreReversedGraphics = true;
                raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
                raycaster.blockingMask = ~0;
            }
            return _root;
        }
    }

    public void SetCanvas(GameObject go, bool sort = true)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;

        var raycaster = go.GetOrAddComponent<GraphicRaycaster>();
        raycaster.ignoreReversedGraphics = true;
        raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
        raycaster.blockingMask = ~0;

        if (sort)
        {
            canvas.sortingOrder = _order;
            _order++;
        }
        else
        {
            canvas.sortingOrder = 0;
        }
    }

    public void SetSortingOrder(GameObject go)
    {
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.sortingOrder = _order;
        _order++;
    }

    public T MakeSubItem<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate(name);
        if (parent != null)
            go.transform.SetParent(parent);

        return Util.GetOrAddComponent<T>(go);
    }

    public T ShowSceneUI<T>() where T : UI_Scene
    {
        T scene = null;

        var find = _scenes.Find(f => f.GetType() == typeof(T));
        if (find != null)
            scene = find as T;
        else
        {
            GameObject go = Managers.Resource.Instantiate(typeof(T).Name, Root.transform);
            scene = go.GetComponent<T>();
            _scenes.Add(scene);
        }

        if (_currentScene != null)
            _currentScene.Hide();
        _currentScene = scene;
        _currentScene.Show();

        return scene;
    }

    public T GetSceneUI<T>() where T : UI_Scene
    {
        var find = _scenes.Find(f => f.GetType() == typeof(T));
        if (find != null)
            return find as T;

        GameObject go = Managers.Resource.Instantiate(typeof(T).Name, Root.transform);
        var scene = go.GetComponent<T>();
        _scenes.Add(scene);

        scene.Hide();

        return scene;
    }

    /// <summary>
    /// 위에 작게뜨는 문구 팝업
    /// </summary>
    // public void ShowTextNotify(string langKey)
    // {
    //     var popup = ShowPopupUI<TextNoti_Popup>();
    //     popup.SetString(Managers.Localize.GetText(langKey));
    // }

    public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate(name, Root.transform);

        T popup = go.GetComponent<T>();
        _popupList.Add(popup);

        if (_settingPopupCount < _totalPopupCount)
            PopupCasting(popup);

        popup.Init();

        _currentPopup = popup;

        return popup;
    }

    /// <summary>
    /// 위에 작게뜨는 이펙트형 텍스트
    /// </summary>
    // public void ShowAlertText(string text, Vector2? textSizeDelta = null, Vector2? boxSizeDelta = null)
    // {
    //     if (_alertTextCanvas == null)
    //         _alertTextCanvas = Managers.Global.Resource.Instantiate("UI_AlertText").GetComponent<UI_AlertText>();

    //     if (!_alertTextCanvas.gameObject.activeSelf)
    //         _alertTextCanvas.gameObject.SetActive(true);

    //     _alertTextCanvas.Alert(text, textSizeDelta, boxSizeDelta);
    // }

    // public void ShowAddUnitCountUIEffect()
    // {
    //     if (_itemEffectCanvas == null)
    //     {
    //         _itemEffectCanvas = Managers.Global.Resource.Instantiate("UI_ItemEffect").GetComponent<UI_ItemEffect>();
    //     }
    //     if (!_itemEffectCanvas.gameObject.activeSelf)
    //     {
    //         _itemEffectCanvas.gameObject.SetActive(true);
    //     }

    //     _itemEffectCanvas.StartAddUnitCountEffect();
    // }

    // public void StartSpriteEffect(Sprite spr, Vector3 screenPos, Action onComplete = null)
    // {
    //     if (_itemEffectCanvas == null)
    //     {
    //         _itemEffectCanvas = Managers.Global.Resource.Instantiate("UI_ItemEffect").GetComponent<UI_ItemEffect>();
    //     }
    //     if (!_itemEffectCanvas.gameObject.activeSelf)
    //     {
    //         _itemEffectCanvas.gameObject.SetActive(true);
    //     }

    //     _itemEffectCanvas.StartSpriteEffect(spr, screenPos, onComplete);
    // }

    // public void StartDirectEffect(Item item)
    // {
    //     StartDirectEffect(item.Type, (item as EggItem)?.EggType ?? EggType.NORMAL);
    // }

    // // beta point 연출용, 베타 종료 후 삭제예정
    // public void StartDirectEffect_TribeBeta(TribeBetaPointType type)
    // {
    //     if (_itemEffectCanvas == null)
    //     {
    //         _itemEffectCanvas = Managers.Global.Resource.Instantiate("UI_ItemEffect").GetComponent<UI_ItemEffect>();
    //     }
    //     if (!_itemEffectCanvas.gameObject.activeSelf)
    //     {
    //         _itemEffectCanvas.gameObject.SetActive(true);
    //     }

    //     _itemEffectCanvas.StartBetaPointEffect(type);
    // }

    // /// <summary>
    // /// IAP Key값으로 연출
    // /// </summary>
    // /// <param name="iapKey"></param>
    // public void StartDirectEffect(string iapKey)
    // {
    //     if (DataRepository.Instance.IAPData.IAP.TryGetValue(iapKey, out var itemList))
    //     {
    //         foreach (var item in itemList)
    //         {
    //             var type = item.Type;
    //             if (item is FeverItem feverItem)
    //             {
    //                 type = feverItem.FeverType switch
    //                 {
    //                     FeverType.CoolTime => ItemType.CoolTimeFever,
    //                     FeverType.Mutation => ItemType.MutationRateFever,
    //                     FeverType.MovingSpeed => ItemType.MovingSpeedFever,
    //                     FeverType.Food => ItemType.MeatFever
    //                 };
    //             }

    //             StartDirectEffect(type, (item as EggItem)?.EggType ?? EggType.NORMAL);
    //         }
    //     }
    // }

    // public void StartDirectEffect(ItemType type, EggType eggType = default, Action action = null)
    // {
    //     if (_itemEffectCanvas == null)
    //     {
    //         _itemEffectCanvas = Managers.Global.Resource.Instantiate("UI_ItemEffect").GetComponent<UI_ItemEffect>();
    //     }
    //     if (!_itemEffectCanvas.gameObject.activeSelf)
    //     {
    //         _itemEffectCanvas.gameObject.SetActive(true);
    //     }

    //     _itemEffectCanvas.StartDirectEffect(type, action, eggType);
    // }

    // //홈 스킨용
    // public void StartDirectEffect(HomeSkinItem item)
    // {
    //     if (_itemEffectCanvas == null)
    //     {
    //         _itemEffectCanvas = Managers.Global.Resource.Instantiate("UI_ItemEffect").GetComponent<UI_ItemEffect>();
    //     }
    //     if (!_itemEffectCanvas.gameObject.activeSelf)
    //     {
    //         _itemEffectCanvas.gameObject.SetActive(true);
    //     }

    //     _itemEffectCanvas.StartDirectEffect(item);
    // }

    // public void StartClaimEffect(Item item)
    // {
    //     switch (item)
    //     {
    //         case HomeSkinItem:
    //         case EmojiItem:
    //         case FeverItem:
    //         case LevelPassItem:
    //             break;
    //         default:
    //             Managers.Game.UI.StartNewsLetterEffect(item);
    //             break;
    //     }
    // }

    // public void StartNewsLetterEffect(Item item)
    // {
    //     if (item is FeverItem feverItem)
    //     {
    //         feverItem.Type = feverItem.FeverType switch
    //         {
    //             FeverType.CoolTime => ItemType.CoolTimeFever,
    //             FeverType.Mutation => ItemType.MutationRateFever,
    //             FeverType.Food => ItemType.MeatFever,
    //             _ => feverItem.Type
    //         };
    //     }
    //     StartNewsLetterEffect(item.Type, (item as EggItem)?.EggType ?? EggType.NORMAL);
    // }

    // public void StartNewsLetterEffect(ItemType type, EggType eggType = default, Action action = null)
    // {
    //     if (_itemEffectCanvas == null)
    //     {
    //         _itemEffectCanvas = Managers.Global.Resource.Instantiate("UI_ItemEffect").GetComponent<UI_ItemEffect>();
    //     }
    //     if (!_itemEffectCanvas.gameObject.activeSelf)
    //     {
    //         _itemEffectCanvas.gameObject.SetActive(true);
    //     }

    //     _itemEffectCanvas.StartNewsLetterEffect(type, eggType, action);
    // }

    // public void StartInventoryEffect(ItemType type)
    // {
    //     if (_itemEffectCanvas == null)
    //     {
    //         _itemEffectCanvas = Managers.Global.Resource.Instantiate("UI_ItemEffect").GetComponent<UI_ItemEffect>();
    //     }
    //     if (!_itemEffectCanvas.gameObject.activeSelf)
    //     {
    //         _itemEffectCanvas.gameObject.SetActive(true);
    //     }

    //     _itemEffectCanvas.StartInventoryEffect(type);
    // }

    // public void CloseUIEffect()
    // {
    //     _itemEffectCanvas.gameObject.SetActive(false);
    // }

    public void PopupCasting<T>(T popup) where T : UI_Popup
    {
        switch (popup)
        {

        }
    }

    public void RemovePopup<T>() where T : UI_Popup
    {
        if (_popupList.Count == 0)
            return;

        // 리스트에서 T 타입의 첫번째 팝업을 찾습니다.
        UI_Popup popup = _popupList.Find(p => p is T);
        if (popup != null)
        {
            // 필요하다면 popup.Reset() 등 초기화 작업 후 파괴
            popup.Reset();
            Managers.Resource.Destroy(popup.gameObject);
            _popupList.Remove(popup);
            _order--;
        }
    }

    // 최종적으로 마지막에 Close 되는 함수
    public void ClosePopupUI()
    {
        if (_popupList.Count == 0)
            return;

        UI_Popup popup = _popupList[_popupList.Count - 1];
        if (popup != null)
        {
            popup.Reset();
            Managers.Resource.Destroy(popup.gameObject);
        }
        _popupList.RemoveAt(_popupList.Count - 1);
        _order--;
    }

    /// <summary>
    /// count만큼의 팝업은 남기고 나머지 팝업 닫는 함수
    /// </summary>
    /// <param name="count">남길 개수</param>
    public void ClosePopupsExcept(int count)
    {
        for (int i = _popupList.Count; i > count; i--)
            ClosePopupUI();
    }

    public void CloseAllPopupUI()
    {
        OnPopupClosed = default;
        while (_popupList.Count > 0)
        {
            ClosePopupUI();
        }
    }

    public bool IsShowPopup()
    {
        return _popupList.Count > 0;
    }

    public int GetPopupCount()
    {
        return _popupList.Count;
    }

    public bool CheckIsPopupActive<T>()
    {
        UI_Popup popup = _popupList.Find(p => p is T);
        if (popup != null)
        {
            return true;
        }
        else
            return false;
    }

    public GameObject GetPopupObject<T>()
    {
        var popup = _popupList.Find(p => p.GetComponent<T>() != null);
        if (popup != null)
        {
            return popup.gameObject;
        }
        else
            return null;
    }

    public bool IsUITouch()
    {
        bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
        {
            for (int index = 0; index < eventSystemRaysastResults.Count; index++)
            {
                RaycastResult curRaysastResult = eventSystemRaysastResults[index];
                if (curRaysastResult.gameObject.layer == 5)
                    return true;
            }
            return false;
        }

        List<RaycastResult> GetEventSystemRaycastResults()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;
            List<RaycastResult> raysastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raysastResults);
            return raysastResults;
        }

        return IsPointerOverUIElement(GetEventSystemRaycastResults()) || Managers.UI.IsShowPopup();
    }

    public T GetCurrentPopup<T>() where T : UI_Popup
    {
        if (_popupList.Count == 0) return null;

        return _popupList[_popupList.Count - 1].GetComponent<T>();
    }

    public void OnCloseAction()
    {
        OnPopupClosed?.Invoke();
    }

    public void Clear()
    {
        // CloseAllPopupUI();
        Destroy(Root);
        _currentScene = null;
    }

    #region UI Animation Part

    public void SettingUnitImageAnimator(ref Animator[] animators, int animIndex, GameObject target, int skinId, string partName)
    {
        StartCoroutine(CreateAnimator(animators, animIndex, target, skinId, partName));
    }

    private IEnumerator CreateAnimator(Animator[] animators, int animIndex, GameObject target, int skinId, string partName)
    {
        var animator = target.AddComponent<Animator>();
        if (animator == null)
        {
            yield return null;
            animator = target.AddComponent<Animator>();
        }
        animator.runtimeAnimatorController = LoadAnimatorController(skinId, partName);

        animators[animIndex] = animator;
    }

    private RuntimeAnimatorController LoadAnimatorController(int skinId, string partName)
    {
        return Managers.Resource.Load<RuntimeAnimatorController>($"AnimationSkinUIAnim_{skinId % 10000}_{partName}");
    }

    #endregion

    public void ShowNotificationPopup(string key, int index = 0)
    {
        if (notification != null)
        {
            notification.GetComponent<Notification_Popup>().Exit();
        }

        notification = Managers.Resource.Instantiate("Notification_Popup" + (index != 0 ? $"_{index}" : ""), Managers.UI.Root.transform).gameObject;
        notification.GetComponent<Notification_Popup>().Init();
        notification.GetComponent<Notification_Popup>().Setting(Managers.Localize.GetText(key));
    }

    public void GenerateUIParticle(Transform start, uiParticleMarkerType markerType, Define.ItemType item)
    {
        GameObject particle = Managers.Resource.Instantiate("UI_Particle", start);
        particle.GetComponent<Image>().sprite = GetItemIcon(item);
        particle.GetComponent<Image>().SetNativeSize();
        particle.transform.SetParent(Managers.UI.Root.transform);

        // 시작 위치에 랜덤 오프셋 추가 (±30픽셀 범위)
        Vector3 randomOffset = new Vector3(
            UnityEngine.Random.Range(-100f, 100f),
            UnityEngine.Random.Range(-100f, 100f),
            0f
        );
        particle.transform.position = start.position + randomOffset;

        // 초기 스케일을 0으로 설정
        particle.transform.localScale = Vector3.zero;

        // 시퀀스 생성
        Sequence sequence = DOTween.Sequence();

        // 1단계: 크기 0에서 뿅하고 나타나기 (약간 오버슈트)
        sequence.Append(particle.transform.DOScale(Vector3.one * 1.2f, 0.3f).SetEase(Ease.OutBack));

        // 2단계: 원래 크기로 살짝 줄어들기
        sequence.Append(particle.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.InOutQuad));

        // 3단계: 뒤로 살짝 물러나기 (목적지 반대 방향으로)
        Vector3 startPos = particle.transform.position; // 이미 랜덤 오프셋이 적용된 위치
        Vector3 endPos = Managers.UI.uiParticleMarkers[markerType].position;
        Vector3 direction = (endPos - startPos).normalized;

        // 후진 거리도 살짝 랜덤하게 (40~60픽셀)
        float backwardDistance = UnityEngine.Random.Range(40f, 60f);
        Vector3 backwardPos = startPos - direction * backwardDistance;

        sequence.Append(particle.transform.DOMove(backwardPos, 0.2f).SetEase(Ease.OutQuad));

        sequence.AppendInterval(UnityEngine.Random.Range(0, 0.2f));

        // 4단계: 목적지로 자연스럽게 이동 (베지어 커브처럼)
        sequence.Append(particle.transform.DOMove(endPos, UnityEngine.Random.Range(0.7f, 0.8f)).SetEase(Ease.InOutCubic));

        // 5단계: 도착하면서 살짝 축소
        sequence.Join(particle.transform.DOScale(Vector3.one * 0.8f, 0.8f).SetEase(Ease.InQuad));

        // 완료 시 파괴
        sequence.OnComplete(() =>
        {
            Managers.Resource.Destroy(particle);
        });
    }

    public void GenerateUIParticle(Transform start, Transform end, Sprite icon, Vector3 size)
    {
        GameObject particle = Managers.Resource.Instantiate("UI_Particle", start);
        particle.GetComponent<Image>().sprite = icon;
        particle.GetComponent<Image>().SetNativeSize();
        particle.transform.SetParent(Managers.UI.Root.transform);


        // 시작 위치에 랜덤 오프셋 추가 (±30픽셀 범위)
        Vector3 randomOffset = new Vector3(
            UnityEngine.Random.Range(-50f, 50f),
            UnityEngine.Random.Range(-50f, 50f),
            0f
        );
        particle.transform.position = start.position + randomOffset;

        // 초기 스케일을 0으로 설정
        particle.transform.localScale = Vector3.zero;

        // 시퀀스 생성
        Sequence sequence = DOTween.Sequence();

        // 1단계: 크기 0에서 뿅하고 나타나기 (약간 오버슈트)
        sequence.Append(particle.transform.DOScale(size * 1.2f, 0.3f).SetEase(Ease.OutBack));

        // 2단계: 원래 크기로 살짝 줄어들기
        sequence.Append(particle.transform.DOScale(size, 0.1f).SetEase(Ease.Linear));

        // 3단계: 뒤로 살짝 물러나기 (목적지 반대 방향으로)
        Vector3 startPos = particle.transform.position; // 이미 랜덤 오프셋이 적용된 위치
        Vector3 direction = (end.position - startPos).normalized;

        // 후진 거리도 살짝 랜덤하게 (40~60픽셀)
        float backwardDistance = UnityEngine.Random.Range(40f, 60f);
        Vector3 backwardPos = startPos - direction * backwardDistance;

        sequence.Append(particle.transform.DOMove(backwardPos, 0.2f).SetEase(Ease.OutQuad));

        sequence.AppendInterval(UnityEngine.Random.Range(0, 0.2f));

        // 4단계: 목적지로 자연스럽게 이동 (베지어 커브처럼)
        sequence.Append(particle.transform.DOMove(end.position, UnityEngine.Random.Range(0.7f, 0.8f)).SetEase(Ease.InOutCubic));

        // 5단계: 도착하면서 살짝 축소
        sequence.Join(particle.transform.DOScale(Vector3.one * 0.8f, 0.8f).SetEase(Ease.InQuad));

        // 완료 시 파괴
        sequence.OnComplete(() =>
        {
            Managers.Resource.Destroy(particle);
        });
    }

    public static Sprite GetItemIcon(Define.ItemType item)
    {
        return Managers.Resource.Load<Sprite>($"Icon/{item.ToString()}");
    }
}
