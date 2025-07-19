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
        ContinueBtn,
        NoThanksBtn
    }

    enum Images
    {
        RankImage,
        Shine,
        ResultShine,
        RVIcon,
        GemImage
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
        ContinueText,
        NoThanksGemCountText,
        ClaimText,
        Title
    }

    [SerializeField] private Color[] colors;

    private int GemCount = 0;

    private bool iswin = false;


    public override void Init()
    {
        base.Init();

        GetImage(Images.RankImage).sprite = Managers.Resource.Load<Sprite>($"Tier/{Define.GetPlayerCurrentTier().ToString()}");
        GetImage(Images.RankImage).SetNativeSize();

        GetTextMesh(Texts.ContinueText).text = Managers.Localize.GetText("global.str_continue");

        OpenAnimation();

        GetButton(Buttons.ContinueBtn).gameObject.SetActive(false);
        GetButton(Buttons.NoThanksBtn).gameObject.SetActive(false);

        GetTextMesh(Texts.GemText).text = Managers.LocalData.PlayerGemCount.ToString();
        GetTextMesh(Texts.TicketCount).text = Managers.LocalData.PlayerRvTicketCount.ToString();
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        GetButton(Buttons.ContinueBtn).AddButtonEvent(() =>
        {
            ActiveResult();
        });

        GetButton(Buttons.ClaimBtn).AddButtonEvent(() =>
        {
            //광고 보고 잼 4배 추가로 획득
            Managers.Ad.ShowRewardAd(() =>
            {
                if (iswin)
                {
                    //이긴 경우 이 버튼 클릭하면 RV 광고 재생
                    Managers.LocalData.PlayerGemCount += GemCount * 4;

                    Exit();
                }
                else
                {
                    Managers.LocalData.PlayerGemCount += GemCount;
                    Exit();
                }
            });
        });

        GetButton(Buttons.NoThanksBtn).AddButtonEvent(() =>
        {
            //광고 안보고 그냥 잼 획득
            Managers.LocalData.PlayerGemCount += GemCount;
            Exit();
        });
    }

    void ActiveResult()
    {
        gameObject.FindRecursive("Ranking").gameObject.SetActive(false);
        gameObject.FindRecursive("Result").gameObject.SetActive(true);

        gameObject.FindRecursive("Result").transform.localScale = Vector3.zero;
        gameObject.FindRecursive("Result").transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        StartCoroutine(wait());

        IEnumerator wait()
        {
            GetButton(Buttons.ClaimBtn).transform.localScale = Vector3.zero;

            yield return new WaitForSeconds(0.5f);

            GetButton(Buttons.ClaimBtn).transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

            if (iswin)
            {
                yield return new WaitForSeconds(1.5f);

                GetButton(Buttons.NoThanksBtn).GetComponent<CanvasGroup>().alpha = 0f;
                GetButton(Buttons.NoThanksBtn).gameObject.SetActive(true);
                GetButton(Buttons.NoThanksBtn).GetComponent<CanvasGroup>().DOFade(1f, 2f);
            }
        }
    }

    public void Setting(bool isWin, int point, bool isChallengeMode = false)
    {
        iswin = isWin;
        GemCount = Define.GetResultGemCount(isWin, isChallengeMode);

        GetTextMesh(Texts.Title).text = isChallengeMode ? Managers.Localize.GetText("global.str_challenge_mode") : Managers.Localize.GetText("global.str_rank_mode");

        // 공통 텍스트
        GetTextMesh(Texts.RankingTierText).text = Define.GetPlayerCurrentTier().ToString();
        GetTextMesh(Texts.RankingPointDiffText).text = (isWin ? "+" : "-") + point.ToString();

        // 공통 GemCountText, NoThanksGemCountText, RVIcon, ResultTitle, ClaimText
        int claimGem = isWin ? GemCount * 4 : GemCount;
        GetTextMesh(Texts.GemCountText).text = $"x {claimGem}";
        GetTextMesh(Texts.NoThanksGemCountText).text = $"x {GemCount}";
        GetImage(Images.RVIcon).gameObject.SetActive(isWin);
        GetTextMesh(Texts.ResultTitle).text = Managers.Localize.GetText(isWin ? "global.str_win_reward" : "global.str_lose");
        GetTextMesh(Texts.ClaimText).text = isWin
            ? Managers.Localize.GetDynamicText("global.str_claim_reward", 4.ToString())
            : Managers.Localize.GetText("global.str_claim");

        if (isChallengeMode)
        {
            ActiveResult();
        }
        else
        {
            // 승패별 추가 UI
            GetImage(Images.Shine).gameObject.SetActive(isWin);
            GetImage(Images.ResultShine).gameObject.SetActive(isWin);
            GetTextMesh(Texts.RankingPointDiffText).color = colors[isWin ? 0 : 1];

            // 랭킹 포인트 Tween
            int from = isWin ? Managers.LocalData.PlayerRankingPoint - point : Managers.LocalData.PlayerRankingPoint + point;
            TextUtils.UINumberTween(
                GetTextMesh(Texts.RankingPointText),
                from,
                Managers.LocalData.PlayerRankingPoint,
                3
            );

            StartCoroutine(ShowContinueButton());
        }

        IEnumerator ShowContinueButton()
        {
            yield return new WaitForSeconds(5f);
            var continueBtn = GetButton(Buttons.ContinueBtn);
            continueBtn.transform.localScale = Vector3.zero;
            continueBtn.gameObject.SetActive(true);
            continueBtn.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
    }

    public override void Reset()
    {

    }

    private void Exit()
    {
        ClosePopupUI();
    }
}
