using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Match_Making_Popup : UI_Popup
{
    enum Buttons
    {
        ExitBtn,
        MatchBtn,
        InfoBtn,
        MatchingBtn,
        RankModeRvBtn
    }

    enum Images
    {
        TouchGuard,
        RankImage,
        CoinRvImage,
        EnergyRvImage
    }

    enum Texts
    {
        MatchingText,
        MatchingTimeText,
        RankingPointText,
        Title
    }

    GameObject matching;
    GameObject ranking;

    Transform playerLayout;
    PlayerBoxUI enemyBox;
    PlayerBoxUI[] playerBoxes;

    private bool isMatching = false;

    private float matchingTime = 0f;

    bool isChallengeMode = false;
    int challengeStage = 0;


    public override void Init()
    {
        base.Init();

        GetImage(Images.RankImage).sprite = Managers.Resource.Load<Sprite>($"Tier/{Define.GetPlayerCurrentTier().ToString()}");
        GetImage(Images.RankImage).SetNativeSize();
        GetTextMesh(Texts.RankingPointText).text = Define.GetPlayerCurrentTier().ToString() + "<br>" + Managers.LocalData.PlayerRankingPoint.ToString();

        OnShowStartRv();
        OpenAnimation();

        GetButton(Buttons.ExitBtn).gameObject.SetActive(Managers.LocalData.PlayerGameCount > 0);

        Managers.Tutorial.StartTutorial(GetButton(Buttons.MatchBtn), PlayerTutorialStep.StartMatching);

        GetButton(Buttons.RankModeRvBtn).gameObject.SetActive(Managers.LocalData.PlayerGameCount >= 1 && (!Managers.Game.goldRvBonus && !Managers.Game.energyRvBonus));

    }

    void Update()
    {
        if (isMatching)
        {
            matchingTime += Time.deltaTime;
            GetTextMesh(Texts.MatchingTimeText).text = Managers.Localize.GetDynamicText("global.str_matching", matchingTime.ToString("F1"));
        }
    }

    public void Setting(bool isChallengeMode = false, int level = 0)
    {
        this.isChallengeMode = isChallengeMode;
        this.challengeStage = level;

        GetTextMesh(Texts.Title).text = isChallengeMode ? Managers.Localize.GetText("global.str_challenge_mode") : Managers.Localize.GetText("global.str_rank_mode");
    }

    public override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));

        matching = gameObject.FindRecursive("Matching");
        ranking = gameObject.FindRecursive("Ranking");

        GetButton(Buttons.ExitBtn).AddButtonEvent(Exit);
        GetButton(Buttons.MatchBtn).AddButtonEvent(() => StartMatching(false));
        GetButton(Buttons.InfoBtn).AddButtonEvent(() =>
        {
            Managers.UI.ShowPopupUI<Tier_Popup>();
        });

        playerLayout = gameObject.FindRecursive("PlayerLayout").transform;
        enemyBox = gameObject.FindRecursive("EenmyBoxUI").GetComponent<PlayerBoxUI>();
        playerBoxes = playerLayout.GetComponentsInChildren<PlayerBoxUI>(true);

        this.SetListener(GameObserverType.Game.OnMatchedPlayerCharactor, () =>
        {
            // 모든 playerBoxes가 isFind == true인지 확인
            if (playerBoxes.All(n => n.isFind) && enemyBox.isFind)
            {
                isMatching = false;

                // 모든 플레이어와 적이 찾았을 때 실행할 코드
                GetTextMesh(Texts.MatchingText).text = Managers.Localize.GetText("global.str_gamestart_ready");

                StartCoroutine(wait());

                IEnumerator wait()
                {
                    yield return new WaitForSeconds(Random.Range(2f, 4f));

                    GetTextMesh(Texts.MatchingText).text = Managers.Localize.GetText("global.str_touch_and_start");
                    GetButton(Buttons.MatchingBtn).GetComponent<Image>().raycastTarget = true;

                    GetButton(Buttons.MatchingBtn).AddButtonEvent(() =>
                    {
                        Exit();
                        Managers.Game.OnRankGameStart(isChallengeMode, challengeStage);
                    });

                    if (!Managers.Tutorial.IsCompletedTutorial(PlayerTutorialStep.TouchToStart))
                    {
                        Managers.Tutorial.StartTutorial(GetButton(Buttons.MatchingBtn), PlayerTutorialStep.TouchToStart);
                    }
                    else
                    {
                        yield return new WaitForSeconds(5f);

                        if (!Managers.Game.isGameStart)
                        {
                            Exit();
                            Managers.Game.OnRankGameStart(isChallengeMode, challengeStage);
                        }
                    }
                }
            }
        });

        GetButton(Buttons.RankModeRvBtn).AddButtonEvent(() =>
        {
            var popup = Managers.UI.ShowPopupUI<StartRv_Popup>();

            popup.onShowRv = () =>
            {
                Managers.Game.goldRvBonus = false;
                Managers.Game.energyRvBonus = false;

                if (UnityEngine.Random.Range(0, 2) == 0)
                    Managers.Game.energyRvBonus = true;
                else
                    Managers.Game.goldRvBonus = true;

                GetButton(Buttons.RankModeRvBtn).gameObject.SetActive(false);

                OnShowStartRv();
            };
        });

        GetTextMesh(Texts.MatchingTimeText).text = Managers.Localize.GetDynamicText("global.str_matching", matchingTime.ToString("F1"));
    }

    public void StartMatching(bool challengeMode = false)
    {
        matching.SetActive(true);
        ranking.SetActive(false);

        isMatching = true;

        if (challengeMode)
        {
            GetButton(Buttons.MatchingBtn).transform.localScale = Vector3.one;
        }

        // Define.EnemyType 배열에서 랜덤으로 하나 선택
        var enemyTypes = System.Enum.GetValues(typeof(Define.EnemyType));

        Define.EnemyType enemyType;

        if (Managers.LocalData.PlayerWinCount == 0)
            enemyType = Define.EnemyType.TungTungTung;
        else if (Managers.LocalData.PlayerWinCount == 1)
            enemyType = Define.EnemyType.Tralalero;
        else
        {
            var random = new System.Random();
            enemyType = (Define.EnemyType)enemyTypes.GetValue(random.Next(enemyTypes.Length));
        }

        Managers.Game.enemyType = enemyType;

        // 5개의 캐릭터 타입을 랜덤으로 선택
        var charactorTypesArray = System.Enum.GetValues(typeof(Define.CharactorType));
        Managers.Game.charactorType = new Define.CharactorType[6];
        var rand = new System.Random();
        for (int i = 0; i < 6; i++)
        {
            Managers.Game.charactorType[i] = (Define.CharactorType)charactorTypesArray.GetValue(rand.Next(charactorTypesArray.Length));
        }

        for (int i = 0; i < playerBoxes.Length; i++)
        {
            var box = playerBoxes[i];

            box.Init();
            if (!box.isPlayer)
                box.PlayerSetting(Managers.Game.charactorType[i], NameGenerator.GetRandomName());
            else
                box.PlayerSetting((Define.CharactorType)Managers.LocalData.SelectedCharactor, "Me");
        }

        enemyBox.Init();
        var name = NameGenerator.GetRandomName();
        enemyBox.EnemySetting(enemyType, name);
        Managers.Game.enemyName = name;

        OpenAnimation();
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public override void Reset()
    {

    }

    private void Exit()
    {
        ClosePopupUI();
    }

    private void OnShowStartRv()
    {
        GetImage(Images.CoinRvImage).gameObject.SetActive(Managers.Game.goldRvBonus);
        GetImage(Images.EnergyRvImage).gameObject.SetActive(Managers.Game.energyRvBonus);

        if (Managers.Game.goldRvBonus)
            Managers.UI.GenerateUIParticle(transform, GetImage(Images.CoinRvImage).transform, GetImage(Images.CoinRvImage).sprite, Vector3.one * 3);

        if (Managers.Game.energyRvBonus)
            Managers.UI.GenerateUIParticle(transform, GetImage(Images.EnergyRvImage).transform, GetImage(Images.EnergyRvImage).sprite, Vector3.one * 3);
    }
}
