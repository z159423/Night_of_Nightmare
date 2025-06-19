using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;
using TMPro;
using UnityEngine.UI;
using VInspector;

public class Enemy : Charactor
{
    public enum EnemyState
    {
        Chase,
        Heal
    }

    public float MaxHp = 100;
    public float hp = 100;
    public int level = 0;
    public int currentExp = 0;
    public int damage = 0;

    public PlayerableCharactor currentTarget;
    public Structure currentTargetStructure;
    public int targetIndex;
    public HealZone targetHealZone;
    EnemyState enemyState = EnemyState.Chase;

    public bool canAttack = true;

    private int changeStateTime = 0;
    private float currentChangeStateTime = 0;

    private float checkHpTime = 0;
    private float currentCheckHpTime = 0;

    Transform hpBarPivot;
    Transform hpBarFill;
    TextMeshPro levelText;
    TextMeshPro nameText;

    bool isHitColorEffectRunning = false;

    // Implementation of the abstract Hit() method from Charactor
    public override void Hit(int damage)
    {
        // Provide your logic here, for example:
        hp -= damage; // Default damage
        if (hp <= 0)
        {
            // Handle enemy death

            Managers.Game.GameWin();
        }

        // Show HP bar
        if (hpBarPivot != null)
        {
            hpBarPivot.localScale = new Vector3(hp / MaxHp, 1, 1);
        }

        // Change sprite color to red and smoothly transition back to white using DOTween
        if (bodySpriteRenderer != null && !isHitColorEffectRunning)
        {
            isHitColorEffectRunning = true;
            bodySpriteRenderer.color = Color.red;
            bodySpriteRenderer.DOColor(Color.white, 0.5f).OnComplete(() => isHitColorEffectRunning = false);
        }

        StartCoroutine(Particle());

        IEnumerator Particle()
        {
            var particle = Managers.Resource.Instantiate("Particles/SmokeParticle2");

            particle.transform.position = transform.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), -1);

            yield return new WaitForSeconds(1f);

            Managers.Resource.Destroy(particle);
        }
    }

    public void Setting()
    {
        SetBodySkin();

        transform.localPosition = Vector3.zero;

        StartCoroutine(EnemyStateMachine());

        GetComponentInParent<NavMeshAgent>(true).enabled = true;

        hpBarPivot = gameObject.FindRecursive("Pivot").transform;
        hpBarFill = gameObject.FindRecursive("Fill").transform;
        levelText = gameObject.FindRecursive("LevelText").GetComponent<TextMeshPro>();
        nameText = gameObject.FindRecursive("NameText").GetComponent<TextMeshPro>();

        SetNameText(Managers.Game.enemyName);

        MaxHp = Define.enemyHp[level];
        hp = MaxHp;
        damage = Define.enemyDamage[level];
    }

    public void SetNameText(string name)
    {
        if (nameText != null)
        {
            nameText.text = name;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (gameObject == null)
            return;

        if (enemyState == EnemyState.Heal && targetHealZone != null && Vector2.Distance(targetHealZone.transform.position, transform.position) < 1.5f)
        {
            hp += MaxHp * 0.00014f; // Heal 0.14% of MaxHp per frame

            hp = Mathf.Clamp(hp, 0, MaxHp); // Ensure hp does not exceed MaxHp

            if (hp >= MaxHp)
            {
                hp = MaxHp;
                targetHealZone = null; // Stop healing when at max HP
                enemyState = EnemyState.Chase; // Switch back to chasing state
            }

            if (hpBarPivot != null)
            {
                hpBarPivot.localScale = new Vector3(hp / MaxHp, 1, 1);
            }
        }
        else if (currentTarget != null && currentTarget.playerData.room == null && enemyState == EnemyState.Chase)
        {
            if (agent != null && agent.isOnNavMesh)
                agent.SetDestination(currentTarget.transform.position);
        }
        else if (currentTargetStructure != null && enemyState == EnemyState.Chase)
        {
            // Move towards the target
            if (agent != null && agent.isOnNavMesh)
                agent.SetDestination(currentTargetStructure.transform.position);
        }

        if (enemyState != EnemyState.Heal)
        {
            currentChangeStateTime += Time.deltaTime;
            currentCheckHpTime += Time.deltaTime;
        }

        if (agent != null)
        {
            // agent가 경로를 따라 이동 중이면 OnMove 호출
            if (agent.hasPath && agent.remainingDistance > agent.stoppingDistance)
            {
                OnMove();
            }
            // 목적지에 도달했거나 이동 중이 아니면 OnMoveStop 호출
            else
            {
                OnMoveStop();
            }
        }
    }

    public IEnumerator EnemyStateMachine()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (enemyState != EnemyState.Heal)
                if (hp < MaxHp * 0.2f && currentCheckHpTime > checkHpTime)
                {
                    currentCheckHpTime = 0;
                    FindHealSpot();

                    if (targetHealZone != null)
                    {
                        // Move towards the heal zone
                        agent.SetDestination(targetHealZone.transform.position);
                        enemyState = EnemyState.Heal;
                    }
                }
                else if (currentTarget != null && currentTarget.playerData.room == null)
                {
                    float distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);

                    if (canAttack && distanceToTarget < 1f) // Example attack range
                    {
                        StartCoroutine(Attack());
                    }

                    if (currentCheckHpTime > checkHpTime)
                        checkHpTime = Random.Range(0.5f, 4f);
                }
                else if (Managers.Game.charactors.Where(n => !n.die && n.currentActiveRoom == null).Where(n => Vector2.Distance(n.transform.position, transform.position) <= 2.5f)
            .ToList().Count > 0)
                {
                    currentTarget = Managers.Game.charactors.Where(n => !n.die && n.currentActiveRoom == null)
                        .Where(n => Vector2.Distance(n.transform.position, transform.position) <= 2.5f).ToList().First();

                    currentTargetStructure = null;
                }
                else if (currentTarget != null && currentTarget.playerData.room != null && changeStateTime > currentChangeStateTime)
                {
                    if (currentTargetStructure == null || currentTargetStructure.destroyed)
                    {
                        if (currentTarget != null && !currentTarget.die)
                            currentTargetStructure = currentTarget.currentActiveRoom.GetAttackableStructure(transform.position);
                        else
                            FindTarget();
                    }
                    else
                    {
                        // Check if the target is within attack range
                        float distanceToTarget = Vector2.Distance(transform.position, currentTargetStructure.transform.position);

                        if (canAttack && distanceToTarget < 1f) // Example attack range
                        {
                            StartCoroutine(Attack());
                        }

                        if (currentCheckHpTime > checkHpTime)
                            checkHpTime = Random.Range(0.5f, 4f);
                    }
                }
                else
                {
                    FindTarget();

                    currentChangeStateTime = 0;

                    changeStateTime = Random.Range(20, 45);
                }

            yield return new WaitForSeconds(0.2f); // Adjust the frequency of state checks
        }
    }

    public override void SetBodySkin()
    {
        if (bodySpriteRenderer == null)
        {
            bodySpriteRenderer = gameObject.FindRecursive("Icon").GetComponent<SpriteRenderer>();
        }
        bodySpriteRenderer.sprite = Managers.Resource.GetEnemyImage((int)Managers.Game.enemyType + 1);
    }

    public void FindTarget()
    {
        List<PlayerableCharactor> players = new List<PlayerableCharactor>();
        // List<PlayerableCharactor> noBedPlayers = new List<PlayerableCharactor>();

        // noBedPlayers.AddRange(Managers.Game.charactors.Where(n => !n.die && n.currentActiveRoom == null));
        players.AddRange(Managers.Game.charactors.Where(n => !n.die && n.currentActiveRoom != null));

        // noBedPlayers 중에서 transform과의 거리가 150px 이하인 것만 필터링
        // var closeNoBedPlayers = noBedPlayers
        //     .Where(n => Vector2.Distance(n.transform.position, transform.position) <= 2.5f)
        //     .ToList();

        // if (closeNoBedPlayers.Count > 0)
        // {
        //     int randomIndex = Random.Range(0, closeNoBedPlayers.Count);
        //     currentTarget = closeNoBedPlayers[randomIndex];

        //     targetIndex = Managers.Game.charactors.IndexOf(currentTarget);

        //     currentTargetStructure = null; // 침대가 없으므로 구조물 없음
        // }
        // else
        if (players.Count > 0)
        {
            int randomIndex = Random.Range(0, players.Count);
            currentTarget = players[randomIndex];

            targetIndex = Managers.Game.charactors.IndexOf(currentTarget);

            currentTargetStructure = currentTarget.currentActiveRoom.GetAttackableStructure(transform.position);
        }
        else
        {
            currentTarget = null;
            currentTargetStructure = null;
        }
    }

    [Button("Force Target Player")]
    public void ForceTargetPlayer()
    {
        targetIndex = Managers.Game.charactors.IndexOf(Managers.Game.playerCharactor);
        currentTarget = Managers.Game.playerCharactor;
        currentTargetStructure = currentTarget.currentActiveRoom.GetAttackableStructure(transform.position);
    }

    public void FindHealSpot()
    {
        List<HealZone> healZones = Managers.Game.currentMap.healZones.ToList();

        if (healZones.Count > 0)
        {
            int randomIndex = Random.Range(0, healZones.Count);
            targetHealZone = healZones[randomIndex];
        }
        else
        {
            targetHealZone = null;
        }
    }

    public IEnumerator Attack()
    {
        canAttack = false;
        currentExp++;

        Vector3 originalPosition = body.localPosition;
        Vector3 targetPosition = Vector3.zero;
        System.Action hitAction = null;

        if (currentTarget != null && currentTarget.playerData.room == null)
        {
            targetPosition = currentTarget.transform.position;
            hitAction = () =>
            {
                currentTarget.Hit(damage);

                if (Managers.UI._currentScene is UI_GameScene_Map gameScene_Map)
                    gameScene_Map.AttackedAnimation(targetIndex);
            };
        }
        else if (currentTargetStructure != null)
        {
            targetPosition = currentTargetStructure.transform.position;
            hitAction = () =>
            {
                currentTargetStructure.Hit(damage);

                if (currentExp >= Define.enemyExp[level])
                {
                    currentExp = 0;
                    LevelUp();
                }

                if (Managers.UI._currentScene is UI_GameScene_Map gameScene_Map)
                    gameScene_Map.AttackedAnimation(targetIndex);
            };
        }

        if (hitAction != null)
        {
            Vector3 targetDirection = (targetPosition - body.position).normalized;
            Vector3 dashPosition = body.localPosition + targetDirection * 1.75f;

            yield return body.DOLocalMove(dashPosition, 0.1f).SetEase(Ease.Linear).WaitForCompletion();

            hitAction.Invoke();

            yield return body.DOLocalMove(originalPosition, 0.1f).SetEase(Ease.Linear).WaitForCompletion();
        }

        yield return new WaitForSeconds(1.2f);

        canAttack = true;
    }

    public void LevelUp()
    {
        level++;
        levelText.text = "Lv." + (level + 1);

        float prevMaxHp = MaxHp;
        MaxHp = Define.enemyHp[level];
        hp += MaxHp - prevMaxHp;
        hp = Mathf.Clamp(hp, 0, MaxHp);

        damage = Define.enemyDamage[level];

        hpBarPivot.localScale = new Vector3(hp / MaxHp, 1, 1);

        var popup = Managers.Resource.Instantiate("Notification_Popup", Managers.UI.Root.transform);
        popup.GetComponent<Notification_Popup>().Init();
        popup.GetComponent<Notification_Popup>().Setting(Managers.Localize.GetText("global.str_toast_enemy_level_up"));
    }
}
