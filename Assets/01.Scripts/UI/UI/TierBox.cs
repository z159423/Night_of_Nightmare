using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TierBox : UI_Base
{
    enum Images
    {
        Icon,
        Frame,
        BackYellow,
        Line
    }

    enum Texts
    {
        NameText,
        ScoreText
    }

    bool _init = false;

    public override void Init()
    {
        if (!_init)
        {
            FirstSetting();
        }
    }

    public void FirstSetting()
    {
        _init = true;

        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    public TierBox Setting(Define.Tier tier)
    {
        GetImage(Images.Icon).sprite = Managers.Resource.Load<Sprite>($"Tier/{tier.ToString()}");
        GetImage(Images.Icon).SetNativeSize();
        GetTextMesh(Texts.NameText).text = tier.ToString();
        GetTextMesh(Texts.ScoreText).text = Define.TierToScore[tier].ToString();

        GetTextMesh(Texts.NameText).color = Define.TierColor[tier];
        GetTextMesh(Texts.ScoreText).color = Define.TierColor[tier];

        if (Managers.LocalData.PlayerRankingPoint < Define.TierToScore[tier])
        {
            GetImage(Images.Icon).color = new Color32(90, 90, 90, 255);
            GetImage(Images.Frame).color = new Color32(90, 90, 90, 255);
        }

        int nextTierScore = 0;
        if (Define.TierToScore.ContainsKey(tier + 1))
            nextTierScore = Define.TierToScore[tier + 1];
        else
        {
            nextTierScore = int.MaxValue;
            GetImage(Images.Line).gameObject.SetActive(false);
        }

        // 점수가 해당 Tier에 포함되는 경우만 SetActive true
        bool isInTier = Managers.LocalData.PlayerRankingPoint < nextTierScore &&
                        Managers.LocalData.PlayerRankingPoint >= Define.TierToScore[tier];

        GetImage(Images.BackYellow).gameObject.SetActive(isInTier);

        if (isInTier || (Managers.LocalData.PlayerRankingPoint < 0 && tier == Define.Tier.Iron4)) return this;
        else
            return null;
    }
}
