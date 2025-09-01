using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public class Ability_Popup : UI_Popup
{
    enum Buttons
    {

    }

    enum Images
    {

    }

    enum Texts
    {

    }

    private Transform layout;


    public override void Init()
    {
        base.Init();
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        layout = gameObject.FindRecursive("Content").transform;

        Define.Tier currentTier = Define.Tier.Iron4;

        bool first = false;

        var ability = Managers.Resource.Load<AbilityData>("AbilityData/AbilityData");

        foreach (var data in ability.abilities)
        {
            Ability additionalAbility = null;
            if (data.needTier != currentTier)
            {
                first = true;
                currentTier = data.needTier;
                additionalAbility = ability.GetAdditionalAbility(data.needTier);
            }
            else
            {
                first = false;
            }

            var boostBox = Managers.Resource.Instantiate("AbilitySlotUI", layout);
            boostBox.transform.SetAsFirstSibling();
            boostBox.GetComponent<AbilitySlotUI>().Init();
            bool isLast = data == ability.abilities.Last();
            boostBox.GetComponent<AbilitySlotUI>().Setting(data, additionalAbility, first, isLast);
        }

        OpenAnimation();
    }

    public override void Reset()
    {

    }

    public void Exit()
    {
        ClosePopupUI();
    }
}
