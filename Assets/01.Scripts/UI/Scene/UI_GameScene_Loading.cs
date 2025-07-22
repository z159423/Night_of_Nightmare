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

            yield return new WaitForSeconds(1f);

            bool isLogin = false;

            yield return new WaitUntil(() => LongriverSDK.instance != null && LongriverSDK.instance.HasInit);

            LongriverSDKUserPayment.instance.autoLoginAsync(true, delegate (AutoLoginResult r)
            {
                print("autologin success " + JsonUtility.ToJson(r));
                isLogin = true;

            }, delegate (State s)
            {
                print("autologin fail " + JsonUtility.ToJson(s));
            });

            // LongriverSDK.instance.SetDebugGeography(1);

            // string umpJson = LongriverSDK.instance.GetUMPParameters();

            // bool isRequired = LongriverSDK.instance.IsPrivacyOptionsRequired();
            // Dictionary<string, object> umpDict = Json.Deserialize(umpJson) as Dictionary<string, object>;
            // bool isGDPR = umpDict.ContainsKey("isGDPR") && (bool)umpDict["isGDPR"];

            // bool isPrivacyOptions = false;

            // try
            // {
            //     if (isGDPR && isRequired)
            //     {
            //         // 展示隐私选项界面
            //         LongriverSDK.instance.ShowPrivacyOptionsForm((State state) =>
            //         {
            //             if (state.code == 200)
            //             {
            //                 print($"privacy options - success - {state.code} - {state.msg}");
            //                 isPrivacyOptions = true;
            //             }
            //             else
            //             {
            //                 print($"privacy options - failed - {state.code} - {state.msg}");
            //                 isPrivacyOptions = false;
            //             }
            //         });
            //     }
            //     else
            //     {
            //         isPrivacyOptions = true;
            //     }
            // }
            // catch (System.Exception e)
            // {
            //     Debug.LogError($"Error showing privacy options: {e.Message}");
            //     isPrivacyOptions = true;
            // }

            yield return new WaitUntil(() => Managers.Localize.init && Managers.IAP.init && isLogin);

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
