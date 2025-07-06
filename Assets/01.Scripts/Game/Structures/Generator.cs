using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : Structure
{
    protected override void Start()
    {
        base.Start();

        upgradePercent = 25f;

    }

    public override void Upgrade()
    {
        base.Upgrade();

        SetBodySprite();
    }

    public override void UpgradeTo(int levelTo)
    {
        base.UpgradeTo(levelTo);

        SetBodySprite();
    }

    public override void SetBodySprite()
    {
        GetComponentInChildren<SpriteRenderer>().sprite = Managers.Game.GetStructureData(Define.StructureType.Generator).sprite1[level];
    }

    public void ResourceGetParticle(int value)
    {
        var particle = Managers.Resource.Instantiate("ResourceGetParticle", transform);
        particle.transform.localPosition = Vector3.zero;
        particle.GetComponent<ResourceGetParticle>().Setting(
            "energy",
            value,
            0
        );
    }
}
