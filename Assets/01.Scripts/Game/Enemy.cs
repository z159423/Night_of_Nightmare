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

    public Transform sickle;
    public Transform slanderManKnife;

    public bool activeSickle = false;
    public bool activeSlanderManKnife = false;

    bool canScream = true;

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

        if (bodySpriteRenderer != null)
        {
            // 펀치(흔들림) 효과 추가 - 랜덤 방향
            bodySpriteRenderer.transform.DOKill(); // 기존 트윈 중지

            // 0.12~0.18 사이의 랜덤 세기, 방향도 랜덤
            float punchX = Random.Range(-0.3f, 0.3f);
            float punchY = Random.Range(-0.3f, 0.3f);
            Vector3 punch = new Vector3(punchX, punchY, 0);

            bodySpriteRenderer.transform.DOPunchPosition(
                punch,     // 랜덤 방향
                0.3f,      // 지속 시간
                15,        // 진동 횟수
                1f         // 탄성
            );
        }

        if (particle)
            StartCoroutine(Particle());

        IEnumerator Particle()
        {
            var particle = Managers.Resource.Instantiate("Particles/SmokeParticle");

            particle.transform.position = transform.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), -0.4f);

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
            hpBarPivot.localScale = new Vector3(hp / MaxHp, 1, 1);
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
        mossManSkillParticle = gameObject.FindRecursive("MossManSkillParticle").transform;

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
        StartCoroutine(CheckHeal());
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
            hp += MaxHp * 0.084f * Time.deltaTime; // Heal 0.84% of MaxHp per second

            hp = Mathf.Clamp(hp, 0, MaxHp); // Ensure hp does not exceed MaxHp

            if (hp >= MaxHp)
            {
                hp = MaxHp;
                targetHealZone = null; // Stop healing when at max HP
                enemyState = EnemyState.Chase; // Switch back to chasing state
                agent.stoppingDistance = 0.8f;
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

    public override void OnMove()
    {
        if (_moveTween != null && _moveTween.IsActive())
            return;

        _moveTween = body.DOLocalRotate(new Vector3(0, 0, 4), 0.3f)
            .SetLoops(-1, LoopType.Yoyo)
            .From(new Vector3(0, 0, -4));
    }

    public override void OnMoveStop()
    {

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
                if (currentTarget != null && currentTarget.playerData.room == null)
                {
                    float distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);

                    if (canAttack && distanceToTarget < 1f) // Example attack range
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

    [Button("Force Target Random")]
    public void FindTarget()
    {
        List<PlayerableCharactor> players = new List<PlayerableCharactor>();
        // List<PlayerableCharactor> noBedPlayers = new List<PlayerableCharactor>();    

        // noBedPlayers.AddRange(Managers.Game.charactors.Where(n => !n.die && n.currentActiveRoom == null));
        players.AddRange(Managers.Game.charactors.Where(n => !n.die && n.currentActiveRoom != null && currentTarget != n));

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
                        float value = Random.Range(0, 1f);

                        var pitch = Random.Range(1.0f, 1.3f);
                        if (currentTargetStructure.playerData.type == Define.CharactorType.LampGirl)
                        {
                            if (value < 0.25f)
                                Managers.Audio.PlaySound("snd_girl_scream_1", minRangeVolumeMul: -1f, pitch: pitch);
                            else if (value < 0.5f && value >= 0.25f)
                                Managers.Audio.PlaySound("snd_girl_scream_2", minRangeVolumeMul: -1f, pitch: pitch);
                            else if (value < 0.625f && value >= 0.5f)
                                Managers.Audio.PlaySound("snd_girl_scream_3", minRangeVolumeMul: -1f, pitch: pitch);

                        }
                        else
                        {
                            if (value < 0.25f)
                                Managers.Audio.PlaySound("snd_scream_1", minRangeVolumeMul: -1f, pitch: pitch);
                            else if (value < 0.5f && value >= 0.25f)
                                Managers.Audio.PlaySound("snd_scream_2", minRangeVolumeMul: -1f, pitch: pitch);
                            else if (value < 0.625f && value >= 0.5f)
                                Managers.Audio.PlaySound("snd_scream_3", minRangeVolumeMul: -1f, pitch: pitch);
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
            Vector3 dashPosition = body.localPosition + targetDirection * 1.5f;

            yield return body.DOLocalMove(dashPosition, 0.1f / attackSpeed.Value).SetEase(Ease.Linear).WaitForCompletion();

            hitAction.Invoke();

            yield return body.DOLocalMove(originalPosition, 0.1f / attackSpeed.Value).SetEase(Ease.Linear).WaitForCompletion();
        }

        yield return new WaitForSeconds(0.75f / attackSpeed.Value);

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

        Managers.UI.ShowNotificationPopup("global.str_toast_enemy_level_up");

        if (enemyType == Define.EnemyType.ScareCrow && level > 4)
        {
            ActiveSicle();
        }

        if (enemyType == Define.EnemyType.SlanderMan && level > 5)
        {
            ActiveSlanderManKnife();
        }

        Managers.Audio.PlaySound("snd_enemy_level_up", minRangeVolumeMul: 0.4f);
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
            yield return new WaitForSeconds(Random.Range(0.5f, 4f));

            if (enemyState != EnemyState.Heal && Random.Range(0, 100) < 66 && hp < MaxHp * (skills.Any(n => n is AttackSpeedSkill) ? 0.20f : 0.28f))
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
}
