using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharactor : Charactor
{
    void Start()
    {
        Managers.Game.playerCharactor = this;
    }
}   
