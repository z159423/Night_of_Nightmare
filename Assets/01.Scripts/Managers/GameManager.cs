using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using VInspector;
using System.Linq;
using static Define;
using NavMeshPlus.Components;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    public GameMode currentGameMode = GameMode.Home;
    public PlayerCharactor playerCharactor;
    private CharactorController charactorController;

    public Map currentMap;
    public NavMeshPlus.Components.NavMeshSurface navMeshSurface;
    public List<Bed> beds = new List<Bed>();
    public CharactorType[] charactorType;
    public List<PlayerableCharactor> charactors = new List<PlayerableCharactor>();

    public PlayerData playerData;
    //ai들 데이터
    public List<PlayerData> aiPlayerDatas = new List<PlayerData>();

    public EnemyType enemyType;
    public Enemy enemy;
    public string enemyName;

    public bool isGameStart = false;
    public Coroutine spawnEnemyCoroutine;

    public Tile selectedTile;

    public Dictionary<StructureType, StructureData> structuredatas = new Dictionary<StructureType, StructureData>();

    Coroutine getReourcesCoroutine;

    public bool isChallengeMode = false;
    public int challengeLevel = 0;

    float lossPoint = 0;

    private NavMeshDataInstance navMeshInstance;


    public List<HomeCharactor> homeCharactors = new List<HomeCharactor>();

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Managers.LocalData.PlayerGemCount += 100;
        }
    }
#endif

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

    [Button("OnRankGameStart")]
    public void OnRankGameStart(bool challengeMode = false, int level = 0)
    {
        lossPoint = LoseRankingPoint();

        isGameStart = true;
        isChallengeMode = challengeMode;
        challengeLevel = level;

        playerData = new PlayerData((Define.CharactorType)Managers.LocalData.SelectedCharactor);

        int mapIndex = 1;

        currentMap = Managers.Resource.Instantiate("Maps/Map" + mapIndex).GetComponent<Map>();
        currentMap.Setting();

        if (navMeshSurface == null)
        {
            navMeshSurface = Managers.Resource.Instantiate("NavMeshSurface2D").GetComponent<NavMeshPlus.Components.NavMeshSurface>();
        }

        navMeshSurface.BuildNavMeshAsync();

        // navMeshSurface.UpdateNavMesh(Managers.Resource.Load<NavMeshData>("NavMeshSurfaceData/Map" + 1));

        // navMeshInstance = NavMesh.AddNavMeshData(Managers.Resource.Load<NavMeshData>("NavMeshSurfaceData/Map" + 1), transform.position, transform.rotation);

        Managers.Game.ChangeGameMode(GameMode.Map);

        if (Managers.UI._currentScene is UI_GameScene_Map)
            Managers.UI._currentScene.GetComponent<UI_GameScene_Map>().ClearCharactorIcons();

        // 플레이어 캐릭터 생성
        var playerCharactor = Managers.Resource.Instantiate("PlayerCharactor", currentMap.transform);
        SetPos(playerCharactor.transform);
        playerCharactor.GetComponentInChildren<PlayerCharactor>().Setting();

        getReourcesCoroutine = StartCoroutine(GetResources());

        if (charactorType == null || charactorType.Length == 0)
        {
            charactorType = new CharactorType[6]
            {
                CharactorType.Farmer,
                CharactorType.Miner,
                  CharactorType.ReapireMan,
                  CharactorType.Chef,
                  CharactorType.Scientist,
                  CharactorType.Farmer,
            };
        }

        CharactorType[] tempTypes = new CharactorType[5]
        {
            charactorType[0],
            charactorType[2],
            charactorType[3],
            charactorType[4],
            charactorType[5]
        };

        int waitTime = Random.Range(Managers.LocalData.PlayerWinCount > 0 ? 1 : 3, 6);

        // ai 캐릭터 생성
        for (int i = 0; i < 5; i++)
        {
            var aiCharactor = Managers.Resource.Instantiate("AICharactor", currentMap.transform);

            SetPos(aiCharactor.transform);

            aiCharactor.GetComponentInChildren<AiCharactor>().SettingAiCharactor(tempTypes[i]);
            aiCharactor.GetComponentInChildren<AiCharactor>().ActiveAiCharactor(waitTime);
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
            spawnEnemyCoroutine = StartCoroutine(spawnEnemy());

            IEnumerator spawnEnemy()
            {
                yield return new WaitForSeconds(20f); // 적 생성 딜레이

                if (enemy == null && currentMap.transform != null)
                {
                    SpawnEnemy();
                }
            }
        }

        Managers.Camera.cameras[GameMode.Map].Follow = Managers.Game.playerCharactor.transform;

        // 반지름 1.6유닛(160px) 원 안에 랜덤 스폰
    }
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

    [Button("SpawnEnemy")]
    void SpawnEnemy()
    {
        var _enemy = Managers.Resource.Instantiate("Enemy", currentMap.transform);

        SetPos(_enemy.transform);

        _enemy.GetComponentInChildren<Enemy>().Setting(enemyType);
        enemy = _enemy.GetComponentInChildren<Enemy>();
    }

    public void GoHome()
    {
        isGameStart = false;

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

        if (spawnEnemyCoroutine != null)
        {
            StopCoroutine(spawnEnemyCoroutine);
            spawnEnemyCoroutine = null;
        }

        if (getReourcesCoroutine != null)
        {
            StopCoroutine(getReourcesCoroutine);
            getReourcesCoroutine = null;
        }
    }

    [Button("GameOver")]
    public void GameOver()
    {
        Managers.UI.CloseAllPopupUI();

        GoHome();

        var result = Managers.UI.ShowPopupUI<MatchResult_Popup>();

        result.Init();
        result.Setting(false, (int)lossPoint, isChallengeMode);

        isChallengeMode = false;
    }

    [Button("GameWin")]
    public void GameWin()
    {
        Managers.LocalData.PlayerWinCount++;

        int point = 0;

        Managers.LocalData.PlayerRankingPoint += (int)lossPoint;

        if (isChallengeMode)
        {
            if (challengeLevel == Managers.LocalData.ChallengeModeLevel)
            {
                Managers.LocalData.ChallengeModeLevel = challengeLevel + 1;
            }
        }
        else
        {

            var pointRange = TierWinGetPoint[GetPlayerCurrentTier()];
            point = Random.Range(pointRange.Item1, pointRange.Item2);

            Managers.LocalData.PlayerRankingPoint += point;
        }

        GoHome();

        var result = Managers.UI.ShowPopupUI<MatchResult_Popup>();

        result.Init();
        result.Setting(true, point, isChallengeMode);

        isChallengeMode = false;

    }

    public IEnumerator GetResources()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            foreach (var charactor in charactors)
            {
                charactor.playerData.GetResources();
            }

            GameObserver.Call(GameObserverType.Game.OnChangeCoinCount);
            GameObserver.Call(GameObserverType.Game.OnChangeEnergyCount);
        }
    }

    [Button("AddCoin")]
    public void AddCoin()
    {
        playerData.coin += 1000;
        GameObserver.Call(GameObserverType.Game.OnChangeCoinCount);
    }

    [Button("AddEnergy")]
    public void AddEnergy()
    {
        playerData.energy += 1000;
        GameObserver.Call(GameObserverType.Game.OnChangeEnergyCount);
    }

    public StructureData GetStructureData(StructureType type)
    {
        if (structuredatas.ContainsKey(type))
            return structuredatas[type];

        StructureData data = Managers.Resource.GetStructureData(type);
        structuredatas.Add(type, data);
        return data;
    }

    public void OnPurchaseBoostItem(BoostType type)
    {
        if (GetBoostData(type).price <= Managers.LocalData.PlayerGemCount)
        {
            Managers.LocalData.PlayerGemCount -= GetBoostData(type).price;
            Managers.LocalData.AddBoostItem(type, 1);
        }
    }

    public void OnUserBoostItem(BoostType type)
    {
        Managers.LocalData.AddBoostItem(type, -1);
    }

    public bool BuildStructure(PlayerData _playerData, StructureType type, Tile tile)
    {
        var structureData = GetStructureData(type);

        if (structureData.CanPurchase(_playerData, out string reason, 0))
        {
            if (reason == "FREE")
            {
                _playerData.AddFreeCount(type);
            }
            else
            {
                _playerData.UseResource(structureData.GetPurchaseCoin(0, _playerData), structureData.GetPurchaseEnergy(0, _playerData));

                if (type == StructureType.Lamp && _playerData == Managers.Game.playerData)
                    Managers.LocalData.PlayerLampCount--;
            }

            var find = Managers.Resource.LoadAll<GameObject>("Structures").First(n => n.GetComponentInChildren<Structure>() != null && n.GetComponentInChildren<Structure>().type == structureData.structureType);
            var structure = Instantiate(find, tile.transform).GetComponentInChildren<Structure>();

            if (_playerData != this.playerData && (type == StructureType.Turret || type == StructureType.Generator || type == StructureType.CopperOre || type == StructureType.SilverOre || type == StructureType.GoldOre))
            {
                structure.CheckPossibleUpgrade();
            }

            var particle = Managers.Resource.Instantiate("Particles/StructureProductParticle");

            particle.transform.position = tile.transform.position;

            StartCoroutine(destroy());

            IEnumerator destroy()
            {
                yield return new WaitForSeconds(1.2f);
                Managers.Resource.Destroy(particle);
            }

            _playerData.BuildStructure(structure);

            if (type != StructureType.MovingFrog)
                tile.currentStructure = structure;

            return true;
        }
        else
            return false;
    }

    public float LoseRankingPoint()
    {
        if (!isChallengeMode)
        {
            var pointRange = TierWinGetPoint[GetPlayerCurrentTier()];
            float point = Random.Range(pointRange.Item1, pointRange.Item2) * TierLossRatio[GetPlayerCurrentTier()];

            Managers.LocalData.PlayerRankingPoint -= (int)point;

            return (int)point;
        }

        return 0;
    }

    public void ChangePlayerCharactor(CharactorType type)
    {
        Managers.LocalData.SelectedCharactor = (int)type;
    }
}