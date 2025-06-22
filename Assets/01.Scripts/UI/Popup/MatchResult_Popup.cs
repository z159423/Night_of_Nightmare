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
        RankingPointText,
        ContinueText
    }

    [SerializeField] private Color[] colors;


    public override void Init()
    {
        base.Init();

        GetImage(Images.RankImage).sprite = Managers.Resource.Load<Sprite>($"Tier/{Define.GetPlayerCurrentTier().ToString()}");
        GetImage(Images.RankImage).SetNativeSize();

        GetTextMesh(Texts.ContinueText).text = Managers.Localize.GetText("global.str_continue");

        OpenAnimation();
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

            GetTextMesh(Texts.RankingPointDiffText).color = colors[0];
        }
        else
            GetTextMesh(Texts.RankingPointDiffText).color = colors[1];

        GetTextMesh(Texts.RankingPointDiffText).text = (isWin ? "+" : "-") + point.ToString();

        GetTextMesh(Texts.RankingPointText).text = (Managers.LocalData.PlayerRankingPoint - point).ToString();
        GetTextMesh(Texts.RankingTierText).text = Define.GetPlayerCurrentTier().ToString();

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
