using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldenMirror : Structure
{
    private LineRenderer lineRenderer;
    private Enemy target;
    private Coroutine damageRoutine;

    bool started = false;

    protected override void Start()
    {
        base.Start();

        lineRenderer = gameObject.FindRecursive("MirrorLine").GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position);
        }

        damageRoutine = StartCoroutine(DamageEnemyRoutine());
    }

    protected override void Update()
    {
        base.Update();
        target = Managers.Game.enemy;
        if (lineRenderer != null && target != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, target.transform.position + (Vector3.up * 0.5f));
        }
        else if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position);
        }
    }

    private IEnumerator DamageEnemyRoutine()
    {
        while (true)
        {
            if (Managers.Game.enemy != null && !started)
            {
                Managers.Game.enemy.mirrorParticle.GetComponent<ParticleSystem>().Play();
                started = true;
            }

            yield return new WaitForSeconds(0.5f);

            target = Managers.Game.enemy;
            if (target != null)
            {
                int damage = Mathf.Max(1, Mathf.RoundToInt(target.MaxHp * 0.015f));
                target.Hit(damage, false);
            }
        }
    }
}
