using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialTouchBox : UI_Popup
{
    enum Buttons
    {
        Btn
    }

    public override void FirstSetting()
    {
        base.FirstSetting();
    }

    public void Setting(Button button, int tutorialIndex)
    {
        Bind<Button>(typeof(Buttons));

        var rect = button.GetComponent<RectTransform>();
        var myRect = GetComponent<RectTransform>();

        myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.width + 100);
        myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.rect.height + 100);

        myRect.position = rect.position;

        GetButton(Buttons.Btn).AddButtonEvent(() =>
        {
            button.onClick.Invoke();

            Managers.LocalData.CompleteTutorial(tutorialIndex);
            ClosePopupUI();
        });
    }
}
