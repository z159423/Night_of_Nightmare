using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ore : Structure
{
    protected override void Start()
    {
        base.Start();
    }

    public override void Upgrade()
    {
        base.Upgrade();
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        switch (type)
        {
            case Define.StructureType.CopperOre:
            spriteRenderer.sprite = Managers.Game.GetStructureData(Define.StructureType.CopperOre).sprite1[level];
            break;
            case Define.StructureType.SilverOre:
            spriteRenderer.sprite = Managers.Game.GetStructureData(Define.StructureType.SilverOre).sprite1[level];
            break;
            case Define.StructureType.GoldOre:
            spriteRenderer.sprite = Managers.Game.GetStructureData(Define.StructureType.GoldOre).sprite1[level];
            break;
            case Define.StructureType.DiamondOre:
            spriteRenderer.sprite = Managers.Game.GetStructureData(Define.StructureType.DiamondOre).sprite1[level];
            break;
        }
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
