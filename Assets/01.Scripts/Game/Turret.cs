using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : Structure
{
    public Transform target;
    public float attackRange = 5f;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    GameObject body;
    GameObject head;

    protected override void Start()
    {
        base.Start();
        lastAttackTime = Time.time;

        body = gameObject.FindRecursive("Body");
        head = gameObject.FindRecursive("Head");
    }

    private void Update()
    {
        if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }

    private void Attack()
    {
        // 공격 로직 구현
        Debug.Log("Attacking target: " + target.name);
    }

    public override void Upgrade()
    {
        
    }
}
