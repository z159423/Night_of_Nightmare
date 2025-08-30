using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionRewardSlot : UI_Base
{
    enum Buttons
    {
        RewardBtn
    }

    enum Images
    {
        Reddot,
        CheckImage,
        Black
    }

    enum Texts
    {
        NeedTimeText
    }

    bool isInit;

    public int rewardMin = 0;

    public override void Init()
    {
        FirstSetting();

        this.SetListener(GameObserverType.Game.Timer, UpdateUI);

        GetTextMesh(Texts.NeedTimeText).text = rewardMin + " Min";
        UpdateUI();
    }

    public void FirstSetting()
    {
        if (isInit) return;

        isInit = true;

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        GetButton(Buttons.RewardBtn).AddButtonEvent(() =>
        {
            // 출석체크 팝업
            if (Managers.SessionReward.TryClaim(rewardMin, out var items))
            {
                // 출석체크 성공
                foreach (var item in items)
                {
                    Managers.UI.GenerateUIParticle(transform, item.Item2, item.Item1);
                }
            }

            UpdateUI();
        }, false);

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (Managers.SessionReward.IsAlreadyClaimed(rewardMin))
        {
            GetImage(Images.Black).gameObject.SetActive(true);
            GetImage(Images.CheckImage).gameObject.SetActive(true);
            GetImage(Images.Reddot).gameObject.SetActive(false);
            return;
        }

        if (Managers.SessionReward.CanClaim(rewardMin))
        {
            GetImage(Images.Black).gameObject.SetActive(false);
            GetImage(Images.CheckImage).gameObject.SetActive(false);
            GetImage(Images.Reddot).gameObject.SetActive(true);
        }
    }
}
