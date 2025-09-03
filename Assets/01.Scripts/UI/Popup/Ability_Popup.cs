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

    private AbilityBubbleUI abilityBubbleUI;


    public override void Init()
    {
        base.Init();
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        layout = gameObject.FindRecursive("Content").transform;

        Define.Tier currentTier = Define.Tier.Iron4;

        bool first = false;

        var ability = Managers.Resource.Load<AbilityData>("AbilityData/AbilityData");

        List<int> usedAdditionalAbilities = new List<int>();

        for (int i = 0; i < ability.abilities.Count; i++)
        {
            var data = ability.abilities[i];
            if (data.needTier != currentTier)
            {
                first = true;
                currentTier = data.needTier;
            }
            else
            {
                first = false;
            }

            var additionalAbilityIndex = ability.GetAdditionalAbilityIndex(data.needTier);

            if (usedAdditionalAbilities.Contains(additionalAbilityIndex))
            {
                additionalAbilityIndex = -1;
            }
            else if (additionalAbilityIndex != -1)
            {
                usedAdditionalAbilities.Add(additionalAbilityIndex);
            }

            var boostBox = Managers.Resource.Instantiate("AbilitySlotUI", layout);
            boostBox.transform.SetAsFirstSibling();
            boostBox.GetComponent<AbilitySlotUI>().Init();
            bool isLast = i == ability.abilities.Count - 1;
            boostBox.GetComponent<AbilitySlotUI>().Setting(i, additionalAbilityIndex, first, isLast);
        }

        OpenAnimation();

        GetComponentInChildren<ScrollRect>().onValueChanged.AddListener((value) =>
        {
            if (abilityBubbleUI != null)
            {
                Destroy(abilityBubbleUI.gameObject);
            }
        });
    }

    public void GenerateAbilityBubble(Transform trans, int abilityIndex = -1, int additionalAbilityIndex = -1)
    {
        if (abilityIndex >= 0)
        {
            Generate(abilityIndex, false);
        }

        if (additionalAbilityIndex >= 0)
        {
            Generate(additionalAbilityIndex, true);
        }

        void Generate(int index, bool isAdditional = false)
        {
            if (abilityBubbleUI != null)
            {
                Destroy(abilityBubbleUI.gameObject);
            }

            abilityBubbleUI = Managers.Resource.Instantiate("AbilityBubbleUI", trans).GetComponent<AbilityBubbleUI>();
            abilityBubbleUI.Init();
            abilityBubbleUI.Setting(index, isAdditional);

            abilityBubbleUI.gameObject.FindRecursive("Panel" + (isAdditional ? "2" : "")).SetActive(true);

            abilityBubbleUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(isAdditional ? -130 : 100, 0);
        }
    }

    public override void Reset()
    {

    }

    public void Exit()
    {
        if (abilityBubbleUI != null)
        {
            Destroy(abilityBubbleUI.gameObject);
        }

        ClosePopupUI();
    }
}
