using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Tier_Popup : UI_Popup
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

    GameObject layout;
    ScrollRect scrollRect;

    public override void Init()
    {
        base.Init();

        TierBox currentTierBox = null;

        foreach (var tier in Define.TierToScore.Keys)
        {
            var tierBox = Managers.Resource.Instantiate("TierBox").GetComponent<TierBox>();
            tierBox.Init();
            var find = tierBox.Setting(tier);
            tierBox.transform.SetParent(layout.transform, false);

            if (find != null)
                currentTierBox = find;
        }

        if (currentTierBox != null && scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();

            RectTransform content = layout.GetComponent<RectTransform>();
            RectTransform viewport = scrollRect.viewport;
            RectTransform targetRect = currentTierBox.GetComponent<RectTransform>();

            float contentHeight = content.rect.height;
            float viewportHeight = viewport.rect.height;
            float targetCenterY = Mathf.Abs(targetRect.anchoredPosition.y) + targetRect.rect.height / 2f;

            float normalized = (targetCenterY - viewportHeight / 2f) / (contentHeight - viewportHeight);
            normalized = 1f - Mathf.Clamp01(normalized);

            scrollRect.verticalNormalizedPosition = normalized;
        }
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        layout = gameObject.FindRecursive("Layout");
        scrollRect = gameObject.FindRecursive("Scroll View").GetComponent<ScrollRect>();

        GetButton(Buttons.ExitBtn).AddButtonEvent(Exit);
    }

    public override void Reset()
    {

    }

    private void Exit()
    {
        ClosePopupUI();
    }
}
