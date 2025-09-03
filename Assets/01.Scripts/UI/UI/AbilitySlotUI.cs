using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.Mathematics;

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

    int abilityIndex = -1;
    int additionalAbilityIndex = -1;

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

        this.SetListener(GameObserverType.Game.OnAbilityChanged, () =>
        {
            UpdateUI();
        });
    }

    public void Setting(int abilityIndex, int additionalAbilityIndex = -1, bool first = false, bool last = false)
    {
        this.abilityIndex = abilityIndex;
        this.additionalAbilityIndex = additionalAbilityIndex;

        var ability = Managers.Ability.GetAbility(abilityIndex);
        var additionalAbility = Managers.Ability.GetAdditionalAbility(additionalAbilityIndex);

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

        if ((int)ability.needTier <= Managers.LocalData.PlayerHighestTier)
            GetButton(Buttons.AbilityBtn).image.color = Color.white;
        else
            GetButton(Buttons.AbilityBtn).image.color = new Color32(50, 50, 50, 255);

        GetButton(Buttons.AbilityBtn).AddButtonEvent(() =>
        {
            GetComponentInParent<Ability_Popup>().GenerateAbilityBubble(transform, abilityIndex, -1);
        });

        GetImage(Images.AbilityReddot).gameObject.SetActive(Managers.Ability.CanPurchaseAbility(abilityIndex));

        if (additionalAbility != null)
        {
            GetButton(Buttons.AdditionalBtn).image.sprite = Managers.Resource.Load<Sprite>($"Ability/ability_{(int)additionalAbility.type}");

            if ((int)additionalAbility.needTier <= Managers.LocalData.PlayerHighestTier)
                GetButton(Buttons.AdditionalBtn).image.color = Color.white;
            else
                GetButton(Buttons.AdditionalBtn).image.color = new Color32(50, 50, 50, 255);

            GetButton(Buttons.AdditionalBtn).AddButtonEvent(() =>
            {
                GetComponentInParent<Ability_Popup>().GenerateAbilityBubble(transform, -1, additionalAbilityIndex);
            });

            GetImage(Images.AdditionalReddot).gameObject.SetActive(Managers.Ability.CanPurchaseAdditionalAbility(additionalAbilityIndex));
        }
        else
        {
            GetImage(Images.Line2).gameObject.SetActive(false);
            GetButton(Buttons.AdditionalBtn).gameObject.SetActive(false);
        }

        GetTextMesh(Texts.NeedTierText).text = Define.GetTierName(ability.needTier);

        if (last)
        {
            GetImage(Images.Line3).gameObject.SetActive(false);
        }
    }

    void UpdateUI()
    {
        var additionalAbility = Managers.Ability.GetAdditionalAbility(additionalAbilityIndex);

        GetImage(Images.AbilityReddot).gameObject.SetActive(Managers.Ability.CanPurchaseAbility(abilityIndex));

        if (additionalAbility != null)
        {
            GetImage(Images.AdditionalReddot).gameObject.SetActive(Managers.Ability.CanPurchaseAdditionalAbility(additionalAbilityIndex));
        }
    }
}
