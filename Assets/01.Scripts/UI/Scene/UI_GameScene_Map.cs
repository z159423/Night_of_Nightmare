using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Linq;

public class UI_GameScene_Map : UI_Scene
{
    enum Buttons
    {
        PauseBtn,
        BoostFireBtn,
        BoostShieldBtn,
        BoostHammerBtn,
        RepairBtn,
        CoinBtn,
        EnergyBtn,
        EnemyForceBtn,
        GameWinBtn,
        GameLoseBtn,
        EnemyLevelUp,
        AdTicket
    }

    enum Texts
    {
        GoldText,
        EnergyCount,
        TicketCount,
        RepairCoolTimeText,
        BoostFireCoolTimeText,
        BoostShieldCoolTimeText,
        BoostHammerCoolTimeText,
        BoostFireCountText,
        BoostShieldCountText,
        BoostHammerCountText,
        TutorialText,
        TutorialRewardText
    }

    enum Images
    {
        RepairHpbarMask,
        Tutorial,
        BoostFireRvIcon,
        BoostShieldRvIcon,
        BoostHammerRvIcon,
        TutorialRewardGemImage
    }

    private Transform playerLayout;

    bool _init = false;

    bool canDoorRepair = true;
    bool canFireBoost = true;
    bool canShieldBoost = true;
    bool canHammerBoost = true;
    float repairStartTime;
    float fireStartTime;
    float shieldStartTime;
    float hammerStartTime;

    private RectTransform hpBarRect;
    private TextMeshProUGUI repairCoolTimeText;
    private Transform hpbar;

    [SerializeField] private Sprite[] tutorialSprites;
    private TutorialData currentTutorial;

    private Transform cheatBtns;


    public override void Init()
    {
        base.Init();

        if (!_init)
        {
            FirstSetting();
        }

        this.SetListener(GameObserverType.Game.OnChangeCoinCount, () =>
        {
            GetTextMesh(Texts.GoldText).text = Managers.Game.playerData.coin.ToString();
        });

        this.SetListener(GameObserverType.Game.OnChangeEnergyCount, () =>
        {
            GetTextMesh(Texts.EnergyCount).text = Managers.Game.playerData.energy.ToString();
        });

        this.SetListener(GameObserverType.Game.OnChangeTicketCount, () =>
        {
            GetTextMesh(Texts.TicketCount).text = Managers.LocalData.PlayerRvTicketCount.ToString();
        });

        this.SetListener(GameObserverType.Game.OnChangeBoostItemCount, () =>
        {
            UpdateBoostItemUI();
        });

        UpdateBoostItemUI();


        GetTextMesh(Texts.GoldText).text = Managers.Game.playerData.coin.ToString();
        GetTextMesh(Texts.EnergyCount).text = Managers.Game.playerData.energy.ToString();
        GetTextMesh(Texts.TicketCount).text = Managers.LocalData.PlayerRvTicketCount.ToString();

        StartTutorial();

        this.SetListener(GameObserverType.Game.OnPlayerTutorialActing, () =>
        {
            TutorialCheck();
        });

        GetTextMesh(Texts.RepairCoolTimeText).gameObject.SetActive(false);
        GetButton(Buttons.RepairBtn).GetComponent<Image>().color = Color.white;

        GetTextMesh(Texts.BoostFireCoolTimeText).gameObject.SetActive(false);
        canFireBoost = true;
        GetButton(Buttons.BoostFireBtn).GetComponent<Image>().color = Color.white;

        GetTextMesh(Texts.BoostShieldCoolTimeText).gameObject.SetActive(false);
        canShieldBoost = true;
        GetButton(Buttons.BoostShieldBtn).GetComponent<Image>().color = Color.white;

        GetTextMesh(Texts.BoostHammerCoolTimeText).gameObject.SetActive(false);
        canHammerBoost = true;
        GetButton(Buttons.BoostHammerBtn).GetComponent<Image>().color = Color.white;

        GetTextMesh(Texts.RepairCoolTimeText).gameObject.SetActive(false);
        GetButton(Buttons.RepairBtn).GetComponent<Image>().color = Color.white;

        hpbar.gameObject.SetActive(false);

        this.SetListener(GameObserverType.Game.OnActivePlayerBed, () =>
        {
            hpbar.gameObject.SetActive(true);
        });

    }

    public void FirstSetting()
    {
        _init = true;

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));

        playerLayout = gameObject.FindRecursive("PlayerLayout").transform;

        GetButton(Buttons.PauseBtn).AddButtonEvent(() =>
        {
            Managers.UI.ShowPopupUI<Exit_Popup>();
        });

        hpbar = gameObject.FindRecursive("Hpbar").transform;

        hpBarRect = GetImage(Images.RepairHpbarMask).rectTransform;
        repairCoolTimeText = GetTextMesh(Texts.RepairCoolTimeText);

        GetButton(Buttons.RepairBtn).AddButtonEvent(() =>
        {
            if (Managers.Game.playerData.canDoorRepair && Managers.Game.playerData.room != null && !Managers.Game.playerData.room.door.destroyed)
            {
                StartCoroutine(RepairDoor());
                IEnumerator RepairDoor()
                {
                    if (Managers.Game.playerData.canDoorRepair)
                    {
                        Managers.Game.playerData.SelfRepairDoor();

                        GetButton(Buttons.RepairBtn).GetComponent<Image>().color = new Color32(25, 25, 25, 255);
                        repairStartTime = Time.time; // DateTime.Now 대신 Time.time 사용
                        GetTextMesh(Texts.RepairCoolTimeText).gameObject.SetActive(true);

                        yield return new WaitForSeconds(20);

                        GetTextMesh(Texts.RepairCoolTimeText).gameObject.SetActive(false);
                        GetButton(Buttons.RepairBtn).GetComponent<Image>().color = Color.white;
                    }
                }
            }
        });

        GetButton(Buttons.BoostFireBtn).AddButtonEvent(() =>
        {
            if (canFireBoost)
            {
                if (Managers.LocalData.GetBoostItemCount(Define.BoostType.Overheat) > 0)
                {
                    StartCoroutine(StartFireBoost());
                }
                else
                {
                    Managers.Ad.ShowRewardAd(() =>
                    {
                        StartCoroutine(StartFireBoost(true));
                    });
                }
            }
        });

        GetButton(Buttons.BoostShieldBtn).AddButtonEvent(() =>
        {
            if (canShieldBoost && Managers.Game.playerData.room != null && !Managers.Game.playerData.room.door.destroyed)
            {
                if (Managers.LocalData.GetBoostItemCount(Define.BoostType.HolyProtection) > 0)
                {
                    StartCoroutine(StartShieldBoost());
                }
                else
                {
                    Managers.Ad.ShowRewardAd(() =>
                    {
                        StartCoroutine(StartShieldBoost(true));
                    });
                }
            }
        });

        GetButton(Buttons.BoostHammerBtn).AddButtonEvent(() =>
        {
            if (canHammerBoost && Managers.Game.enemy != null)
            {
                if (Managers.LocalData.GetBoostItemCount(Define.BoostType.HammerThrow) > 0)
                {
                    StartCoroutine(StartHammerBoost());
                }
                else
                {
                    Managers.Ad.ShowRewardAd(() =>
                    {
                        StartCoroutine(StartHammerBoost(true));
                    });
                }
            }
        });

        GetButton(Buttons.CoinBtn).AddButtonEvent(() =>
       {
           Managers.Game.playerData.AddCoin(10000);
       });

        GetButton(Buttons.EnergyBtn).AddButtonEvent(() =>
        {
            Managers.Game.playerData.AddEnergy(10000);
        });

        GetButton(Buttons.EnemyForceBtn).AddButtonEvent(() =>
        {
            Managers.Game.enemy.ForceTargetPlayer();
        });

        GetButton(Buttons.EnemyLevelUp).AddButtonEvent(() =>
        {
            Managers.Game.enemy.LevelUp();
        });

        GetButton(Buttons.GameWinBtn).AddButtonEvent(() =>
        {
            Managers.Game.GameWin();
        });

        GetButton(Buttons.GameLoseBtn).AddButtonEvent(() =>
        {
            Managers.Game.GameOver();
        });

        GetButton(Buttons.AdTicket).AddButtonEvent(() =>
        {
            Managers.LocalData.CheatMode = Managers.LocalData.CheatMode == 0 ? 1 : 0;
            GameObserver.Call(GameObserverType.Game.OnCheatModeOn);
        });

        cheatBtns = gameObject.FindRecursive("CheatBtn").transform;

        this.SetListener(GameObserverType.Game.OnCheatModeOn, () =>
        {
            cheatBtns.gameObject.SetActive(Managers.LocalData.CheatMode == 1);
            GetImage(Images.Tutorial).GetComponent<RectTransform>().anchoredPosition = Managers.LocalData.CheatMode == 0 ? new Vector2(475, -490) : new Vector2(475, -640); // 초기 위치를 화면 밖으로 설정
        });

        cheatBtns.gameObject.SetActive(Managers.LocalData.CheatMode == 1);
        GetImage(Images.Tutorial).GetComponent<RectTransform>().anchoredPosition = Managers.LocalData.CheatMode == 0 ? new Vector2(475, -490) : new Vector2(475, -640); // 초기 위치를 화면 밖으로 설정
    }

    void Update()
    {
        if (Managers.Game.playerData.room != null && !Managers.Game.playerData.room.door.destroyed)
        {
            if (!Managers.Game.playerData.canDoorRepair)
            {
                float elapsedTime = Time.time - repairStartTime; // TimeSpan 계산 대신 단순 뺄셈
                float remainingTime = Mathf.Max(0f, 20f - elapsedTime);
                repairCoolTimeText.text = remainingTime.ToString("F1");
            }

            var maxHp = Managers.Game.playerData.room.door.GetMaxHp();
            var hp = Managers.Game.playerData.room.door.GetHp();

            // width 계산 및 적용
            float ratio = Mathf.Clamp01((float)hp / maxHp);
            float minWidth = 0f;
            float maxWidth = 135f;
            float width = Mathf.Lerp(minWidth, maxWidth, ratio);

            Vector2 size = hpBarRect.sizeDelta;
            size.x = width;
            hpBarRect.sizeDelta = size;
        }

        if (!canFireBoost)
        {
            float elapsedTime = Time.time - fireStartTime;
            float remainingTime = Mathf.Max(0f, 20f - elapsedTime);
            GetTextMesh(Texts.BoostFireCoolTimeText).text = remainingTime.ToString("F1");
        }

        if (!canShieldBoost)
        {
            float elapsedTime = Time.time - shieldStartTime;
            float remainingTime = Mathf.Max(0f, 20f - elapsedTime);
            GetTextMesh(Texts.BoostShieldCoolTimeText).text = remainingTime.ToString("F1");
        }

        if (!canHammerBoost)
        {
            float elapsedTime = Time.time - hammerStartTime;
            float remainingTime = Mathf.Max(0f, 20f - elapsedTime);
            GetTextMesh(Texts.BoostHammerCoolTimeText).text = remainingTime.ToString("F1");
        }
    }

    public void ClearCharactorIcons()
    {
        foreach (Transform child in playerLayout)
        {
            Destroy(child.gameObject);
        }
    }

    public void SetPlayerIcon(PlayerableCharactor player)
    {
        var icon = Managers.Resource.Instantiate("CharactorIcon", playerLayout);

        icon.GetComponent<CharactorIcon>().Setting(player);

        icon.FindRecursive("Icon").GetComponent<Image>().sprite = Managers.Resource.GetCharactorIcons((int)player.charactorType + 1);
    }

    public void SetCharactorIcon(AiCharactor aiCharactor)
    {
        var icon = Managers.Resource.Instantiate("CharactorIcon", playerLayout);
        icon.GetComponent<CharactorIcon>().Setting(aiCharactor);
        icon.FindRecursive("Icon").GetComponent<Image>().sprite = Managers.Resource.GetCharactorIcons((int)aiCharactor.charactorType + 1);
        icon.gameObject.FindRecursive("Arrrow").SetActive(false);
    }

    public void AttackedAnimation(int index)
    {
        playerLayout.GetChild(index).GetComponent<CharactorIcon>().AttackedAnimation();
    }

    IEnumerator StartFireBoost(bool isAd = false)
    {
        if (canFireBoost)
        {
            foreach (var turret in Managers.Game.playerData.structures.Where(n => n is Turret).ToList())
            {
                if (turret != null && turret.GetComponent<Turret>() != null)
                {
                    turret.GetComponent<Turret>().AddEffect(new OverHeat(6));
                }
            }

            Managers.Audio.PlaySound("snd_get", minRangeVolumeMul: -1f);

            if (!isAd)
                Managers.LocalData.AddBoostItem(Define.BoostType.Overheat, -1);
            GetButton(Buttons.BoostFireBtn).GetComponent<Image>().color = new Color32(25, 25, 25, 255);
            canFireBoost = false;
            fireStartTime = Time.time; // DateTime.Now 대신 Time.time 사용
            GetTextMesh(Texts.BoostFireCoolTimeText).gameObject.SetActive(true);

            yield return new WaitForSeconds(20);

            GetTextMesh(Texts.BoostFireCoolTimeText).gameObject.SetActive(false);
            canFireBoost = true;
            GetButton(Buttons.BoostFireBtn).GetComponent<Image>().color = Color.white;
        }
    }

    IEnumerator StartShieldBoost(bool isAd = false)
    {
        if (canShieldBoost)
        {
            Managers.Audio.PlaySound("snd_get", minRangeVolumeMul: -1f);
            Managers.Game.playerData.room.door.AddEffect(new HolyProtection(15));

            if (!isAd)
                Managers.LocalData.AddBoostItem(Define.BoostType.HolyProtection, -1);
            GetButton(Buttons.BoostShieldBtn).GetComponent<Image>().color = new Color32(25, 25, 25, 255);
            canShieldBoost = false;
            shieldStartTime = Time.time; // DateTime.Now 대신 Time.time 사용
            GetTextMesh(Texts.BoostShieldCoolTimeText).gameObject.SetActive(true);

            yield return new WaitForSeconds(20);

            GetTextMesh(Texts.BoostShieldCoolTimeText).gameObject.SetActive(false);
            canShieldBoost = true;
            GetButton(Buttons.BoostShieldBtn).GetComponent<Image>().color = Color.white;
        }
    }

    IEnumerator StartHammerBoost(bool isAd = false)
    {
        if (canHammerBoost)
        {
            Managers.Audio.PlaySound("snd_get", minRangeVolumeMul: -1f);
            StartCoroutine(FireHammerBullet(Managers.Game.enemy));
            if (!isAd)
                Managers.LocalData.AddBoostItem(Define.BoostType.HammerThrow, -1);
            GetButton(Buttons.BoostHammerBtn).GetComponent<Image>().color = new Color32(25, 25, 25, 255);
            canHammerBoost = false;
            hammerStartTime = Time.time; // DateTime.Now 대신 Time.time 사용
            GetTextMesh(Texts.BoostHammerCoolTimeText).gameObject.SetActive(true);

            yield return new WaitForSeconds(20);

            GetTextMesh(Texts.BoostHammerCoolTimeText).gameObject.SetActive(false);
            canHammerBoost = true;
            GetButton(Buttons.BoostHammerBtn).GetComponent<Image>().color = Color.white;
        }
    }

    private IEnumerator FireHammerBullet(Enemy target)
    {
        if (target == null || !target.gameObject.activeInHierarchy)
            yield break;

        Managers.Audio.PlaySound("snd_get", minRangeVolumeMul: -1f);
        GameObject hammer = Managers.Resource.Instantiate("HammerBullet");
        Vector3 startPos = target.transform.position + new Vector3(0, 6f, 0);
        hammer.transform.position = startPos;

        float speed = 7f;
        float slowSpeed = 1.5f;
        float minDistance = 1f;

        bool hitted = false;

        float spinInterval = 0.8f;
        float spinTimer = 0f;

        while (true)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                Managers.Resource.Destroy(hammer);
                yield break;
            }

            Vector3 targetPos = target.transform.position;
            Vector3 bulletPos = hammer.transform.position;

            float distance = Mathf.Abs(bulletPos.y - targetPos.y);

            // target과의 y축 거리가 1.5 이하이면 속도 절반
            float moveSpeed = distance <= 2f ? slowSpeed : speed;

            // y축만 감소시켜서 target을 향해 이동
            float newY = Mathf.MoveTowards(bulletPos.y, targetPos.y, moveSpeed * Time.deltaTime);
            hammer.transform.position = new Vector3(targetPos.x, newY, bulletPos.z);

            // 0.8초마다 snd_sword_swing 재생
            spinTimer += Time.deltaTime;
            if (spinTimer >= spinInterval)
            {
                Managers.Audio.PlaySound("snd_sword_swing", minRangeVolumeMul: -1f);
                spinTimer = 0f;
            }

            if (!hitted && Mathf.Abs(newY - targetPos.y) < 2f)
            {
                hitted = true;

                int damage = Mathf.RoundToInt(target.MaxHp * 0.12f);
                target.Hit(damage, false);
                target.AddEffect(new StunEffect(3f));

                target.hammerThrowParticle.GetComponent<ParticleSystem>().Play();

                Managers.Audio.PlaySound("snd_cutter", target.transform);

                // 1초 동안 hammer의 color를 0으로 페이드 아웃
                var spriteRenderer = hammer.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.DOFade(0f, 1f);
                }
            }

            // 도착 체크
            if (Mathf.Abs(newY - targetPos.y) < minDistance)
            {
                // 도착 시 target에게 최대 체력의 12% 데미지
                Managers.Resource.Destroy(hammer);
                yield break;
            }

            yield return null;
        }
    }

    public void DisableCharactorIcon(AiCharactor aiCharactor)
    {
        playerLayout.GetComponentsInChildren<CharactorIcon>().First(n => n.charactor == aiCharactor).OnDie();
    }

    public void StartTutorial()
    {
        var tutorialdata = Managers.Resource.LoadAll<TutorialData>("TutorialData/")
            .OrderBy(td => int.Parse(System.IO.Path.GetFileNameWithoutExtension(td.name)))
            .ToList();
        if (tutorialdata.Count() > Managers.LocalData.PlayerTutorialStep)
        {
            currentTutorial = tutorialdata[Managers.LocalData.PlayerTutorialStep];

            GetImage(Images.Tutorial).gameObject.SetActive(true);
            GetImage(Images.Tutorial).sprite = tutorialSprites[0];
            GetTextMesh(Texts.TutorialText).text = GetTutorialText(currentTutorial);
            GetTextMesh(Texts.TutorialRewardText).text = currentTutorial.gemRewardCount.ToString();

            TutorialCheck();
        }
        else
        {
            GetImage(Images.Tutorial).gameObject.SetActive(false);
        }
    }

    public void TutorialCheck()
    {
        if (currentTutorial == null)
            return;

        if (Managers.Game.playerData != null && Managers.Game.playerData.room != null && Managers.Game.playerData.room.bed != null)
            switch (currentTutorial.jubjectType)
            {
                case TutorialData.JubjectType.LayBed:
                    if (Managers.Game.playerData.room.bed.active)
                        OnClearTutorial();
                    break;

                case TutorialData.JubjectType.PurchaseStructure:
                    GetTextMesh(Texts.TutorialText).text = GetTutorialText(currentTutorial);

                    if (currentTutorial.purchaseCount <= Managers.Game.playerData.structures.Count(n => n.type == currentTutorial.purchaseType))
                        OnClearTutorial();

                    break;

                case TutorialData.JubjectType.UpgradeStructure:
                    int maxLevel = Managers.Game.playerData.structures.Count > 0
                        ? Managers.Game.playerData.structures.Where(n => n.type == currentTutorial.upgradeType).Max(s => s.level)
                        : 0;
                    if (currentTutorial.upgradeLevel <= maxLevel)
                        OnClearTutorial();
                    break;
            }
    }

    public void OnClearTutorial()
    {
        Managers.LocalData.PlayerGemCount += currentTutorial.gemRewardCount;

        Managers.UI.GenerateUIParticle(GetImage(Images.TutorialRewardGemImage).transform, uiParticleMarkerType.GemIcon, Define.ItemType.Gem);

        currentTutorial = null;
        Managers.LocalData.PlayerTutorialStep++;

        var tutorialCount = Managers.Resource.LoadAll<TutorialData>("TutorialData/").Count();

        if (tutorialCount <= Managers.LocalData.PlayerTutorialStep)
        {
            GetTextMesh(Texts.TutorialText).text = Managers.Localize.GetText("global.str_tuto_desc_12");
        }
        Managers.Audio.PlaySound("snd_get_item");

        StartCoroutine(clear());

        IEnumerator clear()
        {
            GetImage(Images.Tutorial).sprite = tutorialSprites[1];
            yield return new WaitForSeconds(3);

            StartTutorial();
        }
    }

    string GetTutorialText(TutorialData tutorialData)
    {
        switch (tutorialData.jubjectType)
        {
            case TutorialData.JubjectType.LayBed:
                return Managers.Localize.GetText("tutorial_lay_bed");

            case TutorialData.JubjectType.PurchaseStructure:
                var structureData = Managers.Game.GetStructureData(tutorialData.purchaseType);
                int currentCount = Managers.Game.playerData.structures.Count(n => n.type == tutorialData.purchaseType);
                if (tutorialData.purchaseCount == 1)
                    return Managers.Localize.GetDynamicText("tutorial_upgrade", Managers.Localize.GetText(structureData.nameKey));
                else
                    return Managers.Localize.GetDynamicText("tutorial_purchase", Managers.Localize.GetText(structureData.nameKey), currentCount.ToString(), tutorialData.purchaseCount.ToString());

            case TutorialData.JubjectType.UpgradeStructure:
                var structureData2 = Managers.Game.GetStructureData(tutorialData.upgradeType);
                return Managers.Localize.GetDynamicText("tutorial_upgrade", Managers.Localize.GetText(structureData2.nameKey + "_" + (tutorialData.upgradeLevel + 1)));
            default:
                return "";
        }
    }

    public void UpdateBoostItemUI()
    {
        GetTextMesh(Texts.BoostFireCountText).text = Managers.LocalData.GetBoostItemCount(Define.BoostType.Overheat).ToString();
        GetTextMesh(Texts.BoostShieldCountText).text = Managers.LocalData.GetBoostItemCount(Define.BoostType.HolyProtection).ToString();
        GetTextMesh(Texts.BoostHammerCountText).text = Managers.LocalData.GetBoostItemCount(Define.BoostType.HammerThrow).ToString();

        if (Managers.LocalData.GetBoostItemCount(Define.BoostType.Overheat) > 0)
        {
            GetTextMesh(Texts.BoostFireCountText).transform.parent.gameObject.SetActive(true);
            GetImage(Images.BoostFireRvIcon).gameObject.SetActive(false);
        }
        else
        {
            GetTextMesh(Texts.BoostFireCountText).transform.parent.gameObject.SetActive(false);
            GetImage(Images.BoostFireRvIcon).gameObject.SetActive(true);
        }

        if (Managers.LocalData.GetBoostItemCount(Define.BoostType.HolyProtection) > 0)
        {
            GetTextMesh(Texts.BoostShieldCountText).transform.parent.gameObject.SetActive(true);
            GetImage(Images.BoostShieldRvIcon).gameObject.SetActive(false);
        }
        else
        {
            GetTextMesh(Texts.BoostShieldCountText).transform.parent.gameObject.SetActive(false);
            GetImage(Images.BoostShieldRvIcon).gameObject.SetActive(true);
        }

        if (Managers.LocalData.GetBoostItemCount(Define.BoostType.HammerThrow) > 0)
        {
            GetTextMesh(Texts.BoostHammerCountText).transform.parent.gameObject.SetActive(true);
            GetImage(Images.BoostHammerRvIcon).gameObject.SetActive(false);
        }
        else
        {
            GetTextMesh(Texts.BoostHammerCountText).transform.parent.gameObject.SetActive(false);
            GetImage(Images.BoostHammerRvIcon).gameObject.SetActive(true);
        }
    }

    public override void Show()
    {
        gameObject.SetActive(true);
    }

    public override void Hide()
    {
        gameObject.SetActive(false);
        this.RemoveListener(GameObserverType.Game.OnPlayerTutorialActing);
        StopAllCoroutines();
    }
}
