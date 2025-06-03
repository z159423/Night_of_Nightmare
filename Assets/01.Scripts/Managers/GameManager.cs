using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using static Define;

public class GameManager : MonoBehaviour
{
    public GameMode currentGameMode = GameMode.Home;
    public Dictionary<GameMode, CinemachineVirtualCamera> cameras = new Dictionary<GameMode, CinemachineVirtualCamera>();

    private CharactorController charactorController;

    public PlayerCharactor playerCharactor;

    public void ChangeGameMode(GameMode mode)
    {
        if (currentGameMode == mode)
            return;

        ChangeCamera(mode);
        currentGameMode = mode;

        switch (mode)
        {
            case GameMode.Home:
                Managers.UI.ShowSceneUI<UI_GameScene_Home>();
                break;
            case GameMode.Map:
                Managers.UI.ShowSceneUI<UI_GameScene_Map>();
                break;
        }
    }

    public void ChangeCamera(GameMode mode)
    {
        // 현재 게임 모드의 카메라 priority를 낮춤
        if (cameras.TryGetValue(currentGameMode, out var prevCamera))
        {
            prevCamera.Priority = 0;
        }

        // 새 게임 모드의 카메라 priority를 높임
        if (cameras.TryGetValue(mode, out var newCamera))
        {
            newCamera.Priority = 10;
        }
    }

    public void OnRankGameStart()
    {
        ChangeGameMode(GameMode.Map);

        if (charactorController == null)
        {
            charactorController = Managers.Resource.Instantiate("ControllerCanvas").GetComponent<CharactorController>();

            charactorController.player = playerCharactor.GetComponent<NavMeshAgent>();
            charactorController.playerCharactor = playerCharactor;

        }

        cameras[GameMode.Map].Follow = playerCharactor.transform;
    }
}
