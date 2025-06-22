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
        MatchingBtn
    }

    enum Images
    {
        TouchGuard,
        RankImage
    }

    enum Texts
    {
        MatchingText,
        MatchingTimeText,
        RankingPointText
    }

    GameObject matching;
    GameObject ranking;

    Transform playerLayout;
    PlayerBoxUI enemyBox;
    PlayerBoxUI[] playerBoxes;

    private bool isMatching = false;

    private float matchingTime = 0f;


    public override void Init()
    {
        base.Init();

        GetImage(Images.RankImage).sprite = Managers.Resource.Load<Sprite>($"Tier/{Define.GetPlayerCurrentTier().ToString()}");
        GetImage(Images.RankImage).SetNativeSize();
        GetTextMesh(Texts.RankingPointText).text = Define.GetPlayerCurrentTier().ToString() + "<br>" + Managers.LocalData.PlayerRankingPoint.ToString();

        OpenAnimation();
    }

    void Update()
    {
        if (isMatching)
        {
            matchingTime += Time.deltaTime;
            GetTextMesh(Texts.MatchingTimeText).text = Managers.Localize.GetDynamicText("global.str_matching", matchingTime.ToString("F1"));
        }
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
        GetButton(Buttons.MatchBtn).AddButtonEvent(StartMatching);
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
                        Managers.Game.OnRankGameStart();
                    });

                    yield return new WaitForSeconds(5f);

                    if (!Managers.Game.isGameStart)
                    {
                        Exit();
                        Managers.Game.OnRankGameStart();
                    }
                }
            }
        });

        GetTextMesh(Texts.MatchingTimeText).text = Managers.Localize.GetDynamicText("global.str_matching", matchingTime.ToString("F1"));
    }

    private void StartMatching()
    {
        matching.SetActive(true);
        ranking.SetActive(false);

        isMatching = true;

        // Define.EnemyType 배열에서 랜덤으로 하나 선택
        var enemyTypes = System.Enum.GetValues(typeof(Define.EnemyType));
        var random = new System.Random();
        var enemyType = (Define.EnemyType)enemyTypes.GetValue(random.Next(enemyTypes.Length));

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
                box.PlayerSetting(Managers.Game.currentPlayerCharacterType, "Me");
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
}
