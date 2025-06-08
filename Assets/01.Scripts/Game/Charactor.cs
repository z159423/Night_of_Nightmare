using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;


public abstract class Charactor : MonoBehaviour
{
    protected Transform body;
    private Tween _moveTween;
    protected SpriteRenderer bodySpriteRenderer;

    protected NavMeshAgent agent;

    protected virtual void Start()
    {
        body = gameObject.FindRecursive("Body").transform;
        agent = gameObject.GetComponentInParent<NavMeshAgent>();

        agent.updateRotation = false;

        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
    }

    public virtual void SetBodySkin()
    {
    }

    public void OnMove()
    {
        if (_moveTween != null && _moveTween.IsActive())
            return;

        _moveTween = body.DOLocalRotate(new Vector3(0, 0, 3), 0.15f)
            .SetLoops(-1, LoopType.Yoyo)
            .From(new Vector3(0, 0, -3));
    }

    public void OnMoveStop()
    {
        if (_moveTween != null && _moveTween.IsActive())
        {
            _moveTween.Kill();
            _moveTween = null;
        }
        body.DOLocalRotate(Vector3.zero, 0.1f);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
    }

    public abstract void Hit(int damage);
}
