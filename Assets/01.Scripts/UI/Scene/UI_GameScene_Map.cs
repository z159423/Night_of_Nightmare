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
        RepairBtn
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
        BoostHammerCountText
    }

    enum Images
    {
        RepairHpbarMask,
    }

    private Transform playerLayout;

    bool _init = false;

    bool canDoorRepair = true;
    bool canFireBoost = true;
    bool canShieldBoost = true;
    bool canHammerBoost = true;
    DateTime repairStartTime;
    DateTime fireStartTime;
    DateTime shieldStartTime;
    DateTime hammerStartTime;

    private RectTransform hpBarRect;
    private TextMeshProUGUI repairCoolTimeText;
    private Transform hpbar;


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
            GetTextMesh(Texts.TicketCount).text = Managers.LocalData.PlayerTicketCount.ToString();
        });

        this.SetListener(GameObserverType.Game.OnChangeBoostItemCount, () =>
        {
            GetTextMesh(Texts.BoostFireCountText).text = Managers.LocalData.GetBoostItemCount(Define.BoostType.Overheat).ToString();
            GetTextMesh(Texts.BoostShieldCountText).text = Managers.LocalData.GetBoostItemCount(Define.BoostType.HolyProtection).ToString();
            GetTextMesh(Texts.BoostHammerCountText).text = Managers.LocalData.GetBoostItemCount(Define.BoostType.HammerThrow).ToString();
        });

        GetTextMesh(Texts.BoostFireCountText).text = Managers.LocalData.GetBoostItemCount(Define.BoostType.Overheat).ToString();
        GetTextMesh(Texts.BoostShieldCountText).text = Managers.LocalData.GetBoostItemCount(Define.BoostType.HolyProtection).ToString();
        GetTextMesh(Texts.BoostHammerCountText).text = Managers.LocalData.GetBoostItemCount(Define.BoostType.HammerThrow).ToString();

        GetTextMesh(Texts.GoldText).text = Managers.Game.playerData.coin.ToString();
        GetTextMesh(Texts.EnergyCount).text = Managers.Game.playerData.energy.ToString();
        GetTextMesh(Texts.TicketCount).text = Managers.LocalData.PlayerTicketCount.ToString();
    }

    public void FirstSetting()
    {
        _init = true;

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));

        playerLayout = gameObject.FindRecursive("PlayerLayout").transform;

        GetButton(Buttons.PauseBtn).onClick.AddListener(() =>
        {
            Managers.UI.ShowPopupUI<Exit_Popup>();
        });

        hpbar = gameObject.FindRecursive("Hpbar").transform;

        hpBarRect = GetImage(Images.RepairHpbarMask).rectTransform;
        repairCoolTimeText = GetTextMesh(Texts.RepairCoolTimeText);

        GetButton(Buttons.RepairBtn).onClick.AddListener(() =>
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
                        repairStartTime = DateTime.Now;
                        GetTextMesh(Texts.RepairCoolTimeText).gameObject.SetActive(true);
                        hpbar.gameObject.SetActive(true);

                        yield return new WaitForSeconds(20);

                        GetTextMesh(Texts.RepairCoolTimeText).gameObject.SetActive(false);
                        hpbar.gameObject.SetActive(false);
                        GetButton(Buttons.RepairBtn).GetComponent<Image>().color = Color.white;
                    }
                }
            }
        });

        GetButton(Buttons.BoostFireBtn).onClick.AddListener(() =>
        {
            if (canFireBoost && Managers.LocalData.GetBoostItemCount(Define.BoostType.Overheat) > 0)
                StartCoroutine(StartFireBoost());
        });

        GetButton(Buttons.BoostShieldBtn).onClick.AddListener(() =>
        {
            if (canShieldBoost && Managers.Game.playerData.room != null && !Managers.Game.playerData.room.door.destroyed && Managers.LocalData.GetBoostItemCount(Define.BoostType.HolyProtection) > 0)
                StartCoroutine(StartShieldBoost());
        });

        GetButton(Buttons.BoostHammerBtn).onClick.AddListener(() =>
        {
            if (canHammerBoost && Managers.Game.enemy != null && Managers.LocalData.GetBoostItemCount(Define.BoostType.HammerThrow) > 0)
                StartCoroutine(StartHammerBoost());
        });
    }

    void Update()
    {
        if (!Managers.Game.playerData.canDoorRepair && Managers.Game.playerData.room != null && !Managers.Game.playerData.room.door.destroyed)
        {
            TimeSpan timeSpan = DateTime.Now - repairStartTime;
            float remainingTime = Mathf.Max(0f, 20f - (float)timeSpan.TotalSeconds);
            repairCoolTimeText.text = remainingTime.ToString("F1");

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
            TimeSpan timeSpan = DateTime.Now - fireStartTime;
            float remainingTime = Mathf.Max(0f, 20f - (float)timeSpan.TotalSeconds);
            GetTextMesh(Texts.BoostFireCoolTimeText).text = remainingTime.ToString("F1");
        }

        if (!canShieldBoost)
        {
            TimeSpan timeSpan = DateTime.Now - shieldStartTime;
            float remainingTime = Mathf.Max(0f, 20f - (float)timeSpan.TotalSeconds);
            GetTextMesh(Texts.BoostShieldCoolTimeText).text = remainingTime.ToString("F1");
        }

        if (!canHammerBoost)
        {
            TimeSpan timeSpan = DateTime.Now - hammerStartTime;
            float remainingTime = Mathf.Max(0f, 20f - (float)timeSpan.TotalSeconds);
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

    public void SetPlayerIcon(Define.CharactorType type)
    {
        var icon = Managers.Resource.Instantiate("CharactorIcon", playerLayout);

        icon.FindRecursive("Icon").GetComponent<Image>().sprite = Managers.Resource.GetCharactorIcons((int)type + 1);
    }

    public void SetCharactorIcon(Define.CharactorType type, AiCharactor aiCharactor)
    {
        var icon = Managers.Resource.Instantiate("CharactorIcon", playerLayout);
        icon.GetComponent<CharactorIcon>().charactor = aiCharactor;
        icon.FindRecursive("Icon").GetComponent<Image>().sprite = Managers.Resource.GetCharactorIcons((int)type + 1);
        icon.gameObject.FindRecursive("Arrrow").SetActive(false);
    }

    public void AttackedAnimation(int index)
    {
        playerLayout.GetChild(index).GetComponent<CharactorIcon>().AttackedAnimation();
    }

    IEnumerator StartFireBoost()
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

            Managers.LocalData.AddBoostItem(Define.BoostType.Overheat, -1);
            GetButton(Buttons.BoostFireBtn).GetComponent<Image>().color = new Color32(25, 25, 25, 255);
            canFireBoost = false;
            fireStartTime = DateTime.Now;
            GetTextMesh(Texts.BoostFireCoolTimeText).gameObject.SetActive(true);

            yield return new WaitForSeconds(20);

            GetTextMesh(Texts.BoostFireCoolTimeText).gameObject.SetActive(false);
            canFireBoost = true;
            GetButton(Buttons.BoostFireBtn).GetComponent<Image>().color = Color.white;
        }
    }

    IEnumerator StartShieldBoost()
    {
        if (canShieldBoost)
        {
            Managers.Game.playerData.room.door.AddEffect(new HolyProtection(15));

            Managers.LocalData.AddBoostItem(Define.BoostType.HolyProtection, -1);
            GetButton(Buttons.BoostShieldBtn).GetComponent<Image>().color = new Color32(25, 25, 25, 255);
            canShieldBoost = false;
            shieldStartTime = DateTime.Now;
            GetTextMesh(Texts.BoostShieldCoolTimeText).gameObject.SetActive(true);

            yield return new WaitForSeconds(20);

            GetTextMesh(Texts.BoostShieldCoolTimeText).gameObject.SetActive(false);
            canShieldBoost = true;
            GetButton(Buttons.BoostShieldBtn).GetComponent<Image>().color = Color.white;
        }
    }

    IEnumerator StartHammerBoost()
    {
        if (canHammerBoost)
        {
            StartCoroutine(FireHammerBullet(Managers.Game.enemy));

            Managers.LocalData.AddBoostItem(Define.BoostType.HammerThrow, -1);
            GetButton(Buttons.BoostHammerBtn).GetComponent<Image>().color = new Color32(25, 25, 25, 255);
            canHammerBoost = false;
            hammerStartTime = DateTime.Now;
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

        GameObject hammer = Managers.Resource.Instantiate("HammerBullet");
        Vector3 startPos = target.transform.position + new Vector3(0, 6f, 0);
        hammer.transform.position = startPos;

        float speed = 7f;
        float slowSpeed = 1.5f;
        float minDistance = 1f;

        bool hitted = false;

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

            if (!hitted && Mathf.Abs(newY - targetPos.y) < 2f)
            {
                hitted = true;

                int damage = Mathf.RoundToInt(target.MaxHp * 0.12f);
                target.Hit(damage, false);
                target.AddEffect(new StunEffect(3f));

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

    public override void Show()
    {
        gameObject.SetActive(true);
    }

    public override void Hide()
    {
        gameObject.SetActive(false);

        StopAllCoroutines();
    }
}
