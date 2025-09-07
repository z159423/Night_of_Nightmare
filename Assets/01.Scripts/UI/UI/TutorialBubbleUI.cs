using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TutorialBubbleUI : UI_Base
{
    enum Buttons
    {
    }

    enum Images
    {
    }

    enum Texts
    {
        DescText,
    }

    bool init;

    public override void Init()
    {
        if (!init)
        {
            FirstSetting();
            init = true;
        }
    }

    public void FirstSetting()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    public void SetText(PlayerTutorialStep step)
    {
        GetTextMesh(Texts.DescText).text = Managers.Localize.GetText($"tutorial_{step}");

        GetComponent<CanvasGroup>().alpha = 0f;
        GetComponent<CanvasGroup>().DOFade(1f, 1f).SetEase(Ease.InOutSine).SetUpdate(true); // timescale에 영향 안 받게 함
    }
}
