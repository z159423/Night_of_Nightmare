using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;

public class Enemy : Charactor
{
    public enum EnemyState
    {
        Chase,
        Heal
    }

    public const float MaxHp = 100;
    public float hp = 100;

    public PlayerableCharactor currentTarget;
    public HealZone targetHealZone;
    EnemyState enemyState = EnemyState.Chase;

    public bool canAttack = true;

    private int changeStateTime = 0;
    private float currentChangeStateTime = 0;

    private float checkHpTime = 0;
    private float currentCheckHpTime = 0;


    // Implementation of the abstract Hit() method from Charactor
    protected override void Hit(int damage)
    {
        // Provide your logic here, for example:
        hp -= damage; // Default damage
        if (hp <= 0)
        {
            // Handle enemy death
        }
    }

    public void Setting()
    {
        SetBodySkin();

        transform.localPosition = Vector3.zero;

        StartCoroutine(EnemyStateMachine());

        GetComponentInParent<NavMeshAgent>(true).enabled = true;
    }

    void Update()
    {
        if (enemyState == EnemyState.Heal && targetHealZone != null && Vector2.Distance(targetHealZone.transform.position, transform.position) < 1.5f)
        {
            hp += MaxHp * 0.0014f; // Heal 0.14% of MaxHp per frame

            hp = Mathf.Clamp(hp, 0, MaxHp); // Ensure hp does not exceed MaxHp

            if (hp >= MaxHp)
            {
                hp = MaxHp;
                targetHealZone = null; // Stop healing when at max HP
                enemyState = EnemyState.Chase; // Switch back to chasing state
            }
        }
        else if (currentTarget != null && enemyState == EnemyState.Chase)
        {
            // Move towards the target
            agent.SetDestination(currentTarget.transform.position);
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
                else if (currentTarget != null && changeStateTime > currentChangeStateTime)
                {
                    // Check if the target is within attack range
                    float distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);

                    if (canAttack && distanceToTarget < 1.5f) // Example attack range
                    {
                        StartCoroutine(Attack());
                    }

                    if (currentCheckHpTime > checkHpTime)
                        checkHpTime = Random.Range(0.5f, 4f);
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
        bodySpriteRenderer.sprite = Managers.Resource.GetEnemyImage(Random.Range(1, 6));
    }

    public void FindTarget()
    {
        List<PlayerableCharactor> players = new List<PlayerableCharactor>();

        players.AddRange(Managers.Game.aiCharactors.Where(n => !n.die));
        players.Add(Managers.Game.playerCharactor);

        if (players.Count > 0)
        {
            int randomIndex = Random.Range(0, players.Count);
            currentTarget = players[randomIndex];


        }
        else
        {
            currentTarget = null;
        }
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

        if (currentTarget != null)
        {
            Vector3 originalPosition = body.localPosition;
            Vector3 targetDirection = (currentTarget.transform.localPosition - body.localPosition).normalized;
            Vector3 dashPosition = body.localPosition + targetDirection * 1.5f;

            // 몸통박치기 애니메이션 (DoTween)
            yield return body.DOLocalMove(dashPosition, 0.1f).SetEase(Ease.Linear).WaitForCompletion();
            yield return body.DOLocalMove(originalPosition, 0.1f).SetEase(Ease.Linear).WaitForCompletion();
        }

        yield return new WaitForSeconds(1.5f); // Attack delay

        canAttack = true;
    }
}
