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

    public enum EnemySkillType
    {
        AttackDamageIncrease,
        AttackSpeedIncrease,
        Sickle,
        pierrot,
        MossMan,
        SlanderMan
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

    public Define.EnemyType enemyType;

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

    // Enemy.cs 내부에 추가
    private bool isSkillActive = false;
    private float skillCooldownTimer = 0f;
    private float skillDurationTimer = 0f;
    private EnemySkillType? currentSkill = null;

    [SerializeField] public float skillAttackSpeedMultiplier = 1f; // 공격 속도 배수
    [SerializeField] public float skillMoveSpeedMultiplier = 1f; // 이동 속도 배수 
    [SerializeField] public float skillAttackDamageMultiplier = 1f; // 공격력 배수

    public float baseMoveSpeed = 4f;

    // 임시 난이도 값
    private int difficultyValue = 0; // 원하는 값으로 조정

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

        // agent.speed = baseMoveSpeed;

        skills.Add(new AttackSpeedSkill());
        skills.Add(new AttackDamageSkill()); // 등등

        // switch (Managers.Game.enemyType)
        // {
        //     case Define.EnemyType.Sickle:
        //         enemyType = Define.EnemyType.Sickle;
        //         skills.Add(new SickleSkill());
        //         break;
        //     case Define.EnemyType.Pierrot:
        //         enemyType = Define.EnemyType.Pierrot;
        //         skills.Add(new PierrotSkill());
        //         break;
        //     case Define.EnemyType.MossMan:
        //         enemyType = Define.EnemyType.MossMan;
        //         skills.Add(new MossManSkill());
        //         break;
        //     case Define.EnemyType.SlanderMan:
        //         enemyType = Define.EnemyType.SlanderMan;
        //         skills.Add(new SlanderManSkill());
        //         break;
        // }

        CheckUseSkill();
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
            hp += MaxHp * 0.00030f; // Heal 0.14% of MaxHp per frame

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
                currentTarget.Hit(Mathf.RoundToInt(damage * skillAttackDamageMultiplier));

                if (Managers.UI._currentScene is UI_GameScene_Map gameScene_Map)
                    gameScene_Map.AttackedAnimation(targetIndex);
            };
        }
        else if (currentTargetStructure != null)
        {
            targetPosition = currentTargetStructure.transform.position;
            hitAction = () =>
            {
                currentTargetStructure.Hit(Mathf.RoundToInt(damage * skillAttackDamageMultiplier));

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

            yield return body.DOLocalMove(dashPosition, 0.1f / skillAttackSpeedMultiplier).SetEase(Ease.Linear).WaitForCompletion();

            hitAction.Invoke();

            yield return body.DOLocalMove(originalPosition, 0.1f / skillAttackSpeedMultiplier).SetEase(Ease.Linear).WaitForCompletion();
        }

        yield return new WaitForSeconds(1.2f / skillAttackSpeedMultiplier);

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

    public List<EnemySkill> skills = new List<EnemySkill>();

    private Coroutine _checkSkillCoroutine;

    public void CheckUseSkill()
    {
        // 1초마다 체크하도록 코루틴 시작 (Start 등에서 호출)
        if (_checkSkillCoroutine == null)
            _checkSkillCoroutine = StartCoroutine(CheckSkillRoutine());
    }

    private IEnumerator CheckSkillRoutine()
    {
        while (true)
        {
            // 하나라도 스킬이 사용 중이면 아무것도 하지 않음
            if (skills.Any(skill => skill.IsActive))
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            // Attack이 가능할 정도로 가까울 때만 스킬 사용 (currentTarget 기준)
            bool canAttackRange = false;
            if (currentTarget != null && currentTarget.playerData.room == null)
            {
                float distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);
                if (distanceToTarget < 1f) // 기존 공격 가능 거리와 동일하게
                    canAttackRange = true;
            }
            else if (currentTargetStructure != null)
            {
                float distanceToTarget = Vector2.Distance(transform.position, currentTargetStructure.transform.position);
                if (distanceToTarget < 1f)
                    canAttackRange = true;
            }

            if (!canAttackRange)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            foreach (var skill in skills)
            {
                if (skill.CanUse(this))
                {
                    skill.Activate(this);
                    StartCoroutine(SkillDurationRoutine(skill));
                    break; // 한 번에 하나만 사용
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator SkillDurationRoutine(EnemySkill skill)
    {
        float timer = 0f;
        while (timer < skill.Duration)
        {
            if (!skill.IsActive) // 중간에 꺼졌으면 즉시 종료
                yield break;
            timer += Time.deltaTime;
            yield return null;
        }
        skill.Deactivate(this);
    }
}

public abstract class EnemySkill
{
    public string Name;
    public int MinLevel;
    public float Cooldown;
    public float Duration;
    public float LastUseTime = -999f;
    public bool IsActive { get; protected set; }

    public abstract bool CanUse(Enemy enemy);
    public abstract void Activate(Enemy enemy);
    public abstract void Deactivate(Enemy enemy);
}

public class AttackSpeedSkill : EnemySkill
{
    public AttackSpeedSkill()
    {
        Name = "AttackSpeed";
        MinLevel = 3;
        Cooldown = 15f; // 기본값, 실제 쿨타임은 난이도에 따라 계산
        Duration = 4f;
    }

    public override bool CanUse(Enemy enemy)
    {
        return enemy.level >= MinLevel && !IsActive && Time.time - LastUseTime >= Cooldown;
    }

    public override void Activate(Enemy enemy)
    {
        IsActive = true;
        LastUseTime = Time.time;
        enemy.skillAttackSpeedMultiplier = 2.4f;
        enemy.skillMoveSpeedMultiplier = 2f;
        // 기타 효과

        var popup = Managers.Resource.Instantiate("Notification_Popup", Managers.UI.Root.transform);
        popup.GetComponent<Notification_Popup>().Init();
        popup.GetComponent<Notification_Popup>().Setting(Managers.Localize.GetText("global.str_toast_enemy_spd_skill"));

        //근처에 spellBlocker가 있는지 확인

        Managers.Game.charactors
            .Where(n => n.currentActiveRoom != null && n.playerData != null)
            .ToList()
            .ForEach(n =>
            {
                var spellBlocker = n.playerData.structures
                    .FirstOrDefault(s => s.type == Define.StructureType.SpellBlocker && !s.destroyed && Vector2.Distance(s.transform.position, enemy.transform.position) < 5f);
                if (spellBlocker != null && spellBlocker.TryGetComponent<SpellBlocker>(out var sb))
                {
                    sb.TryCastSpellBlock(enemy, () => Deactivate(enemy));
                }
            });
    }

    public override void Deactivate(Enemy enemy)
    {
        IsActive = false;
        enemy.skillAttackSpeedMultiplier = 1f;
        enemy.skillMoveSpeedMultiplier = 1f;
    }
}

public class AttackDamageSkill : EnemySkill
{
    public AttackDamageSkill()
    {
        Name = "AttackDamage";
        MinLevel = 5;
        Cooldown = 20f; // 기본값, 실제 쿨타임은 난이도에 따라 계산
        Duration = 7f;
    }

    public override bool CanUse(Enemy enemy)
    {
        return enemy.level >= MinLevel && !IsActive && Time.time - LastUseTime >= Cooldown;
    }

    public override void Activate(Enemy enemy)
    {
        IsActive = true;
        LastUseTime = Time.time;
        enemy.skillAttackDamageMultiplier = 1.3f;
        // 타격 사운드 변경 등 추가 효과는 필요시 구현

        var popup = Managers.Resource.Instantiate("Notification_Popup", Managers.UI.Root.transform);
        popup.GetComponent<Notification_Popup>().Init();
        popup.GetComponent<Notification_Popup>().Setting(Managers.Localize.GetText("global.str_toast_enemy_dmg_skill"));
    }

    public override void Deactivate(Enemy enemy)
    {
        IsActive = false;
        enemy.skillAttackDamageMultiplier = 1f;
    }
}
