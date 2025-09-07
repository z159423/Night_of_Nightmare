using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerTutorialStep
{
    StartRankGame = 0,
    StartMatching = 1,
    TouchToStart = 2,
    FixDoor = 3,
    OverHeat = 4,
    Shield = 5,
    Hammer = 6
}

public class TutorialManager : MonoBehaviour
{
    public void StartTutorial(Button targetBtn, PlayerTutorialStep playerTutorialStep, bool timePuase = false, bool descBubble = false)
    {
        if (Managers.LocalData.IsTutorialCompleted((int)playerTutorialStep))
            return;

        var mask = Managers.UI.ShowPopupUI<TutorialTouchBox>();
        mask.Setting(targetBtn, (int)playerTutorialStep, timePuase, descBubble);

        if (timePuase)
            Time.timeScale = 0f;
    }

    public bool IsCompletedTutorial(PlayerTutorialStep playerTutorialStep)
    {
        return Managers.LocalData.IsTutorialCompleted((int)playerTutorialStep);
    }
}
