using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBoxUI : UI_Base
{

    enum Images
    {
        LoadingIcon,
        PlayerIcon
    }

    enum Texts
    {
        NameText
    }

    bool _init = false;

    public bool isPlayer = false;

    public bool isFind = false;

    public override void Init()
    {
        if (!_init)
        {
            FirstSetting();
        }

        AnimateLoadingIcon(GetImage(Images.LoadingIcon).transform);
    }

    private void AnimateLoadingIcon(Transform icon)
    {
        void StartRotation(float startAngle = 0f, float duration = 5f)
        {
            icon.localEulerAngles = new Vector3(0, 0, startAngle);
            StartCoroutine(RandomPauseCoroutine(duration, startAngle));
        }

        IEnumerator RandomPauseCoroutine(float durationPer360, float startAngle)
        {
            float speed = 360f / durationPer360; // degrees per second
            float currentAngle = startAngle;

            while (true)
            {
                // Start a new rotation cycle
                float targetAngle = currentAngle - 360f;
                float elapsed = 0f;
                float cycleDuration = 360f / speed;
                Tween tween = icon.DORotate(new Vector3(0, 0, targetAngle), cycleDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear);

                // Wait for a random pause time within the cycle
                float pauseTime = Random.Range(0.33f, 9f);
                while (elapsed < pauseTime && tween.active)
                {
                    yield return null;
                    elapsed += Time.deltaTime;
                }

                // Pause rotation
                icon.DOKill();
                // Calculate the actual angle at pause
                currentAngle = icon.localEulerAngles.z;
                float pauseDuration = Random.Range(0.25f, 0.7f);
                yield return new WaitForSeconds(pauseDuration);

                // Calculate how much angle is left to complete the 360°
                float angleLeft = (360f - ((startAngle - currentAngle + 360f) % 360f));
                float durationLeft = angleLeft / speed;

                // Resume rotation for the remaining angle
                tween = icon.DORotate(new Vector3(0, 0, currentAngle - angleLeft), durationLeft, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear);

                // Wait for the rest of the cycle or until next pause
                elapsed = 0f;
                float nextPauseTime = Random.Range(4f, 8f);
                while (elapsed < nextPauseTime && tween.active)
                {
                    yield return null;
                    elapsed += Time.deltaTime;
                }

                // Pause again
                icon.DOKill();
                currentAngle = icon.localEulerAngles.z;
                pauseDuration = Random.Range(0.25f, 0.7f);
                yield return new WaitForSeconds(pauseDuration);

                // Calculate angle left for the next cycle
                angleLeft = (360f - ((startAngle - currentAngle + 360f) % 360f));
                durationLeft = angleLeft / speed;

                // Resume rotation for the remaining angle
                tween = icon.DORotate(new Vector3(0, 0, currentAngle - angleLeft), durationLeft, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear);

                // After this, the loop continues for the next cycle
                startAngle = currentAngle; // Update startAngle for next cycle
            }
        }

        StartRotation();
    }

    public void FirstSetting()
    {
        _init = true;

        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    public void PlayerSetting(Define.CharactorType charactorType, string playerName)
    {
        if (isPlayer)
        {
            GetImage(Images.PlayerIcon).sprite = Managers.Resource.GetCharactorImage((int)charactorType + 1);
            GetImage(Images.PlayerIcon).gameObject.SetActive(true);
            GetImage(Images.PlayerIcon).SetNativeSize();

            GetTextMesh(Texts.NameText).text = Managers.Localize.GetText("global.str_me");

            // Reset the loading icon rotation
            GetImage(Images.LoadingIcon).gameObject.SetActive(false);

            isFind = true;

            GameObserver.Call(GameObserverType.Game.OnMatchedPlayerCharactor);
        }
        else
        {
            StartCoroutine(findPlayerWait());
        }

        IEnumerator findPlayerWait()
        {
            yield return new WaitForSeconds(Random.Range(1f, 10f));

            GetImage(Images.PlayerIcon).sprite = Managers.Resource.GetCharactorImage((int)charactorType + 1);
            GetImage(Images.PlayerIcon).gameObject.SetActive(true);
            GetImage(Images.PlayerIcon).SetNativeSize();

            GetTextMesh(Texts.NameText).text = playerName;

            // Reset the loading icon rotation
            GetImage(Images.LoadingIcon).gameObject.SetActive(false);

            isFind = true;

            GameObserver.Call(GameObserverType.Game.OnMatchedPlayerCharactor);
        }
    }

    public void EnemySetting(Define.EnemyType enemyType, string playerName)
    {
        StartCoroutine(findPlayerWait());

        IEnumerator findPlayerWait()
        {
            yield return new WaitForSeconds(Random.Range(1f, 10f));

            GetImage(Images.PlayerIcon).sprite = Managers.Resource.GetEnemyImage((int)enemyType + 1);
            GetImage(Images.PlayerIcon).gameObject.SetActive(true);
            GetImage(Images.PlayerIcon).SetNativeSize();

            GetTextMesh(Texts.NameText).text = playerName;

            // Reset the loading icon rotation
            GetImage(Images.LoadingIcon).gameObject.SetActive(false);

            isFind = true;

            GameObserver.Call(GameObserverType.Game.OnMatchedPlayerCharactor);
        }
    }
}
