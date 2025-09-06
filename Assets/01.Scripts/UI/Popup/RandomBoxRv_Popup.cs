using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RandomBoxRv_Popup : UI_Popup
{
    enum Buttons
    {
        BG,
        RvBtn
    }

    enum Images
    {
        TouchGuard
    }

    enum Texts
    {
        CanShowCountText
    }

    public Action onShowRv;

    public override void Init()
    {
        base.Init();

        OpenAnimation(() =>
        {
            GetImage(Images.TouchGuard).gameObject.SetActive(false);
        });
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        GetButton(Buttons.BG).AddButtonEvent(() =>
        {
            GetImage(Images.TouchGuard).gameObject.SetActive(true);
            ClosePop(gameObject.FindRecursive("Panel").transform);
        }, false);

        GetButton(Buttons.RvBtn).AddButtonEvent(() =>
        {
            onShowRv?.Invoke();

            Managers.LocalData.PlayerGemCount += 50;
            Managers.UI.GenerateUIParticle(Managers.UI._currentScene.transform, uiParticleMarkerType.GemIcon, Define.ItemType.Gem);

            switch (UnityEngine.Random.Range(0, 4))
            {
                case 0:
                    Managers.LocalData.AddBoostItem(Define.BoostType.Lamp, 1);
                    Managers.UI.GenerateUIParticle(Managers.UI._currentScene.transform, uiParticleMarkerType.BoostBtn, Define.ItemType.Boost_Ramp);
                    break;
                case 1:
                    Managers.LocalData.AddBoostItem(Define.BoostType.HammerThrow, 1);
                    Managers.UI.GenerateUIParticle(Managers.UI._currentScene.transform, uiParticleMarkerType.BoostBtn, Define.ItemType.Boost_Hammer);
                    break;
                case 2:
                    Managers.LocalData.AddBoostItem(Define.BoostType.HolyProtection, 1);
                    Managers.UI.GenerateUIParticle(Managers.UI._currentScene.transform, uiParticleMarkerType.BoostBtn, Define.ItemType.Boost_Shield);
                    break;
                case 3:
                    Managers.LocalData.AddBoostItem(Define.BoostType.Overheat, 1);
                    Managers.UI.GenerateUIParticle(Managers.UI._currentScene.transform, uiParticleMarkerType.BoostBtn, Define.ItemType.Boost_Fire);
                    break;
            }

            if (Managers.LocalData.RandomBoxRvShowDay != DateTime.Now.Day)
                Managers.LocalData.RandomBoxRvCount = 1;
            else
                Managers.LocalData.RandomBoxRvCount++;

            Managers.LocalData.RandomBoxRvShowDay = DateTime.Now.Day;

            GameObserver.Call(GameObserverType.Game.OnShowRandomReward);

            if (Managers.LocalData.RandomBoxRvCount == 5)
                ClosePop(gameObject.FindRecursive("Panel").transform);

            UpdateUI();
        }, false);

        UpdateUI();
    }

    void UpdateUI()
    {
        GetTextMesh(Texts.CanShowCountText).text = Managers.Localize.GetDynamicText("enable_show_count", (5 - Managers.LocalData.RandomBoxRvCount).ToString());
    }
}
