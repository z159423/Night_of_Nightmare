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
    }

    public void FirstSetting()
    {
        _init = true;

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));

        playerLayout = gameObject.FindRecursive("PlayerLayout").transform;

        GetButton(Buttons.PauseBtn).onClick.AddListener(() =>
        {

        });
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
