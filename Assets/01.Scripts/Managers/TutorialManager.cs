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
    public void GenerateButtonMask(Button targetBtn, PlayerTutorialStep playerTutorialStep)
    {
        var mask = Managers.UI.ShowPopupUI<TutorialTouchBox>();
        mask.Setting(targetBtn, (int)playerTutorialStep);
    }

    public bool IsTutorialCompleted(PlayerTutorialStep playerTutorialStep)
    {
        return Managers.LocalData.IsTutorialCompleted((int)playerTutorialStep);
    }
}
