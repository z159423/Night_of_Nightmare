using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTurret : Turret
{

    protected override void Start()
    {
        // Turret의 Start 호출
        base.Start();

        attackCooldown = 0.5f;
    }

    protected override void Attack()
    {
        base.Attack();
    }
}   
