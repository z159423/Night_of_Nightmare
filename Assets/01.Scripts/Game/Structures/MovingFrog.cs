using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class MovingFrog : Frog
{
    NavMeshAgent agent;
    bool isAttacking = false;
    float attackRange = 4f;

    DOTweenAnimation dOTweenAnimation;

    [SerializeField]
    protected Transform body;

    protected override void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        dOTweenAnimation = GetComponentInChildren<DOTweenAnimation>();
        if (agent != null)
            agent.updateRotation = false;
        StartCoroutine(MoveAndAttackRoutine());
    }

    private IEnumerator MoveAndAttackRoutine()
    {
        while (true)
        {
            var target = Managers.Game.enemy;
            if (target != null && !isAttacking)
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);

                if (distance > attackRange)
                {
                    // 공격 범위 밖이면 target을 따라 이동
                    if (agent != null && agent.isOnNavMesh)
                    {
                        agent.SetDestination(target.transform.position);

                        // 이동 방향을 바라보게
                        if (body != null)
                        {
                            float dir = agent.desiredVelocity.x;
                            float rotY = dir < 0 ? 180f : 0f;
                            Vector3 euler = body.eulerAngles;
                            euler.y = rotY;
                            body.eulerAngles = euler;
                        }
                    }
                }
                else
                {
                    // 공격 범위 안이면 멈추고 공격
                    if (agent != null && agent.isOnNavMesh)
                        agent.SetDestination(transform.position);

                    if (Time.time - lastAttackTime >= attackCooldown)
                    {
                        lastAttackTime = Time.time;
                        if (attackRoutine != null)
                            StopCoroutine(attackRoutine);
                        attackRoutine = StartCoroutine(AttackWithMoveLock(target));
                    }
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    // 공격 중에는 움직이지 않도록 isAttacking으로 제어
    private IEnumerator AttackWithMoveLock(Enemy target)
    {
        isAttacking = true;

        // 공격 중에는 DOTween 애니메이션 중지
        if (dOTweenAnimation != null)
            dOTweenAnimation.DOPause();

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        yield return StartCoroutine(Attack(target));

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = false;

        // 공격 끝나면 DOTween 애니메이션 재생
        if (dOTweenAnimation != null)
            dOTweenAnimation.DOPlay();

        isAttacking = false;
    }
}
