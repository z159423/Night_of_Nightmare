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

    public override void Init()
    {
        base.Init();
        // Managers.UI.SetCanvas(gameObject, true);

        if (!_init)
        {
            FirstSetting();
        }

        StartCoroutine(Loading());

        IEnumerator Loading()
        {
            var titleImage = GetImage(Images.Title2);
            titleImage.DOFade(1.5f, 1f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

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

                LongriverSDK.instance.Init();
                Debug.Log("LongRiverSDK 초기화 중... " + LongriverSDK.instance.HasInit);
            }
            yield return new WaitUntil(() => LongriverSDK.instance != null && LongriverSDK.instance.HasInit);

            while (!LongriverSDKUserPayment.instance.isLogin())
            {
                LongriverSDKUserPayment.instance.autoLoginAsync(true, delegate (AutoLoginResult r)
                {
                    print("autologin success " + JsonUtility.ToJson(r));
                    isLogin = true;

                }, delegate (State s)
                {
                    print("autologin fail " + JsonUtility.ToJson(s));

                    LongriverSDKUserPayment.instance.Logout();
                });

                if (!LongriverSDKUserPayment.instance.isLogin())
                    yield return new WaitForSeconds(1f);
            }

            LongriverSDKUserPayment.instance.setIPurchaseItemsListener(Managers.IAP);
            LongriverSDKUserPayment.instance.setTransactionStatusListener((State state) =>
            {
                Debug.Log("transaction status code: " + state.code);
            });

            Debug.Log("현재 진행 상황 : " + Managers.Localize.init + " " + Managers.IAP.init + " " + isLogin);

            yield return new WaitUntil(() => Managers.Localize.init && Managers.IAP.init && isLogin);

            LongriverSDKUserPayment.instance.getShopItemsAsync(delegate (ShopItemResult r)
            {
                Debug.Log("get item success " + JsonUtility.ToJson(r));
                var shopItemResult = r;

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
