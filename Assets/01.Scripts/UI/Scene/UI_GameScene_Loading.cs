using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using LongriverSDKNS;

public class UI_GameScene_Loading : UI_Scene
{
    enum Images
    {
        Title2,

    }

    enum Texts
    {
        LoadingText
    }

    bool _init = false;

    static bool startedInit = false;

    static bool isTryAutoLogin = false;

    public override void Init()
    {
        base.Init();
        // Managers.UI.SetCanvas(gameObject, true);

        if (!_init)
        {
            FirstSetting();
        }

        if (startedInit)
        {
            Debug.Log("이미 초기화가 시작되었습니다.");
            return;
        }

        StartCoroutine(Loading());

        IEnumerator Loading()
        {
            startedInit = true;

            var titleImage = GetImage(Images.Title2);
            titleImage.DOFade(1.5f, 1f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

            // LongriverSDK.instance.Init();

            yield return new WaitForSeconds(2f);

            var loadingText = GetTextMesh(Texts.LoadingText);
            loadingText.gameObject.SetActive(true);
            loadingText.DOFade(1f, 1f)
                .SetEase(Ease.InOutSine);

            bool isLogin = false;

            Debug.Log("LongRiverSDK 초기화 여부 :" + LongriverSDK.instance.HasInit + " " + (LongriverSDK.instance != null));

            while (LongriverSDK.instance.HasInit == false)
            {
                yield return new WaitForSeconds(1f);

                Debug.Log("LongRiverSDK 초기화 중... " + LongriverSDK.instance.HasInit);
            }

            yield return new WaitUntil(() => LongriverSDK.instance != null && LongriverSDK.instance.HasInit);

            bool checkLogin = false;

            LongriverSDKUserPayment.instance.checkLoginAsync(delegate (CheckLoginResult r)
            {
                Debug.Log("check login success " + JsonUtility.ToJson(r));
                checkLogin = true;

            }, delegate (State s)
            {
                Debug.Log("check login fail " + JsonUtility.ToJson(s));
                checkLogin = true;
            });

            while (!checkLogin)
            {
                Debug.Log("로그인 상태 확인 중...");
                yield return new WaitForSeconds(0.5f);
            }

            if (LongriverSDKUserPayment.instance.isLogin())
            {
                Debug.Log("이미 로그인 상태입니다.");
                isLogin = true;
            }
            else
            {
                Debug.Log("로그인 시도 중...");

                if (!isTryAutoLogin)
                {
                    isTryAutoLogin = true;
                    LongriverSDKUserPayment.instance.autoLoginAsync(true, delegate (AutoLoginResult r)
                    {
                        print("autologin success " + JsonUtility.ToJson(r));
                        isLogin = true;

                    }, delegate (State s)
                    {
                        print("autologin fail " + JsonUtility.ToJson(s));

                        // if (LongriverSDKUserPayment.instance.isLogin())
                        // {
                        //     Debug.Log("이미 로그인 상태입니다.");

                        //     LongriverSDKUserPayment.instance.Logout();
                        //     LongriverSDKUserPayment.instance.autoLoginAsync(true, delegate (AutoLoginResult r)
                        //     {
                        //         print("autologin success " + JsonUtility.ToJson(r));
                        //         isLogin = true;

                        //     }, delegate (State s)
                        //     {
                        //         print("autologin fail " + JsonUtility.ToJson(s));
                        //     });
                        // }

                    });
                }
            }


            while (!isLogin)
                {
                    Debug.Log("로그인 중...");
                    yield return new WaitForSeconds(0.5f);
                }

            // yield return new WaitUntil(() => isLogin);

            LongriverSDKUserPayment.instance.setIPurchaseItemsListener(Managers.IAP);
            LongriverSDKUserPayment.instance.setTransactionStatusListener((State state) =>
            {
                Debug.Log("transaction status code: " + state.code);
            });

            while (!Managers.Localize.init || !isLogin)
            {
                Debug.Log("현재 진행 상황 : " + Managers.Localize.init + " " + Managers.IAP.init + " " + isLogin);
                yield return new WaitForSeconds(0.5f);
            }

            // yield return new WaitUntil(() => Managers.Localize.init && Managers.IAP.init && isLogin);

            LongriverSDKUserPayment.instance.getShopItemsAsync(delegate (ShopItemResult r)
            {
                Debug.Log("get item success " + JsonUtility.ToJson(r));
                var shopItemResult = r;

                Managers.IAP.shopItemResult = shopItemResult;

                foreach (var item in shopItemResult.items)
                {
                    Debug.Log($"Shop Item: {item.itemId}, Price: {item.price}, Type: {item.itemType}, Currency: {item.currency}, formattedPrice: {item.formattedPrice}");
                }

            }, delegate (State s)
            {
                Debug.Log("get item fail " + JsonUtility.ToJson(s));
            });

            var scene = Managers.UI.ShowSceneUI<UI_GameScene_Home>();
            Managers.Audio.PlaySound("bgm_base");
            scene.Init();
        }
    }

    public void FirstSetting()
    {
        _init = true;

        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    public override void Show()
    {
        gameObject.SetActive(true);
    }

    public override void Hide()
    {
        gameObject.SetActive(false);
    }
}
