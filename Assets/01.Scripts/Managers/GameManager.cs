using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using static Define;

public class GameManager : MonoBehaviour
{
    public GameMode currentGameMode = GameMode.Home;
    public PlayerCharactor playerCharactor;
    private CharactorController charactorController;

    public int coin = 0;
    public int energy = 0;
    public int ticket = 0;


    void Start()
    {
        this.SetListener(GameObserverType.Game.OnChangeCoinCount, () =>
        {

        });
    }


    public void ChangeGameMode(GameMode mode)
    {
        if (currentGameMode == mode)
            return;

        Managers.Camera.ChangeCamera(mode);
        currentGameMode = mode;

        switch (mode)
        {
            case GameMode.Home:
                var scene1 = Managers.UI.ShowSceneUI<UI_GameScene_Home>();
                scene1.Init();
                break;
            case GameMode.Map:
                var scene2 = Managers.UI.ShowSceneUI<UI_GameScene_Map>();
                scene2.Init();
                break;
        }
    }

    public void OnRankGameStart()
    {
        Managers.Game.ChangeGameMode(GameMode.Map);

        if (charactorController == null)
        {
            charactorController = Managers.Resource.Instantiate("ControllerCanvas").GetComponent<CharactorController>();

            charactorController.player = Managers.Game.playerCharactor.GetComponentInParent<UnityEngine.AI.NavMeshAgent>();
            charactorController.playerCharactor = Managers.Game.playerCharactor;

            Managers.Camera.TurnVinettaEffect(false);

        }

        Managers.Camera.cameras[GameMode.Map].Follow = Managers.Game.playerCharactor.transform;
    }
}
