using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonFrog : Frog
{
    Transform tongueParticle;

    protected override void Start()
    {
        base.Start();

        // Set the frog's color to green
        tongueParticle = gameObject.FindRecursive("TongueParticle").transform;

        attackStartAction = () =>
        {
            // Play the tongue particle effect
            if (tongueParticle != null)
            {
                tongueParticle.GetComponent<ParticleSystem>().Play();
            }
        };

        attackEndAction = () =>
        {
            // Stop emitting new particles, but keep existing ones alive
            if (tongueParticle != null)
            {
                var ps = tongueParticle.GetComponent<ParticleSystem>();
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        };

        attackHitAction = (enemy) =>
        {
            // Apply poison effect to the enemy
            if (enemy != null)
            {
                enemy.AddEffect(new PoisonEffect());
            }
        };

    }
}
