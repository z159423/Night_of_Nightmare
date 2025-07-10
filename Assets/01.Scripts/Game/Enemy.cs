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
using System;
using System.Data;
using Unity.VisualScripting;

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
    public Transform currentChaseTarget;

    public Structure currentTargetStructure;
    public int targetIndex;
    public HealZone targetHealZone;
    EnemyState enemyState = EnemyState.Chase;

    public Define.EnemyType enemyType;

    public bool canAttack = true;

    [SerializeField] private int changeStateTime = 0;
    [SerializeField] private float currentChangeStateTime = 0;


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
    public Transform mossManSkillParticle;
    public Transform attackSpeedSkillParticle;
    public Transform attackDamageSkillParticle;
    public Transform hammerThrowParticle;

    public Transform sickle;
    public Transform slanderManKnife;

    public bool activeSickle = false;
    public bool activeSlanderManKnife = false;

    bool canScream = true;

    private bool isPunching = false;

    [SerializeField] private int targetCount = 2;
    [SerializeField] private DateTime forcePlayerTargetedTime = DateTime.MinValue;
    [SerializeField] private int forcePlayerTargetTimer = 0;


    [SerializeField] private DateTime randomStateStartTime = DateTime.MinValue;
    [SerializeField] private float randomStateTime = 0;
    [SerializeField] private DateTime randomStateWaitTime = DateTime.MinValue;
    private bool randomState = false;
    private float randomSpeedMul = -1f;

    TextMeshPro _damage;
    TextMeshPro _hp;


    public void Setting(Define.EnemyType type)
    {
        enemyType = type;

        SetBodySkin();

        bodySpriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, -4);

        if (type == Define.EnemyType.SlanderMan)
        {
            bodySpriteRenderer.transform.localScale = new Vector3(3.8f, 3.5f, 1);
            gameObject.FindRecursive("HpBar").transform.localPosition = new Vector3(0.4f, 6, 0);
        }
        else if (type == Define.EnemyType.TungTungTung || type == Define.EnemyType.Tralalero)
        {
            gameObject.FindRecursive("HpBar").transform.localPosition = new Vector3(0.4f, 4.33f, 0);
        }

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
        mossManSkillParticle = gameObject.FindRecursive("MossManSkillParticle").transform;
        attackSpeedSkillParticle = gameObject.FindRecursive("AttackSpeedParticle").transform;
        attackDamageSkillParticle = gameObject.FindRecursive("AttackDamageParticle").transform;
        hammerThrowParticle = gameObject.FindRecursive("HammerThrowParticle").transform;

        _damage = gameObject.FindRecursive("Damage").GetComponent<TextMeshPro>();
        _hp = gameObject.FindRecursive("Hp").GetComponent<TextMeshPro>();
        StartCoroutine(Check());

        SetNameText(Managers.Game.enemyName);

        MaxHp = Define.GetEnemyMaxHp(enemyType, level);
        hp = MaxHp;

        // 능력치 초기화
        attackSpeed.BaseValue = Define.GetEnemyAttackSpeed(enemyType);
        attackPower.BaseValue = Define.GetEnemyDamage(enemyType, level);
        moveSpeed.BaseValue = baseMoveSpeed;

        skills.Add(new AttackSpeedSkill());
        skills.Add(new AttackDamageSkill());
        skills.Add(new Creepylaughter());
        skills.Add(new MothPowder());

        sickle = gameObject.FindRecursive("Sickle").transform;
        slanderManKnife = gameObject.FindRecursive("SlanderManKnife").transform;

        CheckUseSkill();
        StartCoroutine(CheckHeal());

        forcePlayerTargetedTime = DateTime.Now;
        forcePlayerTargetTimer = UnityEngine.Random.Range(5, 120);

        this.SetListener(GameObserverType.Game.OnCheatModeOn, () =>
        {
            _damage.gameObject.SetActive(Managers.LocalData.CheatMode == 1);
            _hp.gameObject.SetActive(Managers.LocalData.CheatMode == 1);
        });

        _damage.gameObject.SetActive(Managers.LocalData.CheatMode == 1);
        _hp.gameObject.SetActive(Managers.LocalData.CheatMode == 1);

        moveSpeed.onValueChanged += (value) =>
        {
            if (agent != null)
            {
                agent.speed = value;
            }
        };
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
        if (agent != null && agent.hasPath && agent.remainingDistance > 0.01f)
        {
            // 오른쪽 이동: y 회전 0, 왼쪽 이동: y 회전 180
            float dir = agent.steeringTarget.x - agent.transform.position.x;
            if (body != null)
            {
                body.transform.localRotation = Quaternion.Euler(0, dir >= 0 ? 0 : 180, 0);
            }
        }

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
            hp += MaxHp * 0.084f * Time.deltaTime; // Heal 0.84% of MaxHp per second

            hp = Mathf.Clamp(hp, 0, MaxHp); // Ensure hp does not exceed MaxHp

            if (hp >= MaxHp)
            {
                hp = MaxHp;
                targetHealZone = null; // Stop healing when at max HP
                enemyState = EnemyState.Chase; // Switch back to chasing state
                agent.stoppingDistance = 1.5f;
            }

            if (hpBarPivot != null)
            {
                SetHpBar();
            }
        }
        else if (randomStateStartTime.AddSeconds(randomStateTime) <= DateTime.Now)
        {
            if (currentTarget != null && currentTarget.playerData.room == null && enemyState == EnemyState.Chase)
            {
                if (agent != null && agent.isOnNavMesh && currentChaseTarget != currentTarget)
                {
                    currentChaseTarget = currentTarget.transform;
                    agent.SetDestination(currentTarget.transform.position);
                }
            }
            else if (currentTargetStructure != null && enemyState == EnemyState.Chase)
            {
                // Move towards the target
                if (agent != null && agent.isOnNavMesh && currentChaseTarget != currentTargetStructure)
                {
                    currentChaseTarget = currentTargetStructure.transform;
                    agent.SetDestination(currentTargetStructure.transform.position);
                }
            }
        }

        // if (enemyState != EnemyState.Heal)
        // {
        currentChangeStateTime += Time.deltaTime;
        // }

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

    public override void OnMove()
    {
        // if (_moveTween != null && _moveTween.IsActive())
        //     return;

        // _moveTween = bodySpriteRenderer.transform.DOLocalRotate(new Vector3(0, 0, 4), 0.3f)
        //     .SetLoops(-1, LoopType.Yoyo)
        //     .From(new Vector3(0, 0, -4));
    }

    public override void OnMoveStop()
    {

    }

    public IEnumerator EnemyStateMachine()
    {
        yield return new WaitForSeconds(1f);

        randomStateStartTime = DateTime.Now;
        randomStateTime = UnityEngine.Random.Range(0.1f, 6);

        while (true)
        {
            if (randomState && randomSpeedMul != -1)
            {
                moveSpeed.RemoveMultiplier(randomSpeedMul);
                randomSpeedMul = -1f;
            }

            if (randomStateStartTime.AddSeconds(randomStateTime) <= DateTime.Now)
                randomState = false;

            if (IsStunned)
            {
                // NavMeshAgent 즉시 멈춤
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            if (enemyState != EnemyState.Heal)
                if (currentTarget != null && currentTarget.playerData.room == null)
                {
                    float distanceToTarget = Vector2.Distance(agent.transform.position, currentTarget.transform.position);

                    if (canAttack && distanceToTarget < 1.5f) // Example attack range
                    {
                        StartCoroutine(Attack());
                    }
                }
                else if (Managers.Game.charactors.Where(n => !n.die && n.currentActiveRoom == null).Where(n => Vector2.Distance(n.transform.position, transform.position) <= 2.5f)
            .ToList().Count > 0)
                {
                    currentTarget = Managers.Game.charactors.Where(n => !n.die && n.currentActiveRoom == null)
                        .Where(n => Vector2.Distance(n.transform.position, transform.position) <= 2.5f).ToList().First();

                    currentTargetStructure = null;
                }
                else if (randomStateStartTime.AddSeconds(randomStateTime) > DateTime.Now)
                {
                    if (randomStateWaitTime > DateTime.Now)
                    {
                        // 랜덤 상태 대기 중

                    }
                    else
                    {
                        randomState = true;
                        agent.SetDestination(GetRandomPointOnNavMesh());

                        randomSpeedMul = UnityEngine.Random.Range(0.5f, 1f);
                        moveSpeed.AddMultiplier(randomSpeedMul);

                        print("타겟 풀리고 랜덤 상태");

                        randomStateWaitTime = DateTime.Now.AddSeconds(UnityEngine.Random.Range(0.25f, 1.5f));

                    }
                }
                else if (forcePlayerTargetedTime.AddSeconds(forcePlayerTargetTimer) < DateTime.Now)
                {
                    // 타겟 시간으로 인해 타겟을 플레이어로 강제 변경
                    print("타겟 시간으로 인해 타겟을 플레이어로 강제 변경");
                    forcePlayerTargetedTime = DateTime.Now;
                    forcePlayerTargetTimer = UnityEngine.Random.Range(30, 120);

                    // currentChangeStateTime = 0;
                    // changeStateTime = UnityEngine.Random.Range(20, 45);
                    FindTarget(Managers.Game.playerCharactor);
                }
                else if (currentTarget != null && currentTarget.playerData.room != null && changeStateTime > currentChangeStateTime)
                {
                    if (targetCount == 4)
                    {
                        //타겟 카운트가 4여서 강제로 플레이어를 타겟
                        print("타겟 카운트가 4여서 강제로 플레이어를 타겟");

                        FindTarget(Managers.Game.playerCharactor);

                        currentChangeStateTime = 0;
                        changeStateTime = UnityEngine.Random.Range(20, 45);
                        targetCount = 0;

                        randomStateStartTime = DateTime.Now;
                        randomStateTime = UnityEngine.Random.Range(0.1f, 6);
                    }
                    else if (currentTargetStructure == null || currentTargetStructure.destroyed)
                    {
                        if (currentTarget != null && !currentTarget.die)
                            currentTargetStructure = currentTarget.currentActiveRoom.GetAttackableStructure(transform.position);
                        else
                            FindTarget();
                    }
                    else
                    {
                        // Check if the target is within attack range
                        float distanceToTarget = Vector2.Distance(agent.transform.position, currentTargetStructure.transform.position);

                        if (canAttack && distanceToTarget < 1.5f) // Example attack range
                        {
                            StartCoroutine(Attack());
                        }
                    }
                }
                else
                {
                    FindTarget();

                    print("시간 지나서 타겟 랜덤 변경");

                    currentChangeStateTime = 0;
                    changeStateTime = UnityEngine.Random.Range(20, 45);

                    randomStateStartTime = DateTime.Now;
                    randomStateTime = UnityEngine.Random.Range(0.1f, 6);
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

    [Button("Force Target Random")]
    public void FindTarget(PlayerableCharactor forceTarget = null)
    {
        List<PlayerableCharactor> players = new List<PlayerableCharactor>();

        players.AddRange(Managers.Game.charactors.Where(n => !n.die && n.currentActiveRoom != null && currentTarget != n));

        if (players.Count > 0)
        {
            if (forceTarget == null)
            {
                int randomIndex = UnityEngine.Random.Range(0, players.Count);
                currentTarget = players[randomIndex];
            }
            else
                currentTarget = forceTarget;

            List<Room> roomStack = new List<Room>();

            Room currentRoom = currentTarget.currentActiveRoom;

            while (true)
            {
                if (currentRoom.bed.active && !currentRoom.bed.destroyed)
                    roomStack.Add(currentRoom);

                if (currentRoom.parentRoom != null)
                {
                    currentRoom = currentRoom.parentRoom;
                }
                else
                {
                    break;
                }
            }

            if (roomStack.Count > 0)
            {
                currentTarget = roomStack.Last().bed.currentCharactor;
                targetIndex = Managers.Game.charactors.IndexOf(currentTarget);
                currentTargetStructure = currentTarget.currentActiveRoom.GetAttackableStructure(transform.position);
            }

            if (forceTarget == null)
                targetCount++;
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
            int randomIndex = UnityEngine.Random.Range(0, healZones.Count);
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
                currentTargetStructure.Hit(attackPower.Value);

                if (skills.Find(n => n is AttackDamageSkill).IsActive)
                    Managers.Audio.PlaySound("snd_enemy_power_hit", transform, minRangeVolumeMul: 0.4f);
                else
                    Managers.Audio.PlaySound("snd_enemy_hit2", transform, minRangeVolumeMul: 0.4f);

                if (currentExp >= Define.GetEnemyExp(enemyType, level))
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


                    if (canScream)
                    {
                        float value = UnityEngine.Random.Range(0, 1f);

                        var pitch = UnityEngine.Random.Range(0.88f, 1.07f);
                        if (currentTargetStructure.playerData.type == Define.CharactorType.LampGirl)
                        {
                            if (value < 0.5f)
                                Managers.Audio.PlaySound($"snd_girl_scream_{UnityEngine.Random.Range(1, 4)}", minRangeVolumeMul: -1f, pitch: pitch);
                        }
                        else
                        {
                            if (value < 0.5f)
                                Managers.Audio.PlaySound($"snd_scream_{UnityEngine.Random.Range(1, 4)}", minRangeVolumeMul: -1f, pitch: pitch);
                        }

                        StartCoroutine(cooltime());

                        IEnumerator cooltime()
                        {
                            canScream = false;
                            yield return new WaitForSeconds(2f);
                            canScream = true;
                        }
                    }
                }

                if (Managers.UI._currentScene is UI_GameScene_Map gameScene_Map)
                    gameScene_Map.AttackedAnimation(targetIndex);

                if (activeSlanderManKnife)
                {
                    Heal(Mathf.RoundToInt(MaxHp * (0.005f + (level * 0.0005f))));
                }
            };
        }

        if (hitAction != null)
        {
            Vector3 targetDirection = (targetPosition - body.position).normalized;
            Vector3 dashPosition = body.localPosition + targetDirection * 1.35f;

            StartCoroutine(AttackMotion());

            IEnumerator AttackMotion()
            {
                yield return body.DOLocalMove(dashPosition, 0.1f * attackSpeed.Value).SetEase(Ease.Linear).WaitForCompletion();

                hitAction.Invoke();

                yield return body.DOLocalMove(originalPosition, 0.1f * attackSpeed.Value).SetEase(Ease.Linear).WaitForCompletion();
            }
        }

        yield return new WaitForSeconds(attackSpeed.Value);

        canAttack = true;
    }

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
            SetHpBar();
        }

        // Change sprite color to red and smoothly transition back to white using DOTween
        if (bodySpriteRenderer != null && !isHitColorEffectRunning)
        {
            isHitColorEffectRunning = true;
            bodySpriteRenderer.color = Color.red;
            bodySpriteRenderer.DOColor(Color.white, 0.5f).OnComplete(() => isHitColorEffectRunning = false);
        }

        if (bodySpriteRenderer != null)
        {
            // 펀치(흔들림) 효과 추가 - 랜덤 방향
            if (!isPunching)
            {
                isPunching = true;
                // bodySpriteRenderer.transform.DOKill(); // 기존 트윈 중지

                float punchX = UnityEngine.Random.Range(-0.3f, 0.3f);
                float punchY = UnityEngine.Random.Range(-0.3f, 0.3f);
                Vector3 punch = new Vector3(punchX, punchY, 0);

                bodySpriteRenderer.transform.DOPunchPosition(
                    punch,     // 랜덤 방향
                    0.3f,      // 지속 시간
                    15,        // 진동 횟수
                    1f         // 탄성
                ).OnComplete(() => isPunching = false); // 트윈 끝나면 플래그 해제
            }
        }

        if (particle)
            StartCoroutine(Particle());

        IEnumerator Particle()
        {
            var particle = Managers.Resource.Instantiate("Particles/SmokeParticle");

            particle.transform.position = transform.position + new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), UnityEngine.Random.Range(-0.2f, 0.2f), -0.4f);

            yield return new WaitForSeconds(1f);

            Managers.Resource.Destroy(particle);
        }
    }

    void Heal(int amount)
    {
        hp += amount;
        hp = Mathf.Clamp(hp, 0, MaxHp);

        if (hpBarPivot != null)
        {
            SetHpBar();
        }
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

        SetHpBar();

        Managers.UI.ShowNotificationPopup("global.str_toast_enemy_level_up");

        if (enemyType == Define.EnemyType.ScareCrow && level > 4)
        {
            ActiveSicle();
        }

        if (enemyType == Define.EnemyType.SlanderMan && level > 5)
        {
            ActiveSlanderManKnife();
        }

        Managers.Audio.PlaySound("snd_enemy_level_up");
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
                if (distanceToTarget < 2f) // 기존 공격 가능 거리와 동일하게
                    canAttackRange = true;
            }
            else if (currentTargetStructure != null)
            {
                float distanceToTarget = Vector2.Distance(transform.position, currentTargetStructure.transform.position);
                if (distanceToTarget < 2f)
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

    public void ActiveSlanderManKnife()
    {
        activeSlanderManKnife = true;

        if (slanderManKnife != null)
        {
            slanderManKnife.gameObject.SetActive(true);
        }

        attackPower.AddMultiplier(1.1f);
    }

    public void DeactiveSlanderManKnife()
    {
        activeSlanderManKnife = false;

        if (slanderManKnife != null)
        {
            slanderManKnife.gameObject.SetActive(false);
        }

        attackPower.RemoveMultiplier(1.1f);
    }

    public IEnumerator CheckHeal()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 4f));

            if (enemyState != EnemyState.Heal && UnityEngine.Random.Range(0, 100f) < 66.6f && hp <= MaxHp * ((skills.Any(n => n is AttackSpeedSkill) ? 0.20f : 0.28f)))
            {
                FindHealSpot();

                if (targetHealZone != null)
                {
                    // Move towards the heal zone
                    agent.SetDestination(targetHealZone.transform.position);
                    enemyState = EnemyState.Heal;

                    agent.stoppingDistance = 0.1f;

                    if (enemyType == Define.EnemyType.ScareCrow && activeSickle)
                    {
                        DeactiveSicle();
                    }

                    if (enemyType == Define.EnemyType.SlanderMan && activeSlanderManKnife)
                    {
                        DeactiveSlanderManKnife();
                    }
                }
            }
        }
    }

    public Vector3 GetRandomPointOnNavMesh()
    {
        return Managers.Game.currentMap.randomBeacons[UnityEngine.Random.Range(0, Managers.Game.currentMap.randomBeacons.Count)].transform.position;
    }

    public void SetHpBar()
    {
        hpBarPivot.localScale = new Vector3(hp / MaxHp, hpBarPivot.localScale.y, 1);
    }



    IEnumerator Check()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);

            _damage.text = attackPower.Value.ToString("F1");
            _hp.text = $"{Mathf.RoundToInt(hp).ToString()} / {Mathf.RoundToInt(MaxHp).ToString()}";
        }
    }
}
