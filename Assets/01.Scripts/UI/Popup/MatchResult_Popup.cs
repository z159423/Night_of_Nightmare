using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class MatchResult_Popup : UI_Popup
{
    enum Buttons
    {
        ClaimBtn,
        ContinueBtn
    }

    enum Images
    {
        RankImage
    }

    enum Texts
    {
        ResultTitle,
        GemCountText,
        RankingPointDiffText,
        RankingPointText,
        GemText,
        TicketCount
    }


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

    public void Setting()
    {
        IEnumerator wait()
        {
            yield return new WaitForSeconds(4f);
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
