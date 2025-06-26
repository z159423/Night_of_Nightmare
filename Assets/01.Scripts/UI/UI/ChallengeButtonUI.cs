using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeButtonUI : UI_Base
{
    enum Images
    {

    }

    enum Texts
    {
        StageText,
        DiffText
    }

    enum Buttons
    {
        Btn
    }

    bool _init = false;

    public override void Init()
    {
        if (!_init)
        {
            FirstSetting();
        }
    }

    public void FirstSetting()
    {
        _init = true;

        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Button>(typeof(Buttons));
    }

    public void Setting(int stage, Action OnClick)
    {
        GetTextMesh(Texts.StageText).text = Managers.Localize.GetDynamicText("global.str_day_night", stage.ToString());
        GetTextMesh(Texts.DiffText).text = Managers.Localize.GetDynamicText("global.str_top_user", Define.ChallengeModeDiff[stage].ToString());
        GetTextMesh(Texts.DiffText).color = Define.ChallengeModeColor[stage];

        if (Managers.LocalData.ChallengeModeLevel < stage)
        {
            GetButton(Buttons.Btn).GetComponent<Image>().color = new Color32(25, 25, 25, 255);
        }
        else
        {
            GetButton(Buttons.Btn).AddButtonEvent(() =>
            {
                OnClick?.Invoke();
                var popup = Managers.UI.ShowPopupUI<Match_Making_Popup>();
                popup.Setting(true, stage);
                popup.StartMatching(true);
            });
        }
    }
}
