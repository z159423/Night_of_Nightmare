using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialTouchBox : UI_Popup
{
    enum Buttons
    {
        Btn
    }

    enum Images
    {
        TutorialBox,
        TouchGuard
    }

    public override void FirstSetting()
    {
        base.FirstSetting();
    }

    public void Setting(Button button, int tutorialIndex, bool timePause = false, bool descBubble = false)
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));

        var rect = button.GetComponent<RectTransform>();
        var myRect = gameObject.FindRecursive("Panel").GetComponent<RectTransform>();

        myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.width + 100);
        myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.rect.height + 100);

        myRect.position = rect.position;

        GetButton(Buttons.Btn).AddButtonEvent(() =>
        {
            Managers.LocalData.CompleteTutorial(tutorialIndex);
            button.onClick.Invoke();

            if (timePause)
                Time.timeScale = 1f;

            ClosePopupUI();
        });


        GetImage(Images.TouchGuard).gameObject.SetActive(false);

        GetImage(Images.TutorialBox)
            .DOFade(175f / 255f, 1f)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true) // timescale에 영향 안 받게 함
            .OnComplete(() =>
            {
                GetImage(Images.TouchGuard).gameObject.SetActive(true);
            });


        if (descBubble)
        {
            var bubble = Managers.Resource.Instantiate("TutorialBubbleUI", transform).GetComponent<TutorialBubbleUI>();
            bubble.Init();
            bubble.GetComponent<RectTransform>().position = new Vector2(myRect.position.x - 550, myRect.position.y);
            bubble.SetText((PlayerTutorialStep)tutorialIndex);
        }
    }
}
