using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

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
        CharactorDesc
    }

    private Transform charctorIconLayouts;

    public static Define.CharactorType selectedCharactorType;

    public Action onExit = null;

    public override void Init()
    {
        base.Init();

        selectedCharactorType = Managers.Game.currentPlayerCharacterType;
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
        charctorIconLayouts = gameObject.FindRecursive("CharactorIconLayouts").transform;

        GetButton(Buttons.ExitBtn).AddButtonEvent(Exit);

        var charactorDataList = Managers.Resource.LoadAll<CharactorData>("CharactorData/")
            .OrderBy(data => (int)data.charactorType)
            .ToList();

        foreach (var type in charactorDataList)
        {
            var btn = Managers.Resource.Instantiate("CharactorSelectIcon", charctorIconLayouts);

            btn.GetComponent<Image>().sprite = Managers.Resource.GetCharactorIcons((int)type.charactorType + 1);

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                selectedCharactorType = type.charactorType;

                UpdateUI();

                GameObserver.Call(GameObserverType.Game.OnChangeSelectedCharactorType);
            });
        }
    }

    public void UpdateUI()
    {
        var find = Managers.Resource.LoadAll<CharactorData>("CharactorData/").FirstOrDefault(n => n.charactorType == selectedCharactorType);

        GetTextMesh(Texts.CharactorName).text = Managers.Localize.GetText(find.nameKey) + ":";
        GetTextMesh(Texts.CharactorDesc).text = Managers.Localize.GetText(find.descriptionKey);

        foreach (Transform child in charctorIconLayouts)
        {

        }
    }

    public override void Reset()
    {

    }

    private void Exit()
    {
        onExit?.Invoke();
        ClosePopupUI();
    }
}
