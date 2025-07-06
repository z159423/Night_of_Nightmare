using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class HomeCharactor : MonoBehaviour
{
    GameObject selectFloor;
    SpriteRenderer icon;
    GameObject selectIcon;

    DOTweenAnimation dOTweenAnimation;

    public Define.CharactorType type;

    // Start is called before the first frame updat
    void Start()
    {
        Managers.Game.homeCharactors.Add(this);

        StartCoroutine(waitUntil());
        IEnumerator waitUntil()
        {
            while (Managers.Resource == null)
            {
                yield return null;
            }

            dOTweenAnimation = GetComponentInChildren<DOTweenAnimation>();

            dOTweenAnimation.DOPause();
            selectFloor = gameObject.FindRecursive("SelectFloor");
            icon = gameObject.FindRecursive("Icon").GetComponent<SpriteRenderer>();
            selectIcon = gameObject.FindRecursive("SelectIcon");

            icon.sprite = Managers.Resource.GetCharactorImage((int)type + 1);

            UpdateUI();

            this.SetListener(GameObserverType.Game.OnChangeCharactor, () =>
            {
                UpdateUI();
            });

            this.SetListener(GameObserverType.Game.OnChangeSelectedCharactorType, () =>
            {
                if (CharactorSelect_Popup.selectedType == type)
                {
                    OnSelect();
                }
                else
                {
                    OnUnselect();
                }
            });
        }
    }

    void UpdateUI()
    {
        if (type == (Define.CharactorType)Managers.LocalData.SelectedCharactor)
        {
            selectIcon.SetActive(true);

            dOTweenAnimation.DOPlay();
        }
        else
        {
            selectIcon.SetActive(false);

        }

        if (Managers.LocalData.HasCharactor(type))
        {
            icon.color = Color.white;
        }
        else
        {
            icon.color = new Color32(34, 34, 34, 255);
        }
    }

    public void OnSelect()
    {
        // 0.5초마다 selectFloor가 켜지고 꺼지는 트윈
        DOTween.Kill(selectFloor); // 중복 방지
        selectFloor.SetActive(true);
        DOTween.Sequence()
            .AppendCallback(() => selectFloor.SetActive(true))
            .AppendInterval(0.5f)
            .AppendCallback(() => selectFloor.SetActive(false))
            .AppendInterval(0.5f)
            .SetLoops(-1, LoopType.Restart)
            .SetId(selectFloor);

        dOTweenAnimation.DOPlay();
    }

    public void OnUnselect()
    {
        DOTween.Kill(selectFloor); // 트윈 중지
        selectFloor.SetActive(false);

        if (dOTweenAnimation != null)
        {
            dOTweenAnimation.DOPause();
        }
    }
}
