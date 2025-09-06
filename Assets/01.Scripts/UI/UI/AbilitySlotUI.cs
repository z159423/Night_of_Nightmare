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


        //이건 구매 가능한 티어일때 색 넣는거
        // if ((int)ability.needTier <= Managers.LocalData.PlayerHighestTier)
        //     GetButton(Buttons.AbilityBtn).image.color = Color.white;
        // else
        //     GetButton(Buttons.AbilityBtn).image.color = new Color32(50, 50, 50, 255);

        //이건 아직 구매 안한 상태면 그냥 색 다 빼는거
        if (Managers.Ability.HasAbility(abilityIndex))
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

            // if ((int)additionalAbility.needTier <= Managers.LocalData.PlayerHighestTier)
            //     GetButton(Buttons.AdditionalBtn).image.color = Color.white;
            // else
            //     GetButton(Buttons.AdditionalBtn).image.color = new Color32(50, 50, 50, 255);

            if (Managers.Ability.HasAdditionalAbility(additionalAbilityIndex))
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

        UpdateUI();
    }

    void UpdateUI()
    {
        var ability = Managers.Ability.GetAbility(abilityIndex);
        var additionalAbility = Managers.Ability.GetAdditionalAbility(additionalAbilityIndex);

        // 기본 능력 빨간점 업데이트
        GetImage(Images.AbilityReddot).gameObject.SetActive(Managers.Ability.CanPurchaseAbility(abilityIndex));

        // 추가 능력 빨간점 업데이트
        if (additionalAbility != null)
        {
            GetImage(Images.AdditionalReddot).gameObject.SetActive(Managers.Ability.CanPurchaseAdditionalAbility(additionalAbilityIndex));
        }

        // === 라인 색상 업데이트 ===

        // Line1 (중간 다리와 이어지는 라인) - 기본 능력 상태에 따라
        bool currentAbilityPurchased = Managers.Ability.HasAbility(abilityIndex);
        bool currentAbilityCanPurchase = Managers.Ability.CanPurchaseAbility(abilityIndex);

        if (currentAbilityPurchased)
        {
            GetImage(Images.Line1).color = Color.white;
        }
        else
        {
            GetImage(Images.Line1).color = Color.black;
        }

        if (Managers.Ability.HasAbility(abilityIndex))
            GetButton(Buttons.AbilityBtn).image.color = Color.white;
        else
            GetButton(Buttons.AbilityBtn).image.color = new Color32(50, 50, 50, 255);

        // Line2 (추가 능력과 이어지는 라인) - 추가 능력이 있을 때만
        if (additionalAbility != null)
        {
            bool additionalAbilityCanPurchase = Managers.Ability.CanPurchaseAdditionalAbility(additionalAbilityIndex);

            if (additionalAbilityCanPurchase || Managers.Ability.HasAdditionalAbility(additionalAbilityIndex))
            {
                GetImage(Images.Line2).color = Color.white;
            }
            else
            {
                GetImage(Images.Line2).color = Color.black;
            }

            if (Managers.Ability.HasAdditionalAbility(additionalAbilityIndex))
                GetButton(Buttons.AdditionalBtn).image.color = Color.white;
            else
                GetButton(Buttons.AdditionalBtn).image.color = new Color32(50, 50, 50, 255);
        }

        // Line3 (다음 기본 능력과 이어지는 라인) - 다음 능력의 상태에 따라
        int nextAbilityIndex = abilityIndex + 1;
        if (nextAbilityIndex < Managers.Ability.GetAbilityCount()) // GetAbilityCount() 메서드 필요
        {
            bool nextAbilityPurchased = Managers.Ability.HasAbility(nextAbilityIndex);
            bool nextAbilityCanPurchase = Managers.Ability.CanPurchaseAbility(nextAbilityIndex);

            if (nextAbilityPurchased || nextAbilityCanPurchase)
            {
                GetImage(Images.Line3).color = Color.white;
            }
            else
            {
                GetImage(Images.Line3).color = Color.black;
            }
        }
        else
        {
            // 마지막 능력인 경우 Line3는 숨김
            GetImage(Images.Line3).gameObject.SetActive(false);
        }
    }
}
