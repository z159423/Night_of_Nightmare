using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

            yield return new WaitUntil(() => Managers.Localize.init);

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
