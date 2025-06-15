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
        StartCoroutine(waitUntil());
        IEnumerator waitUntil()
        {
            while (Managers.Resource == null)
            {
                yield return null;
            }

            dOTweenAnimation = GetComponentInChildren<DOTweenAnimation>();
            selectFloor = gameObject.FindRecursive("SelectFloor");
            icon = gameObject.FindRecursive("Icon").GetComponent<SpriteRenderer>();
            selectIcon = gameObject.FindRecursive("SelectIcon");

            icon.sprite = Managers.Resource.GetCharactorImage((int)type + 1);

            if (type == Managers.Game.currentPlayerCharacterType)
            {
                selectIcon.SetActive(true);
                icon.color = Color.white;
                dOTweenAnimation.DORestart();
            }
            else
            {
                selectIcon.SetActive(false);
                icon.color = new Color32(34, 34, 34, 255);
                dOTweenAnimation.DOPause();
            }

            this.SetListener(GameObserverType.Game.OnChangeCharactor, () =>
            {
                if (type == Managers.Game.currentPlayerCharacterType)
                {
                    selectIcon.SetActive(true);
                    icon.color = Color.white;
                    dOTweenAnimation.DORestart();

                }
                else
                {
                    selectIcon.SetActive(false);
                    icon.color = new Color32(34, 34, 34, 255);
                    dOTweenAnimation.DOPause();
                }
            });

            this.SetListener(GameObserverType.Game.OnChangeSelectedCharactorType, () =>
            {
                if (CharactorSelect_Popup.selectedCharactorType == type)
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

    public void OnSelect()
    {
        if (dOTweenAnimation != null)
        {
            dOTweenAnimation.DORestart();
        }
        else
        {
            Debug.LogWarning("DOTweenAnimation component is not attached to the GameObject.");
        }

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

        dOTweenAnimation.DORestart();
    }

    public void OnUnselect()
    {
        if (dOTweenAnimation != null)
        {
            dOTweenAnimation.DOPlayBackwards();
        }
        else
        {
            Debug.LogWarning("DOTweenAnimation component is not attached to the GameObject.");
        }

        DOTween.Kill(selectFloor); // 트윈 중지
        selectFloor.SetActive(false);

        dOTweenAnimation.DOPause();
    }
}
