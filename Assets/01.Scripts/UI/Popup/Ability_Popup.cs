using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public class Ability_Popup : UI_Popup
{
    enum Buttons
    {

    }

    enum Images
    {
    }

    enum Texts
    {

    }

    private Transform layout;

    private AbilityBubbleUI abilityBubbleUI;


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

        layout = gameObject.FindRecursive("Content").transform;

        Define.Tier currentTier = Define.Tier.Iron4;

        bool first = false;

        var ability = Managers.Resource.Load<AbilityData>("AbilityData/AbilityData");

        List<int> usedAdditionalAbilities = new List<int>();

        AbilitySlotUI currentProgressSlot = null;

        for (int i = 0; i < ability.abilities.Count; i++)
        {
            var data = ability.abilities[i];
            if (data.needTier != currentTier)
            {
                first = true;
                currentTier = data.needTier;
            }
            else
            {
                first = false;
            }

            var additionalAbilityIndex = ability.GetAdditionalAbilityIndex(data.needTier);

            if (usedAdditionalAbilities.Contains(additionalAbilityIndex))
            {
                additionalAbilityIndex = -1;
            }
            else if (additionalAbilityIndex != -1)
            {
                usedAdditionalAbilities.Add(additionalAbilityIndex);
            }

            var slot = Managers.Resource.Instantiate("AbilitySlotUI", layout);
            slot.transform.SetAsFirstSibling();
            slot.GetComponent<AbilitySlotUI>().Init();
            bool isLast = i == ability.abilities.Count - 1;
            slot.GetComponent<AbilitySlotUI>().Setting(i, additionalAbilityIndex, first, isLast);


            if (currentProgressSlot == null && i == Managers.Ability.GetHasAbilityCount())
            {
                currentProgressSlot = slot.GetComponent<AbilitySlotUI>();
            }
        }

        if (currentProgressSlot != null)
        {
            ScrollToTarget(currentProgressSlot.transform);
        }

        OpenAnimation();

        GetComponentInChildren<ScrollRect>().onValueChanged.AddListener((value) =>
        {
            if (abilityBubbleUI != null)
            {
                Destroy(abilityBubbleUI.gameObject);
            }
        });
    }

    private void ScrollToTarget(Transform target)
    {
        StartCoroutine(ScrollToTargetCoroutine(target));
    }

    private IEnumerator ScrollToTargetCoroutine(Transform target)
    {
        // 레이아웃 강제 업데이트
        yield return null;
        Canvas.ForceUpdateCanvases();
        yield return null;

        var scrollRect = GetComponentInChildren<ScrollRect>();
        var content = scrollRect.content;
        var viewport = scrollRect.viewport;

        // 레이아웃 업데이트 강제 실행
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // 타겟의 로컬 위치 (RectTransform 기준)
        var targetRect = target.GetComponent<RectTransform>();
        var contentRect = content;

        // 타겟의 Content 내에서의 위치 계산
        var targetLocalPosition = targetRect.anchoredPosition;

        // Content와 Viewport 크기
        var contentHeight = contentRect.rect.height;
        var viewportHeight = viewport.rect.height;

        // 스크롤 가능한 범위
        var scrollableHeight = contentHeight - viewportHeight;

        if (scrollableHeight <= 0)
        {
            // 스크롤이 필요 없는 경우
            yield break;
        }

        // 타겟이 화면 중앙에 오도록 계산
        // Y축이 위쪽이 양수이므로 음수로 계산
        // var targetYFromTop = -targetLocalPosition.y;

        // normalizedPosition 계산 (0: 맨 아래, 1: 맨 위)
        var normalizedPosition = Mathf.Clamp01((scrollableHeight + (targetLocalPosition.y + (viewportHeight / 2))) / scrollableHeight);

        // Debug.Log($"Target Position: {targetLocalPosition.y}, Content Height: {contentHeight}, Viewport Height: {viewportHeight}");
        // Debug.Log($"Scrollable Height: {scrollableHeight}, Target Y From Top: {targetYFromTop}, Normalized: {normalizedPosition}");

        // 부드러운 스크롤 애니메이션
        float duration = 0.5f;
        float elapsed = 0f;
        float startPos = scrollRect.verticalNormalizedPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            scrollRect.verticalNormalizedPosition = Mathf.Lerp(startPos, normalizedPosition, t);
            yield return null;
        }

        scrollRect.verticalNormalizedPosition = normalizedPosition;
    }

    public void GenerateAbilityBubble(Transform trans, int abilityIndex = -1, int additionalAbilityIndex = -1)
    {
        if (abilityIndex >= 0)
        {
            Generate(abilityIndex, false);
        }

        if (additionalAbilityIndex >= 0)
        {
            Generate(additionalAbilityIndex, true);
        }

        void Generate(int index, bool isAdditional = false)
        {
            if (abilityBubbleUI != null)
            {
                Destroy(abilityBubbleUI.gameObject);
            }

            abilityBubbleUI = Managers.Resource.Instantiate("AbilityBubbleUI", trans).GetComponent<AbilityBubbleUI>();
            abilityBubbleUI.Init();
            abilityBubbleUI.Setting(index, isAdditional);

            abilityBubbleUI.gameObject.FindRecursive("Panel" + (isAdditional ? "2" : "")).SetActive(true);

            abilityBubbleUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(isAdditional ? -130 : 100, 0);
        }
    }

    public override void Reset()
    {

    }

    public void Exit()
    {
        if (abilityBubbleUI != null)
        {
            Destroy(abilityBubbleUI.gameObject);
        }

        // ClosePopupUI();

        gameObject.SetActive(false);
    }
}
