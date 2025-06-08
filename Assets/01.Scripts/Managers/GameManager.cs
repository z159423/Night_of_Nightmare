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

    public CharactorType selectedPlayerCharactorType = CharactorType.Farmer;

    public int coin = 0;
    public int energy = 0;
    public int ticket = 0;

    public Map currentMap;
    public List<Bed> beds = new List<Bed>();
    public CharactorType[] charactorType;
    public List<PlayerableCharactor> charactors = new List<PlayerableCharactor>();
    public EnemyType enemyType;
    public Enemy enemy;

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
        currentMap = Managers.Resource.Instantiate("Maps/Map1").GetComponent<Map>();
        currentMap.Setting();

        Managers.Game.ChangeGameMode(GameMode.Map);

        if (Managers.UI._currentScene is UI_GameScene_Map)
            Managers.UI._currentScene.GetComponent<UI_GameScene_Map>().ClearCharactorIcons();

        // 플레이어 캐릭터 생성
        var playerCharactor = Managers.Resource.Instantiate("PlayerCharactor", currentMap.transform);
        SetPos(playerCharactor.transform);
        playerCharactor.GetComponentInChildren<PlayerCharactor>().Setting();

        CharactorType[] tempTypes = new CharactorType[5]
        {
            charactorType[0],
            charactorType[2],
            charactorType[3],
            charactorType[4],
            charactorType[5]
        };

        // ai 캐릭터 생성
        for (int i = 0; i < 5; i++)
        {
            var aiCharactor = Managers.Resource.Instantiate("AICharactor", currentMap.transform);

            SetPos(aiCharactor.transform);

            aiCharactor.GetComponentInChildren<AiCharactor>().SettingAiCharactor(tempTypes[i]);
            aiCharactor.GetComponentInChildren<AiCharactor>().ActiveAiCharactor();
        }

        // 캐릭터 컨트롤러 생성
        if (charactorController == null)
        {
            charactorController = Managers.Resource.Instantiate("ControllerCanvas").GetComponent<CharactorController>();

            charactorController.player = Managers.Game.playerCharactor.GetComponentInParent<UnityEngine.AI.NavMeshAgent>();
            charactorController.playerCharactor = Managers.Game.playerCharactor;

            Managers.Camera.TurnVinettaEffect(false);
        }

        // 적생성
        if (enemy == null)
        {
            StartCoroutine(spawnEnemy());

            IEnumerator spawnEnemy()
            {
                yield return new WaitForSeconds(20f); // 적 생성 딜레이

                if (currentMap.transform != null)
                {
                    var _enemy = Managers.Resource.Instantiate("Enemy", currentMap.transform);

                    SetPos(_enemy.transform);

                    _enemy.GetComponentInChildren<Enemy>().Setting();
                    enemy = _enemy.GetComponentInChildren<Enemy>();
                }
            }
        }

        Managers.Camera.cameras[GameMode.Map].Follow = Managers.Game.playerCharactor.transform;

        // 반지름 1.6유닛(160px) 원 안에 랜덤 스폰
        void SetPos(Transform trans)
        {
            Vector3 center = currentMap.charactorSpawnPoint.position;
            float radius = 2f; // 1.6유닛 = 160px (유닛:미터 기준)
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(0f, radius);
            float x = Mathf.Cos(angle) * distance;
            float z = Mathf.Sin(angle) * distance;
            Vector3 spawnPos = center + new Vector3(x, z, 0);

            trans.position = spawnPos;
        }
    }

    public void GoHome()
    {
        Managers.Game.ChangeGameMode(GameMode.Home);
        Managers.Camera.ChangeMapCameraMode(CameraManager.MapCameraMode.Player);
        Managers.Camera.cameras[GameMode.Map].Follow = null;

        if (charactorController != null)
        {
            Destroy(charactorController.gameObject);
            charactorController = null;
        }

        Managers.Camera.TurnVinettaEffect(true);

        Destroy(currentMap.gameObject);

        beds.Clear();
        charactors.Clear();
    }

    public void GameOver()
    {

    }

    public void GameWin()
    {

    }
}
