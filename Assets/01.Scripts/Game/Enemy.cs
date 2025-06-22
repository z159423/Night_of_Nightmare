using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;
using TMPro;
using UnityEngine.UI;
using VInspector;
using static EnemySkill;
using static EnemyEffect;

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

    public List<EnemySkill> skills = new List<EnemySkill>();

    private Coroutine _checkSkillCoroutine;

    // 능력치 객체화
    [SerializeField] public StatMultiplier attackSpeed = new StatMultiplier();
    [SerializeField] public StatMultiplier attackPower = new StatMultiplier();
    [SerializeField] public StatMultiplier moveSpeed = new StatMultiplier();

    public List<EnemyEffect> activeEffects = new List<EnemyEffect>();

    public float baseMoveSpeed = 4f;

    public Transform bleedParticle;
    public Transform stunParticle;
    public Transform poisonParticle;
    public Transform mirrorParticle;
    public Transform creepylaughterParticle;


    public Transform sickle;
    public bool activeSickle = false;

    // Implementation of the abstract Hit() method from Charactor

    public override void Hit(int damage)
    {

    }
    public void Hit(int damage, bool particle = true)
    {
        hp -= damage;
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

        if (particle)
            StartCoroutine(Particle());

        IEnumerator Particle()
        {
            var particle = Managers.Resource.Instantiate("Particles/SmokeParticle2");

            particle.transform.position = transform.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), -1);

            yield return new WaitForSeconds(1f);

            Managers.Resource.Destroy(particle);
        }
    }

    public void Setting(Define.EnemyType type)
    {
        enemyType = type;
        
        SetBodySkin();

        transform.localPosition = Vector3.zero;

        StartCoroutine(EnemyStateMachine());

        GetComponentInParent<NavMeshAgent>(true).enabled = true;

        hpBarPivot = gameObject.FindRecursive("Pivot").transform;
        hpBarFill = gameObject.FindRecursive("Fill").transform;
        levelText = gameObject.FindRecursive("LevelText").GetComponent<TextMeshPro>();
        nameText = gameObject.FindRecursive("NameText").GetComponent<TextMeshPro>();

        bleedParticle = gameObject.FindRecursive("BleedParticle").transform;
        stunParticle = gameObject.FindRecursive("StunParticle").transform;
        poisonParticle = gameObject.FindRecursive("PoisonParticle").transform;
        mirrorParticle = gameObject.FindRecursive("MirrorParticle").transform;
        creepylaughterParticle = gameObject.FindRecursive("CreepylaughterParticle").transform;

        SetNameText(Managers.Game.enemyName);

        MaxHp = Define.GetEnemyMaxHp(enemyType, level);
        hp = MaxHp;

        // 능력치 초기화
        attackSpeed.BaseValue = 1f;
        attackPower.BaseValue = Define.GetEnemyDamage(enemyType, level);
        moveSpeed.BaseValue = baseMoveSpeed;

        skills.Add(new AttackSpeedSkill());
        skills.Add(new AttackDamageSkill()); // 등등
        skills.Add(new Creepylaughter()); // 등등


        sickle = gameObject.FindRecursive("Sickle").transform;

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
        UpdateEffects();
        if (gameObject == null)
            return;

        // Stun 상태면 이동 및 행동 금지
        if (IsStunned)
        {
            // NavMeshAgent 즉시 멈춤
            if (agent != null)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            return;
        }

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
            {
                agent.SetDestination(currentTarget.transform.position);
            }
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
            if (IsStunned)
            {
                // NavMeshAgent 즉시 멈춤
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                yield return new WaitForSeconds(0.2f);
                continue;
            }

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

                        if (enemyType == Define.EnemyType.ScareCrow && activeSickle)
                        {
                            DeactiveSicle();
                        }
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
        if (IsStunned)
            yield break;

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
                currentTarget.Hit(Mathf.RoundToInt(attackPower.Value));

                if (Managers.UI._currentScene is UI_GameScene_Map gameScene_Map)
                    gameScene_Map.AttackedAnimation(targetIndex);
            };
        }
        else if (currentTargetStructure != null)
        {
            targetPosition = currentTargetStructure.transform.position;
            hitAction = () =>
            {
                currentTargetStructure.Hit(Mathf.RoundToInt(attackPower.Value));

                if (currentExp >= Define.enemyExp[level])
                {
                    currentExp = 0;
                    LevelUp();
                }

                if (currentTargetStructure is Door door)
                {
                    if (door.playerData.structures.Any(n => n.type == Define.StructureType.Cooler && !n.destroyed))
                        AddEffect(new FreezeEffect(3f));

                    if (door.playerData.structures.Any(n => n.type == Define.StructureType.ThornBush && !n.destroyed))
                        AddEffect(new BleedEffect(3f));
                }


                if (Managers.UI._currentScene is UI_GameScene_Map gameScene_Map)
                    gameScene_Map.AttackedAnimation(targetIndex);
            };
        }

        if (hitAction != null)
        {
            Vector3 targetDirection = (targetPosition - body.position).normalized;
            Vector3 dashPosition = body.localPosition + targetDirection * 1.75f;

            yield return body.DOLocalMove(dashPosition, 0.1f / attackSpeed.Value).SetEase(Ease.Linear).WaitForCompletion();

            hitAction.Invoke();

            yield return body.DOLocalMove(originalPosition, 0.1f / attackSpeed.Value).SetEase(Ease.Linear).WaitForCompletion();
        }

        yield return new WaitForSeconds(1.2f / attackSpeed.Value);

        canAttack = true;
    }

    [Button("Level Up")]
    public void LevelUp()
    {
        level++;
        levelText.text = "Lv." + (level + 1);

        float prevMaxHp = MaxHp;
        MaxHp = Define.GetEnemyMaxHp(enemyType, level);
        hp += MaxHp - prevMaxHp;
        hp = Mathf.Clamp(hp, 0, MaxHp);

        attackPower.BaseValue = Define.GetEnemyDamage(enemyType, level);

        hpBarPivot.localScale = new Vector3(hp / MaxHp, 1, 1);

        var popup = Managers.Resource.Instantiate("Notification_Popup", Managers.UI.Root.transform);
        popup.GetComponent<Notification_Popup>().Init();
        popup.GetComponent<Notification_Popup>().Setting(Managers.Localize.GetText("global.str_toast_enemy_level_up"));

        if (enemyType == Define.EnemyType.ScareCrow && level > 4)
        {
            ActiveSicle();
        }
    }

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
            // Stun 상태면 스킬 사용 불가
            if (IsStunned)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

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


    public void AddEffect(EnemyEffect newEffect)
    {
        // 이미 같은 효과가 있으면 지속시간만 초기화
        var exist = activeEffects.FirstOrDefault(e => e.GetType() == newEffect.GetType());
        if (exist != null)
        {
            exist.Duration = newEffect.Duration;
            exist.elapsedTime = 0f;
            // 기존 효과가 비활성화 상태라면 다시 활성화
            if (!exist.IsActive)
                exist.Apply(this);
        }
        else
        {
            newEffect.Apply(this);
            activeEffects.Add(newEffect);
        }
    }

    // Enemy.Update() 등에서 호출
    private void UpdateEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].Tick(this, Time.deltaTime);
            if (!activeEffects[i].IsActive)
                activeEffects.RemoveAt(i);
        }
    }

    // Stun 상태 확인용 프로퍼티 추가
    public bool IsStunned => activeEffects.Any(e => e is StunEffect && e.IsActive);

    public void ActiveSicle()
    {
        activeSickle = true;

        if (sickle != null)
        {
            sickle.gameObject.SetActive(true);
        }

        attackPower.AddMultiplier(1.5f);
    }

    public void DeactiveSicle()
    {
        activeSickle = false;

        if (sickle != null)
        {
            sickle.gameObject.SetActive(false);
        }

        attackPower.RemoveMultiplier(1.5f);
    }
}

[System.Serializable]
public class StatMultiplier
{
    private List<float> multipliers = new List<float>();

    public float BaseValue { get; set; } = 1f;

    public float Value
    {
        get
        {
            float result = BaseValue;
            foreach (var m in multipliers)
                result *= m;
            return result;
        }
    }

    public void AddMultiplier(float multiplier)
    {
        multipliers.Add(multiplier);
    }

    public void RemoveMultiplier(float multiplier)
    {
        multipliers.Remove(multiplier);
    }

    public void ClearMultipliers()
    {
        multipliers.Clear();
    }
}
