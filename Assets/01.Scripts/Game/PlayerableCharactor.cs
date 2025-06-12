using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class PlayerableCharactor : Charactor
{
    public Bed currentFindBed;
    //현재 있는 방
    public Room currentActiveRoom;

    public Define.CharactorType charactorType;

    public PlayerData playerData;

    public bool die = false;

    

    public override void SetBodySkin()
    {
        if (bodySpriteRenderer == null)
        {
            bodySpriteRenderer = gameObject.FindRecursive("Icon").GetComponent<SpriteRenderer>();
        }
        bodySpriteRenderer.sprite = Managers.Resource.GetCharactorImage((int)charactorType + 1);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Bed>(out Bed bed))
        {
            if (!bed.active)
            {
                currentActiveRoom = bed.OnActive(this);
                gameObject.SetActive(false);

                playerData.room = currentActiveRoom;

                playerData.structures.Add(currentActiveRoom.bed);
                playerData.structures.Add(currentActiveRoom.door);
            }
        }
    }

    public abstract void Die();
}
