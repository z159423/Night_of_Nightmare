using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using DG.Tweening;

public class MatchResult_Popup : UI_Popup
{
    enum Buttons
    {
        ClaimBtn,
        ContinueBtn
    }

    enum Images
    {
        RankImage,
        Shine
    }

    enum Texts
    {
        ResultTitle,
        GemCountText,
        RankingPointDiffText,
        GemText,
        TicketCount,
        RankingTierText,
        RankingPointText

    }

    [SerializeField] private Color[] colors;


    public override void Init()
    {
        base.Init();
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        GetButton(Buttons.ContinueBtn).AddButtonEvent(() =>
        {
            gameObject.FindRecursive("Ranking").gameObject.SetActive(false);
            gameObject.FindRecursive("Result").gameObject.SetActive(true);
        });

        GetButton(Buttons.ClaimBtn).AddButtonEvent(() =>
        {
            Exit();
        });
    }

    public void Setting(bool isWin, int point)
    {
        if (isWin)
        {
            GetImage(Images.Shine).gameObject.SetActive(true);
            GetImage(Images.RankImage).transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).OnStart(() =>
            {
                GetImage(Images.RankImage).transform.localScale = Vector3.one;
            });

            GetTextMesh(Texts.RankingPointDiffText).color = colors[0];
        }
        else
            GetTextMesh(Texts.RankingPointDiffText).color = colors[1];

            GetTextMesh(Texts.RankingPointDiffText).text = point.ToString();

        GetTextMesh(Texts.RankingPointText).text = (Managers.LocalData.PlayerRankingPoint - point).ToString();
        GetTextMesh(Texts.RankingTierText).text = Define.TierToScore.FirstOrDefault(n => n.Value == Managers.LocalData.PlayerRankingPoint).Key.ToString();

        TextUtils.UINumberTween(GetTextMesh(Texts.RankingPointText), (Managers.LocalData.PlayerRankingPoint - point), Managers.LocalData.PlayerRankingPoint, 3);

        IEnumerator wait()
        {
            yield return new WaitForSeconds(5f);
            GetButton(Buttons.ContinueBtn).gameObject.SetActive(true);
        }

        StartCoroutine(wait());
    }

    public override void Reset()
    {

    }

    private void Exit()
    {
        ClosePopupUI();
    }
}
