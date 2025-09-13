using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public class BoostShop_Popup : UI_Popup
{
    enum Buttons
    {
    }

    enum Images
    {
        TouchGuard
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

        layout = gameObject.FindRecursive("Layout").transform;

        foreach (var data in Managers.Resource.LoadAll<BoostData>("BoostData/").OrderBy(n => n.type))
        {
            var boostBox = Managers.Resource.Instantiate("BoostBoxUI", layout);
            boostBox.GetComponent<BoostBoxUI>().Init();
            boostBox.GetComponent<BoostBoxUI>().Setting(() =>
            {
                Managers.LocalData.AddBoostItem(data.type, 1);
                Managers.LocalData.PlayerGemCount -= data.price;

                Managers.Audio.PlaySound("snd_get_item");
            }, data);
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
