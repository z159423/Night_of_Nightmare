using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheep : Structure
{
    protected override void Start()
    {
        base.Start();
    }

    public void ResourceGetParticle(int value)
    {
        var particle = Managers.Resource.Instantiate("ResourceGetParticle", transform);
        particle.transform.localPosition = Vector3.zero;
        particle.GetComponent<ResourceGetParticle>().Setting(
            "coin",
            value,
            0
        );
    }
}
