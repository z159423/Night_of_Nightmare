using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldenChest : Structure
{
    protected override void Start()
    {
        base.Start();
    }

    public override void Upgrade()
    {
        base.Upgrade();

        GetComponentInChildren<SpriteRenderer>().sprite = Managers.Game.GetStructureData(Define.StructureType.GoldenChest).sprite1[level];
    }
}
