using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
using System.Net.NetworkInformation;

public class CharactorSelect_Popup : UI_Popup
{
    enum Buttons
    {
        SelectBtn,
        ExitBtn
    }

    enum Images
    {
        TouchGuard
    }

    enum Texts
    {
        CharactorName,
        CharactorDesc,
        GemCountText,
        SelectText
    }

    public readonly Define.CharactorType[] charactorTypes = new Define.CharactorType[]
    {
        Define.CharactorType.Farmer,
        Define.CharactorType.ReapireMan,
        Define.CharactorType.LampGirl,
        Define.CharactorType.Miner,
        Define.CharactorType.Scientist,
        Define.CharactorType.Chef,
    };

    private Transform layoutParent;
    private List<CharactorSelectIcon> icons = new List<CharactorSelectIcon>();
    public CharactorSelectIcon selectedIcon;
    public static Define.CharactorType selectedType;

    public Action onExit = null;

    [SerializeField] Sprite[] btnSprites;

    public override void Init()
    {
        base.Init();

        OpenAnimation();

        selectedType = (Define.CharactorType)Managers.LocalData.SelectedCharactor;

        selectedIcon = icons.Find(n => n.type == selectedType);
        selectedIcon?.OnSelect();

        GameObserver.Call(GameObserverType.Game.OnChangeSelectedCharactorType);

        this.SetListener(GameObserverType.Game.OnChangeGemCount, () =>
        {
            UpdateUI();
        });

        UpdateUI();
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
        layoutParent = gameObject.FindRecursive("CharactorIconLayouts").transform;

        GetButton(Buttons.ExitBtn).AddButtonEvent(Exit);

        var charactorDataList = Managers.Resource.LoadAll<CharactorData>("CharactorData/")
            .OrderBy(data => Array.IndexOf(charactorTypes, data.charactorType))
            .ToList();

        foreach (var type in charactorDataList)
        {
            var btn = Managers.Resource.Instantiate("CharactorSelectIcon", layoutParent);

            if (btn.TryGetComponent<CharactorSelectIcon>(out var charactorSelectIcon))
            {
                charactorSelectIcon.Setting(type.charactorType, () =>
                {
                    SelectCharactorIcon(type.charactorType);
                });

                icons.Add(charactorSelectIcon);
            }
        }
    }

    public void SelectCharactorIcon(Define.CharactorType charactorType)
    {
        if (selectedIcon != null)
            selectedIcon.UnSelect();

        selectedType = charactorType;

        UpdateUI();

        var icon = icons.Find(n => n.type == selectedType);
        selectedIcon = icon;
        icon.OnSelect();

        GameObserver.Call(GameObserverType.Game.OnChangeSelectedCharactorType);
    }

    public void UpdateUI()
    {
        var find = Managers.Resource.LoadAll<CharactorData>("CharactorData/").FirstOrDefault(n => n.charactorType == selectedType);

        GetTextMesh(Texts.CharactorName).text = Managers.Localize.GetText(find.nameKey) + ":";
        GetTextMesh(Texts.CharactorDesc).text = Managers.Localize.GetText(find.descriptionKey);
        GetButton(Buttons.SelectBtn).gameObject.FindRecursive("Gem").gameObject.SetActive(false);

        if (selectedType == (Define.CharactorType)Managers.LocalData.SelectedCharactor)
        {
            GetButton(Buttons.SelectBtn).GetComponent<Image>().sprite = btnSprites[1];

            GetTextMesh(Texts.SelectText).gameObject.SetActive(true);
            GetTextMesh(Texts.SelectText).text = Managers.Localize.GetText("global.str_choiced");
        }
        else
        {
            if (Managers.LocalData.HasCharactor(selectedType))
            {
                GetButton(Buttons.SelectBtn).GetComponent<Image>().sprite = btnSprites[0];
                GetTextMesh(Texts.SelectText).gameObject.SetActive(true);
                GetTextMesh(Texts.SelectText).text = Managers.Localize.GetText("global.str_choice");

                GetButton(Buttons.SelectBtn).AddButtonEvent(() =>
                {
                    Managers.Game.ChangePlayerCharactor(selectedType);

                    UpdateUI();
                });
            }
            else
            {
                if (selectedType != Define.CharactorType.Scientist)
                    GetTextMesh(Texts.CharactorDesc).text += "<br><size=80%><color=#FFD08D>" + Managers.Localize.GetText("achive_" + selectedType) + "</color></size>";

                var charactorData = Managers.Resource.LoadAll<CharactorData>($"CharactorData/").FirstOrDefault(n => n.charactorType == selectedType);
                if (charactorData.purchaseType == Define.CharactorPurchaseType.Iap)
                {
                    string productId = "";
                    if (charactorData.charactorType == Define.CharactorType.LampGirl)
                        productId = "character_lampgirl";
                    else if (charactorData.charactorType == Define.CharactorType.Scientist)
                        productId = "character_scientist";

                    GetTextMesh(Texts.SelectText).gameObject.SetActive(true);
                    GetTextMesh(Texts.SelectText).text = Managers.IAP.GetLocalizedPrice(productId);

                    GetButton(Buttons.SelectBtn).GetComponent<Image>().sprite = btnSprites[0];

                    GetButton(Buttons.SelectBtn).AddButtonEvent(() =>
                    {
                        //TODO: IAP 구매 로직 추가

                        Managers.IAP.PurchaseStart(productId, () =>
                        {
                            Managers.LocalData.SetCharactorOwned(charactorData.charactorType, true);
                            Managers.Game.ChangePlayerCharactor(selectedType);
                            UpdateUI();

                            Managers.Audio.PlaySound("snd_get_item");
                        });
                    });
                }
                else if (charactorData.purchaseType == Define.CharactorPurchaseType.Gem)
                {
                    GetButton(Buttons.SelectBtn).gameObject.FindRecursive("Gem").gameObject.SetActive(true);
                    GetTextMesh(Texts.SelectText).gameObject.SetActive(false);
                    GetTextMesh(Texts.GemCountText).text = charactorData.requireGem.ToString();

                    if (Managers.LocalData.PlayerGemCount >= charactorData.requireGem)
                    {
                        GetButton(Buttons.SelectBtn).GetComponent<Image>().sprite = btnSprites[0];

                        GetButton(Buttons.SelectBtn).AddButtonEvent(() =>
                        {
                            Managers.LocalData.PlayerGemCount -= charactorData.requireGem;
                            Managers.LocalData.SetCharactorOwned(charactorData.charactorType, true);
                            Managers.Game.ChangePlayerCharactor(selectedType);

                            UpdateUI();

                            Managers.Audio.PlaySound("snd_get_item");
                        });
                    }
                    else
                    {
                        GetButton(Buttons.SelectBtn).GetComponent<Image>().sprite = btnSprites[1];
                        GetButton(Buttons.SelectBtn).onClick.RemoveAllListeners();
                    }
                }
            }
        }
    }

    public override void Reset()
    {

    }

    private void Exit()
    {
        this.RemoveListener(GameObserverType.Game.OnChangeGemCount);

        selectedType = (Define.CharactorType)Managers.LocalData.SelectedCharactor;
        GameObserver.Call(GameObserverType.Game.OnChangeSelectedCharactorType);

        foreach (var charactor in Managers.Game.homeCharactors)
        {
            charactor.OnUnselect();
        }

        onExit?.Invoke();
        ClosePopupUI();
    }
}
