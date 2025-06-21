using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornBush : Structure
{
    protected override void Start()
    {
        base.Start();

        playerData.room.door.ActiveThornBush();
    }

    public override void DestroyStructure()
    {
        base.DestroyStructure();

        // Remove the thorn bush from the player's room
        if (playerData.room != null)
        {
            playerData.room.door.DeactiveThornBush();
        }
    }
}
