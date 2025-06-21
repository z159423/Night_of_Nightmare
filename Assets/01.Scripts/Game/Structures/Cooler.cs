using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cooler : Structure
{
    protected override void Start()
    {
        base.Start();

        playerData.room.door.ActiveCooler();
    }

    public override void DestroyStructure()
    {
        base.DestroyStructure();

        // Remove the thorn bush from the player's room
        if (playerData.room != null)
        {
            playerData.room.door.DeactiveCooler();
        }
    }
}
