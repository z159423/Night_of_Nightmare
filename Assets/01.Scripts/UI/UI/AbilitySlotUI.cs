using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class AbilitySlotUI : UI_Base
{
    enum Buttons
    {
        AbilityBtn,
        AdditionalBtn
    }

    enum Images
    {
        AbilityReddot,
        AdditionalReddot,
        Line1,
        Line2,
        Line3,
        NeedTier
    }

    enum Texts
    {
        NeedTierText
    }

    bool init;

    public override void Init()
    {
        if (!init)
        {
            FirstSetting();
            init = true;
        }

        this.SetListener(GameObserverType.Game.OnChangeGemCount, () =>
        {
            UpdateUI();
        });
    }

    public void FirstSetting()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    public void Setting(Ability ability, Ability additionalAbility = null, bool first = false, bool last = false)
    {
        if (first)
        {
            GetImage(Images.Line1).gameObject.SetActive(true);
            GetImage(Images.NeedTier).gameObject.SetActive(true);
            GetTextMesh(Texts.NeedTierText).gameObject.SetActive(true);
        }
        else
        {
            GetImage(Images.Line1).gameObject.SetActive(false);
            GetImage(Images.Line2).gameObject.SetActive(false);
            GetImage(Images.NeedTier).gameObject.SetActive(false);
            GetTextMesh(Texts.NeedTierText).gameObject.SetActive(false);
        }

        GetButton(Buttons.AbilityBtn).image.sprite = Managers.Resource.Load<Sprite>($"Ability/ability_{(int)ability.type}");

        GetButton(Buttons.AbilityBtn).AddButtonEvent(() =>
        {

        });

        if (additionalAbility != null)
        {
            GetButton(Buttons.AdditionalBtn).image.sprite = Managers.Resource.Load<Sprite>($"Ability/ability_{additionalAbility.type}");

            GetButton(Buttons.AdditionalBtn).AddButtonEvent(() =>
            {

            });
        }
        else
        {
            GetImage(Images.Line2).gameObject.SetActive(false);
            GetButton(Buttons.AdditionalBtn).gameObject.SetActive(false);
        }

        GetTextMesh(Texts.NeedTierText).text = $"[{ability.needTier}]";

        if (last)
        {
            GetImage(Images.Line3).gameObject.SetActive(false);
        }

        UpdateUI();
    }

    void UpdateUI()
    {

    }
}
