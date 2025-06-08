using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
using DG.Tweening;

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

        icon.FindRecursive("Icon").GetComponent<Image>().sprite = Managers.Resource.GetCharactorIcons((int)type + 1);
    }

    public void SetCharactorIcon(Define.CharactorType type)
    {
        var icon = Managers.Resource.Instantiate("CharactorIcon", playerLayout);
        icon.FindRecursive("Icon").GetComponent<Image>().sprite = Managers.Resource.GetCharactorIcons((int)type + 1);
        icon.gameObject.FindRecursive("Arrrow").SetActive(false);
    }

    public void AttackedAnimation(int index)
    {
        if (!Managers.Game.charactors[index].die)
        {
            RectTransform iconRect = playerLayout.GetChild(index).gameObject.FindRecursive("Icon").transform as RectTransform;
            if (iconRect != null)
            {
                iconRect.DOShakeAnchorPos(0.5f, 10f, 30, 90, false, true);
            }
            playerLayout.GetChild(index).gameObject.FindRecursive("Icon").GetComponent<Image>().DOColor(Color.red, 0.5f).OnComplete(() =>
            {
                playerLayout.GetChild(index).gameObject.FindRecursive("Icon").GetComponent<Image>().DOColor(Color.white, 0.5f);
            });
        }
        else
            playerLayout.GetChild(index).gameObject.FindRecursive("Icon").GetComponent<Image>().color = new Color32(30, 30, 30, 255);

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
