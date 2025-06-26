using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChallengeMode_Popup : UI_Popup
{
    enum Buttons
    {
        ExitBtn,
    }

    enum Images
    {
        TouchGuard
    }

    enum Texts
    {

    }

    VerticalLayoutGroup layout;

    public override void Init()
    {
        base.Init();

        Setting();
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        layout = GetComponentInChildren<VerticalLayoutGroup>();

        GetButton(Buttons.ExitBtn).AddButtonEvent(Exit);
    }

    public void Setting()
    {
        for (int i = 1; i < Define.ChallengeModeDiff.Count + 1; i++)
        {
            var button = Managers.Resource.Instantiate("ChallengeModeBtn", layout.transform).GetComponent<ChallengeButtonUI>();
            button.Init();
            button.Setting(i, () => Exit());
        }

        // Layout, Fitter, ScrollRect 업데이트 후 스크롤 맨 위로 이동
        StartCoroutine(RefreshLayoutAndScrollToTop());
    }

    private IEnumerator RefreshLayoutAndScrollToTop()
    {
        // 한 프레임 대기하여 레이아웃 갱신
        yield return null;

        // ContentSizeFitter, LayoutGroup 강제 갱신
        LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());

        // ScrollRect를 찾아서 맨 위로 이동
        var scrollRect = GetComponentInChildren<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
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
