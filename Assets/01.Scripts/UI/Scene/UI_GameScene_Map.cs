using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;

public class UI_GameScene_Map : UI_Scene
{
    enum Buttons
    {
        PauseBtn,
        BoostFireBtn,
        BoostShieldBtn,
        BoostHammerBtn,
        RepairBtn
    }

    enum Texts
    {
        GoldText,
        EnemyCount,
        TicketCount
    }

    private Transform playerLayout;

    bool _init = false;


    public override void Init()
    {
        base.Init();

        if (!_init)
        {
            FirstSetting();
        }

        this.SetListener(GameObserverType.Game.OnChangeCoinCount, () =>
        {
            GetTextMesh(Texts.GoldText).text = Managers.Game.coin.ToString();
        });

        this.SetListener(GameObserverType.Game.OnChangeEnergyCount, () =>
        {
            GetTextMesh(Texts.EnemyCount).text = Managers.Game.energy.ToString();
        });

        this.SetListener(GameObserverType.Game.OnChangeTicketCount, () =>
        {
            GetTextMesh(Texts.TicketCount).text = Managers.Game.ticket.ToString();
        });

        GetTextMesh(Texts.GoldText).text = Managers.Game.coin.ToString();
        GetTextMesh(Texts.EnemyCount).text = Managers.Game.energy.ToString();
        GetTextMesh(Texts.TicketCount).text = Managers.Game.ticket.ToString();
    }

    public void FirstSetting()
    {
        _init = true;

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));

        playerLayout = gameObject.FindRecursive("PlayerLayout").transform;

        GetButton(Buttons.PauseBtn).onClick.AddListener(() =>
        {
            Managers.UI.ShowPopupUI<Exit_Popup>();
        });
    }

    public void ClearCharactorIcons()
    {
        foreach (Transform child in playerLayout)
        {
            Destroy(child.gameObject);
        }
    }

    public void SetPlayerIcon(Define.CharactorType type)
    {
        var icon = Managers.Resource.Instantiate("CharactorIcon", playerLayout);

        icon.GetComponent<Image>().sprite = Managers.Resource.GetCharactorIcons((int)type + 1);
    }

    public void SetCharactorIcon(Define.CharactorType type)
    {
        var icon = Managers.Resource.Instantiate("CharactorIcon", playerLayout);
        icon.GetComponent<Image>().sprite = Managers.Resource.GetCharactorIcons((int)type + 1);

        icon.gameObject.FindRecursive("Arrrow").SetActive(false);
    }

    public override void Show()
    {
        gameObject.SetActive(true);
    }

    public override void Hide()
    {
        gameObject.SetActive(false);
    }
}
