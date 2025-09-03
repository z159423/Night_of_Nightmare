using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.Mathematics;

public class AbilityBubbleUI : UI_Base
{
    enum Buttons
    {
        PurchaseBtn
    }

    enum Images
    {

    }

    enum Texts
    {
        DescText,
        PriceText
    }

    bool init;

    [SerializeField] private Sprite[] sprites;

    private int abilityIndex = -1;
    private bool isAdditional = false;

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

        this.SetListener(GameObserverType.Game.OnAbilityChanged, () =>
        {
            UpdateUI();
        });
    }

    public void Setting(int abilityIndex, bool isAdditional = false)
    {
        this.abilityIndex = abilityIndex;
        this.isAdditional = isAdditional;

        Ability ability;
        if (isAdditional)
        {
            ability = Managers.Ability.GetAdditionalAbility(abilityIndex);
        }
        else
        {
            ability = Managers.Ability.GetAbility(abilityIndex);
        }

        GetTextMesh(Texts.DescText).text = ability.GetAbilityDesc();
        GetTextMesh(Texts.PriceText).text = ability.cost.ToString();

        // if (isAdditional ? Managers.Ability.HasAdditionalAbility(abilityIndex) : Managers.Ability.HasAbility(abilityIndex))
        // {
        //     GetButton(Buttons.PurchaseBtn).gameObject.SetActive(false);
        // }
        // else
        // {
        if (isAdditional ? !Managers.Ability.CanPurchaseAdditionalAbility(abilityIndex) : !Managers.Ability.CanPurchaseAbility(abilityIndex))
            GetButton(Buttons.PurchaseBtn).image.sprite = sprites[1];
        else
        {
            GetButton(Buttons.PurchaseBtn).AddButtonEvent(() =>
            {
                if (isAdditional)
                {
                    Managers.Ability.PurchaseAdditionalAbility(abilityIndex);
                }
                else
                {
                    Managers.Ability.PurchaseAbility(abilityIndex);
                }
            });

            GetButton(Buttons.PurchaseBtn).image.sprite = sprites[0];
        }
        // }
    }

    public void UpdateUI()
    {
        Setting(abilityIndex, isAdditional);
    }
}
